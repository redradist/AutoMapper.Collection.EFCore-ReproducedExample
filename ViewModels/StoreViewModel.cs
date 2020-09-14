using System.Collections.Generic;

namespace AutoMapper.Collection.EFCore.Reproduce.ViewModels
{
    public class StoreViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<ItemViewModel> Items { get; set; } = new List<ItemViewModel>();
    }
}