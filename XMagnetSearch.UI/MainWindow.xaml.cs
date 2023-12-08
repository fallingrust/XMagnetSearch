﻿using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using XMagnetSearch.UI.Models;
using XMagnetSearch.UI.ViewModel;
using XMagnetSearch.UI.Views.Dialog;

namespace XMagnetSearch.UI
{
    public partial class MainWindow : Window
    {
        [ImportMany]
        public IEnumerable<Lazy<ISearch, IMetadata>>? Plugins { get; set; }
        private CompositionContainer? _container = null;
        private int _page = 0;
        public MainWindow()
        {
            InitializeComponent();
            tb_search.PreviewKeyDown += OnSerachKeyDown;
            RegisterPluginAsync();
            sc.ScrollChanged += OnScrollChanged;
        }
        
        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer sc && sc.ViewportHeight + sc.VerticalOffset >= sc.ExtentHeight && !string.IsNullOrWhiteSpace(tb_search.Text))
            {
                LoadSearchs(tb_search.Text);
            }
        }

        private void OnSerachKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox tb && e.Key == Key.Enter && tb.IsKeyboardFocused)
            {               
                if (!string.IsNullOrWhiteSpace(tb.Text))
                {
                    if (DataContext is MainVM vm)
                    {
                        vm.Searchs.Clear();
                    }
                    _page = 0;
                    Grid.SetRowSpan(grid_search, 1);
                    svg_logo_full.Visibility = Visibility.Collapsed;
                    LoadSearchs(tb.Text);
                }
                e.Handled = true;
            }
        }

        private void LoadSearchs(string input)
        {
            DialogHost.Show(new SearchingDialogContent(), "RootDialog");
            Task.Run(async () =>
            {
                _page += 1;
                if (Plugins != null)
                {
                    var tasks = new List<Task<IEnumerable<SearchBean>>>();
                    foreach (var plugin in Plugins)
                    {
                        tasks.Add(plugin.Value.SearchAsync(input, _page));                        
                    }
                    Task.WaitAll([.. tasks]);
                    var results = new List<SearchBean>();
                    foreach (var task in tasks)
                    {
                        try
                        {
                            results.AddRange(await task);
                        }
                        catch { }                      
                    }
                    UpdateSearchs(results);
                }
            });           
        }
        private void UpdateSearchs(IEnumerable<SearchBean> searchBeans)
        {
            if (!CheckAccess())
            {
                Dispatcher.BeginInvoke(DispatcherPriority.DataBind, () => UpdateSearchs(searchBeans));
                return;
            }
            if (DialogHost.IsDialogOpen("RootDialog"))
            {
                DialogHost.Close("RootDialog");
            }
            if (DataContext is MainVM vm)
            {
                foreach (var bean in searchBeans)
                {
                    if (!vm.Searchs.Any(p => p.MagnetUrl == bean.MagnetUrl))
                    {
                        vm.Searchs.Add(SearchModel.Converter(bean));
                    }
                }
            }          
        }
        private void UpdatePluginss(IEnumerable<PluginModel> pluginModels)
        {
            if (!CheckAccess())
            {
                Dispatcher.BeginInvoke(DispatcherPriority.DataBind, () => UpdatePluginss(pluginModels));
                return;
            }
            if (DialogHost.IsDialogOpen("RootDialog"))
            {
                DialogHost.Close("RootDialog");
            }
            if (DataContext is MainVM vm)
            {
                vm.Plugins.AddRange(pluginModels);
            }
        }
        private async void RegisterPluginAsync()
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            if (dir.Exists)
            {
                var catalog = new DirectoryCatalog(dir.FullName, "*.dll");
                _container = new CompositionContainer(catalog);
                try
                {
                    _container.ComposeParts(this);
                    var pluginModels = new List<PluginModel>();
                    foreach(var plugin in Plugins)
                    {
                        var enable = await ISearch.CheckEnableAsync(plugin.Metadata.Source);
                        pluginModels.Add(new PluginModel(plugin.Metadata.Source, plugin.Metadata.Description, enable));
                    }
                    UpdatePluginss(pluginModels);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _container?.Dispose();
        }
        DateTime _lastDownTime;
        private void OnSearchMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement container && container.DataContext is SearchModel searchModel)
            {
                if (DateTime.Now - _lastDownTime <= TimeSpan.FromMilliseconds(300))
                {
                    Clipboard.SetText($"magnet:?xt=urn:btih:{searchModel.MagnetUrl}");
                    Snackbar.MessageQueue?.Clear();
                    Snackbar.MessageQueue?.Enqueue("链接已复制");
                }              
            }
            _lastDownTime = DateTime.Now;
        }
    }
}