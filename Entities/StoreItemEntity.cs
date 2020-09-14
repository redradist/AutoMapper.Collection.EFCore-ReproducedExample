namespace AutoMapper.Collection.EFCore.Reproduce.Entities
{
    public class StoreItemEntity
    {
        public int ItemId { get; set; }
        public ItemEntity Item { get; set; }
        public int StoreId { get; set; }
        public StoreEntity Store { get; set; }
    }
}