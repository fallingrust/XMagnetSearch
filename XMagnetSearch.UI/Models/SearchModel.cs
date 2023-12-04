using Prism.Mvvm;

namespace XMagnetSearch.UI.Models
{
    public class SearchModel : BindableBase
    {
        private string _title = string.Empty;
        private string _magnetUrl= string.Empty;
        private string _size = string.Empty;
        private string _from = string.Empty;

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string MagnetUrl { get => _magnetUrl; set => SetProperty(ref _magnetUrl, value); }
        public string Size { get => _size; set => SetProperty(ref _size, value); }
        public string From { get => _from; set => SetProperty(ref _from, value); }
    }
}
