using Prism.Mvvm;
using System.Collections.ObjectModel;
using XMagnetSearch.UI.Models;

namespace XMagnetSearch.UI.ViewModel
{
    public class MainVM : BindableBase
    {
        public ObservableCollection<SearchModel> Searchs { get; set; } = [];
        public ObservableCollection<PluginModel> Plugins { get; set; } = [];
        public MainVM()
        {
            
        }
    }
}
