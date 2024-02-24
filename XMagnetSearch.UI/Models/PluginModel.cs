using Prism.Mvvm;

namespace XMagnetSearch.UI.Models
{
    public class PluginModel: BindableBase
    {
        private string _url = string.Empty;
        private string _name = string.Empty;
        private bool _enable = false;
        private bool _selected = false;
        private long _ttl = long.MinValue;
        public string Url { get => _url; set => SetProperty(ref _url, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public bool Enable { get => _enable; set => SetProperty(ref _enable, value); }
        public bool Selected { get => _selected; set => SetProperty(ref _selected, value); }

        public long TTL { get => _ttl; set => SetProperty(ref _ttl, value); }
        public PluginModel()
        {
        }       

        public PluginModel(string url, bool enable)
        {
            Url = url;
            Enable = enable;
            Selected = enable;
        }
        public PluginModel(string url, string name, bool enable) : this(url,enable)
        {
            Name = name;         
        }
        public PluginModel(string url, string name, bool enable, long ttl) : this(url, name, enable)
        {
           TTL = ttl;
        }
    }
}
