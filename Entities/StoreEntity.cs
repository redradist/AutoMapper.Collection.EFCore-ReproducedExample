using System.Collections.Generic;

namespace AutoMapper.Collection.EFCore.Reproduce.Entities
{
    public class StoreEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<StoreItemEntity> Items { get; set; } = new List<StoreItemEntity>();
    }
}