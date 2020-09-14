using System.Collections.Generic;

namespace AutoMapper.Collection.EFCore.Reproduce.ViewModels
{
    public class ItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<StoreViewModel> Stores { get; set; } = new List<StoreViewModel>();
    }
}