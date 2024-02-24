using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Runtime.InteropServices;
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
        private DateTime _lastDownTime;
        public MainWindow()
        {
            InitializeComponent();
            tb_search.PreviewKeyDown += OnSerachKeyDown;
            sc.ScrollChanged += OnScrollChanged;
            RootDialog.Loaded += OnRootDialogLoaded;
#if DEBUG
            AllocConsole();
#endif
        }

        private void OnRootDialogLoaded(object sender, RoutedEventArgs e)
        {
            _ = DialogHost.Show(new SearchingDialogContent(), "RootDialog");
            _ = RegisterPluginAsync();
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
                    sc.ScrollToVerticalOffset(0);
                    if (DataContext is MainVM vm)
                    {
                        vm.Searchs.Clear();
                    }
                    _page = 0;
                    Grid.SetRow(grid_search, 0);
                    svg_logo_full.Visibility = Visibility.Collapsed;
                    sc.Visibility = Visibility.Visible;
                    LoadSearchs(tb.Text);
                }
                e.Handled = true;
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _container?.Dispose();
        }
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

        private void OnSourceClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainVM vm)
            {
                DialogHost.Show(new SourceChangeDialogContent(vm.Plugins), "ChildDialog");
            }
        }
        private void LoadSearchs(string input)
        {
            DialogHost.Show(new SearchingDialogContent(), "RootDialog");
            List<string>? selectePlugins = null;
            if (DataContext is MainVM vm)
            {
                selectePlugins = vm.Plugins.Where(p => p.Selected).Select(p => p.Url).ToList();
            }
            if (selectePlugins != null && selectePlugins.Count > 0)
            {
                Task.Run(async () =>
                {
                    _page += 1;
                    if (Plugins != null)
                    {
                        var tasks = new List<Task<IEnumerable<SearchBean>>>();
                        foreach (var plugin in Plugins)
                        {
                            if (selectePlugins.Contains(plugin.Metadata.Source))
                            {
                                tasks.Add(plugin.Value.SearchAsync(input, _page));
                            }
                        }
                        Task.WaitAll(tasks.ToArray());
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
        private void UpdatePlugins(IEnumerable<PluginModel> pluginModels)
        {
            if (!CheckAccess())
            {
                Dispatcher.BeginInvoke(DispatcherPriority.DataBind, () => UpdatePlugins(pluginModels));
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
            Snackbar.MessageQueue?.Clear();
            Snackbar.MessageQueue?.Enqueue("搜索源已更新");
        }
        private async Task RegisterPluginAsync()
        {
            var dir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins"));
            if (dir.Exists)
            {
                
                var dirCatalogs = new List<DirectoryCatalog>();
                foreach(var pluginDir  in dir.GetDirectories())
                {
                    var catalog = new DirectoryCatalog(pluginDir.FullName, "*.dll");
                    dirCatalogs.Add(catalog);
                }
                var catalogs = new AggregateCatalog(dirCatalogs);

                _container = new CompositionContainer(catalogs);
                try
                {
                    _container.ComposeParts(this);
                    var pluginModels = new List<PluginModel>();
                   
                    foreach (var plugin in Plugins)
                    {
                        var ttl = await ISearch.CheckEnableAsync(plugin.Metadata.Source);
                        pluginModels.Add(new PluginModel(plugin.Metadata.Source, plugin.Metadata.Description, ttl != long.MinValue, ttl));
                    }
                    UpdatePlugins(pluginModels);
                }
                catch (Exception e)
                {
                    UpdatePlugins(new List<PluginModel>());
                    Console.WriteLine(e.ToString());
                }
            }
        }
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool AllocConsole();
    }
}