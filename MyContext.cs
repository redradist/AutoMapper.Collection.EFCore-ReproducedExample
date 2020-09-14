using AutoMapper.Collection.EFCore.Reproduce.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoMapper.Collection.EFCore.Reproduce
{
    public class MyContext : DbContext
    {
        public MyContext() { }

        public DbSet<ItemEntity> Items { get; set; }
        public DbSet<StoreEntity> Stores { get; set; }
        public DbSet<StoreItemEntity> StoreItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            string ipAddress = "localhost";
            short port = 5432;
            optionsBuilder.UseNpgsql($"Host={ipAddress};Port={port};Database=tests;Username=postgres;Password=postgres");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ItemEntity>(entity =>
            {
                entity.Property(i => i.Name).IsRequired();
                entity.HasIndex(i => i.Name).IsUnique();
            });
            
            builder.Entity<StoreEntity>(entity =>
            {
                entity.Property(i => i.Name).IsRequired();
                entity.HasIndex(i => i.Name).IsUnique();
            });
            
            builder.Entity<StoreItemEntity>(entity =>
            {
                entity
                    .HasKey(i => new { i.ItemId, i.StoreId });
                entity
                    .HasOne(l => l.Item)
                    .WithMany(s => s.Stores)
                    .HasForeignKey(l => l.ItemId);
                entity
                    .HasOne(s => s.Store)
                    .WithMany(s => s.Items)
                    .HasForeignKey(s => s.StoreId); 
            });
            
            // Description of Stuffs
            builder.HasSequence<int>("stores_id_seq");
            builder.Entity<StoreEntity>()
                .Property(s => s.Id)
                .HasDefaultValueSql("nextval('stores_id_seq'::regclass)");
            
            builder.HasSequence<int>("items_id_seq");
            builder.Entity<ItemEntity>()
                .Property(s => s.Id)
                .HasDefaultValueSql("nextval('items_id_seq'::regclass)");
        }
    }
}
