using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
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
                    Grid.SetRowSpan(bd_search, 1);
                    LoadSearchs(tb.Text);
                }
                e.Handled = true;
            }
        }

        private void LoadSearchs(string input)
        {
            DialogHost.Show(new SearchingDialogContent(), "RootDialog");
            _page += 1;
            if (Plugins != null)
            {
                foreach (var plugin in Plugins)
                {
                    plugin.Value.SearchAsync(input, _page).ContinueWith(ret =>
                    {
                        if (ret.IsCompletedSuccessfully)
                        {
                            UpdateSearchs(ret.Result);
                        }
                    });
                }
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
            var models = new List<SearchModel>();
            foreach(var bean in  searchBeans)
            {
                models.Add(SearchModel.Converter(bean));
            }
            if (DataContext is MainVM vm)
            {
                vm.Searchs.AddRange(models);
            }
        }

        private void RegisterPluginAsync()
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            if (dir.Exists)
            {
                var catalog = new DirectoryCatalog(dir.FullName, "*.dll");
                _container = new CompositionContainer(catalog);
                try
                {
                    _container.ComposeParts(this);                    
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
                    Clipboard.SetText(searchModel.MagnetUrl);
                    Snackbar.MessageQueue?.Clear();
                    Snackbar.MessageQueue?.Enqueue("链接已复制");
                }              
            }
            _lastDownTime = DateTime.Now;
        }
    }
}