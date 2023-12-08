using Prism.Mvvm;

namespace XMagnetSearch.UI.Models
{
    public class PluginModel: BindableBase
    {
        private string _url = string.Empty;
        private bool _enable = false;
        private bool _selected = false;
        public string Url { get => _url; set => SetProperty(ref _url, value); }
        public bool Enable { get => _enable; set => SetProperty(ref _enable, value); }
        public bool Selected { get => _selected; set => SetProperty(ref _selected, value); }
    }
}
