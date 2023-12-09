﻿using Prism.Mvvm;

namespace XMagnetSearch.UI.Models
{
    public class PluginModel: BindableBase
    {
        private string _url = string.Empty;
        private string _name = string.Empty;
        private bool _enable = false;
        private bool _selected = false;
        public string Url { get => _url; set => SetProperty(ref _url, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public bool Enable { get => _enable; set => SetProperty(ref _enable, value); }
        public bool Selected { get => _selected; set => SetProperty(ref _selected, value); }

        public PluginModel()
        {
        }       

        public PluginModel(string url, bool enable)
        {
            Url = url;
            Enable = enable;
        }
        public PluginModel(string url, string name, bool enable) : this(url,enable)
        {
            Name = name;         
        }
    }
}