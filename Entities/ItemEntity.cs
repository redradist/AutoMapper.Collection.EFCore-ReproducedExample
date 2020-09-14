using System.Collections.Generic;

namespace AutoMapper.Collection.EFCore.Reproduce.Entities
{
    public class ItemEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<StoreItemEntity> Stores { get; set; } = new List<StoreItemEntity>();
    }
}