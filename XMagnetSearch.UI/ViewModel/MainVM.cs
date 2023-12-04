using Prism.Mvvm;
using System.Collections.ObjectModel;
using XMagnetSearch.UI.Models;

namespace XMagnetSearch.UI.ViewModel
{
    public class MainVM : BindableBase
    {
        public ObservableCollection<SearchModel> Searchs { get; set; } = [];
        public MainVM()
        {
            for (int i = 0; i < 30; i++)
            {
                Searchs.Add(new SearchModel() 
                { 
                    Title = DateTime.Now.ToString(), 
                    From = DateTime.Now.ToString(),
                    Size = DateTime.Now.ToString()
                });
            }
        }
    }
}
