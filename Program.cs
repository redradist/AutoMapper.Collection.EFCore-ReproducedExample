using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.Collection.EFCore.Reproduce.Entities;
using AutoMapper.Collection.EFCore.Reproduce.ViewModels;
using AutoMapper.EntityFrameworkCore;
using AutoMapper.EquivalencyExpression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AutoMapper.Collection.EFCore.Reproduce
{
    class Program : IDesignTimeDbContextFactory<MyContext>
    {
        public MyContext CreateDbContext(string[] args)
        {
            string ipAddress = "localhost";
            short port = 5432;
            DbContextOptionsBuilder<MyContext> optionsBuilder = new DbContextOptionsBuilder<MyContext>()
                .UseNpgsql($"Host={ipAddress};Port={port};Database=tests;Username=postgres;Password=postgres");
            
            return new MyContext();
        }
        
        static async Task Main(string[] args)
        {
            var program = new Program();
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddCollectionMappers();
                cfg.CreateMap<StoreViewModel, StoreEntity>()
                    .EqualityComparison((odto, o) => odto.Id == o.Id)
                    .ForMember(d => d.Items,
                        opt => opt.MapFrom(s => s.Items))
                    .AfterMap((model, entity) =>
                    {
                        foreach (var entityItem in entity.Items)
                        {
                            entityItem.StoreId = entity.Id;
                            entityItem.Store = entity;
                            entityItem.ItemId = entityItem.Item.Id;
                        }
                    });
                cfg.CreateMap<StoreEntity, StoreViewModel>()
                    .EqualityComparison((odto, o) => odto.Id == o.Id)
                    .ForMember(d => d.Items,
                        opt => opt.MapFrom(s => s.Items.Select(y => y.Item).ToList()))
                    .AfterMap((_, place) =>
                    {
                        var existedItems = new HashSet<long>();
                        var removeItems = new List<ItemViewModel>();
                        foreach (var stuff in place.Items)
                        {
                            if (existedItems.Contains(stuff.Id))
                            {
                                removeItems.Add(stuff);
                            }
                            else
                            {
                                existedItems.Add(stuff.Id);
                            }
                        }
                        foreach (var removeItem in removeItems)
                        {
                            place.Items.Remove(removeItem);
                        }
                    });
                
                cfg.CreateMap<ItemViewModel, ItemEntity>()
                    .EqualityComparison((odto, o) => odto.Id == o.Id)
                    .ForMember(d => d.Stores,
                        opt => opt.MapFrom(s => s.Stores))
                    .AfterMap((model, entity) =>
                    {
                        foreach (var entityStore in entity.Stores)
                        {
                            entityStore.ItemId = entity.Id;
                            entityStore.Item = entity;
                            entityStore.StoreId = entityStore.Store.Id;
                        }
                    });
                cfg.CreateMap<ItemEntity, ItemViewModel>()
                    .EqualityComparison((odto, o) => odto.Id == o.Id)
                    .ForMember(d => d.Stores,
                        opt => opt.MapFrom(s => s.Stores.Select(y => y.Store).ToList()))
                    .AfterMap((_, place) =>
                    {
                        var existedItems = new HashSet<long>();
                        var removeItems = new List<StoreViewModel>();
                        foreach (var stuff in place.Stores)
                        {
                            if (existedItems.Contains(stuff.Id))
                            {
                                removeItems.Add(stuff);
                            }
                            else
                            {
                                existedItems.Add(stuff.Id);
                            }
                        }
                        foreach (var removeItem in removeItems)
                        {
                            place.Stores.Remove(removeItem);
                        }
                    });
                
                cfg.CreateMap<StoreViewModel, StoreItemEntity>()
                    .EqualityComparison((odto, o) => odto.Id == o.StoreId && odto.Items.Any(model => model.Id == o.ItemId))
                    .ForMember(entity => entity.StoreId, opt => opt.MapFrom(model => model.Id))
                    .ForMember(entity => entity.Store, opt => opt.MapFrom(model => model))
                    .ForMember(entity => entity.ItemId, opt => opt.Ignore())
                    .ForMember(entity => entity.Item, opt => opt.Ignore());
                cfg.CreateMap<StoreItemEntity, StoreViewModel>() // < -- Issue
                    .EqualityComparison((odto, o) => odto.StoreId == o.Id && o.Items.Any(model => model.Id == odto.ItemId))
                    .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Store.Id))
                    .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Store.Name))
                    .ForMember(x => x.Items, opt => opt.MapFrom(src => new List<ItemEntity>()
                    {
                        src.Item
                    }));
                
                cfg.CreateMap<ItemViewModel, StoreItemEntity>()
                    .EqualityComparison((odto, o) => odto.Id == o.ItemId && odto.Stores.Any(model => model.Id == o.StoreId))
                    .ForMember(entity => entity.StoreId, opt => opt.Ignore())
                    .ForMember(entity => entity.Store, opt => opt.Ignore())
                    .ForMember(entity => entity.ItemId, opt => opt.MapFrom(model => model.Id))
                    .ForMember(entity => entity.Item, opt => opt.MapFrom(model => model));
                cfg.CreateMap<StoreItemEntity, ItemViewModel>() // < -- Issue
                    .EqualityComparison((odto, o) => odto.ItemId == o.Id && o.Stores.Any(model => model.Id == odto.StoreId))
                    .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Item.Id))
                    .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Item.Name))
                    .ForMember(x => x.Stores, opt => opt.MapFrom(src => new List<StoreEntity>()
                    {
                        src.Store
                    }));
                
            });
#if DEBUG
            configuration.AssertConfigurationIsValid();
#endif
            IMapper mapper = configuration.CreateMapper();
            
            using (var context = program.CreateDbContext(null))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var storeEntity = new StoreEntity()
                {
                    Name = "Store0",
                };
                await context.Stores.AddAsync(storeEntity);
                await context.SaveChangesAsync();
                var store = mapper.Map<StoreViewModel>(storeEntity);

                var itemEntity = new ItemEntity()
                {
                    Name = "Item0",
                };
                await context.Items.AddAsync(itemEntity);
                await context.SaveChangesAsync();
                var item = mapper.Map<ItemViewModel>(itemEntity);

                store.Items.Add(item);
                item.Stores.Add(store);
                var storeEntity2 = mapper.Map<StoreEntity>(store);
                await context.Stores.Persist(mapper).InsertOrUpdateAsync(store);
                await context.SaveChangesAsync();
            }
        }
    }
}
