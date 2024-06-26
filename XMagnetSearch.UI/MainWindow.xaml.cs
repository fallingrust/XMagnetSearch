﻿using MaterialDesignThemes.Wpf;
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
        public IEnumerable<Lazy<SearchBase, IMetadata>>? Plugins { get; set; }
        private CompositionContainer? _container = null;
        private int _page = 0;
        private DateTime _lastDownTime;
        private bool _end = false;
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
            if (RootDialog.Identifier != null)
                _ = DialogHost.Show(new SearchingDialogContent(), RootDialog.Identifier);
            _ = RegisterPluginAsync();
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {        
            if (e.ViewportHeight + e.VerticalOffset >= e.ExtentHeight 
                && e.ExtentHeight > 0
                && e.VerticalChange > 0
                && !_end
                && !DialogHost.IsDialogOpen(RootDialog.Identifier)
                && !string.IsNullOrWhiteSpace(tb_search.Text))
            {
                LoadSearchs(tb_search.Text);
            }
        }
        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement container && container.DataContext is SearchModel searchModel)
            {
                if (searchModel.MagnetUrl.StartsWith("magnet:?xt=urn:btih:"))
                {
                    Clipboard.SetDataObject(searchModel.MagnetUrl);
                }
                else
                {
                    Clipboard.SetDataObject($"magnet:?xt=urn:btih:{searchModel.MagnetUrl}");
                }

                Snackbar.MessageQueue?.Clear();
                Snackbar.MessageQueue?.Enqueue("链接已复制");
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
                    _end = false;
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
                    if (searchModel.MagnetUrl.StartsWith("magnet:?xt=urn:btih:"))
                    {
                        Clipboard.SetDataObject(searchModel.MagnetUrl);
                    }
                    else
                    {
                        Clipboard.SetDataObject($"magnet:?xt=urn:btih:{searchModel.MagnetUrl}");
                    }
                  
                    Snackbar.MessageQueue?.Clear();
                    Snackbar.MessageQueue?.Enqueue("链接已复制");
                }
            }
            _lastDownTime = DateTime.Now;
        }

        private void OnSourceClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainVM vm && ChildDialog.Identifier != null)
            {
                DialogHost.Show(new SourceChangeDialogContent(vm.Plugins), ChildDialog.Identifier);
            }
        }
        private void LoadSearchs(string input)
        {
            if (RootDialog.Identifier != null)
                DialogHost.Show(new SearchingDialogContent(), RootDialog.Identifier);
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
                        Console.WriteLine($"load page {_page}~");
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
                Dispatcher.Invoke(DispatcherPriority.DataBind, () => UpdateSearchs(searchBeans));
                return;
            }
            if (DialogHost.IsDialogOpen(RootDialog.Identifier))
            {
                DialogHost.Close(RootDialog.Identifier);
            }
            if (!searchBeans.Any())
            {
                _end = true;
                return;
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
                Sort();
            }          
        }
        private void UpdatePlugins(IEnumerable<PluginModel> pluginModels)
        {
            if (!CheckAccess())
            {
                Dispatcher.BeginInvoke(DispatcherPriority.DataBind, () => UpdatePlugins(pluginModels));
                return;
            }
            if (DialogHost.IsDialogOpen(RootDialog.Identifier))
            {
                DialogHost.Close(RootDialog.Identifier);
            }
            if (DataContext is MainVM vm)
            {
                vm.Plugins.Clear();
                var models = pluginModels.OrderBy(p => p.TTL);
                foreach(var model in models.Take(3))
                {
                    model.Selected = model.TTL != long.MaxValue;
                }
                vm.Plugins.AddRange(models);
            }
            Snackbar.MessageQueue?.Clear();
            Snackbar.MessageQueue?.Enqueue("搜索源已更新");
        }
        private async Task RegisterPluginAsync()
        {
            await Task.Run(() =>
             {
                 var dir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins"));
                 if (dir.Exists)
                 {

                     var dirCatalogs = new List<DirectoryCatalog>();
                     foreach (var pluginDir in dir.GetDirectories())
                     {
                         var catalog = new DirectoryCatalog(pluginDir.FullName, "*.dll");
                         dirCatalogs.Add(catalog);
                     }
                     var catalogs = new AggregateCatalog(dirCatalogs);

                     _container = new CompositionContainer(catalogs);
                     var pluginModels = new List<PluginModel>();
                     try
                     {
                         _container.ComposeParts(this);
                       
                         if (Plugins != null)
                         {
                             var ttlTasks = new List<Task<long>>();
                             foreach (var plugin in Plugins)
                             {
                                 if (!plugin.Metadata.Enable) continue;
                                 ttlTasks.Add(SearchBase.CheckEnableAsync(plugin.Metadata.Source));
                                 pluginModels.Add(new PluginModel(plugin.Metadata.Source, plugin.Metadata.Description, false, long.MaxValue));
                             }
                             Task.WaitAll(ttlTasks.ToArray());
                             ttlTasks.ForEach(async task =>
                             {
                                 var ttl = await task;
                                 var index = ttlTasks.IndexOf(task);
                                 pluginModels[index].TTL = ttl;
                                 pluginModels[index].Enable = ttl != long.MaxValue;
                             });
                         }   
                     }
                     catch (Exception e)
                     {
                         Console.WriteLine(e.ToString());
                     }
                     finally
                     {
                         UpdatePlugins(pluginModels);
                     }
                 }
             });
            
        }

        private void OnSortSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Sort();
        }

        private void Sort()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => Sort());
                return;
            }
            if (DataContext is MainVM vm && cb_sort.SelectedItem is ComboBoxItem cbi)
            {
                if (cbi.Name == cbi_size_des.Name)
                {
                    var temp = vm.Searchs.OrderByDescending(p => p.SizeInBytes).ToArray();
                    vm.Searchs.Clear();
                    vm.Searchs.AddRange(temp);
                }
                else if (cbi.Name == cbi_time_des.Name)
                {
                    var temp = vm.Searchs.OrderByDescending(p => p.DateTime).ToArray();
                    vm.Searchs.Clear();
                    vm.Searchs.AddRange(temp);
                }
                else if (cbi.Name == cbi_time.Name)
                {
                    var temp = vm.Searchs.OrderBy(p => p.DateTime).ToArray();
                    vm.Searchs.Clear();
                    vm.Searchs.AddRange(temp);
                }
                else if (cbi.Name == cbi_size.Name)
                {
                    var temp = vm.Searchs.OrderBy(p => p.SizeInBytes).ToArray();
                    vm.Searchs.Clear();
                    vm.Searchs.AddRange(temp);
                }
            }
        }
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool AllocConsole();

       
    }
}