using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace XMagnetSearch.UI
{
    public partial class MainWindow : Window
    {
        [ImportMany]
        public IEnumerable<Lazy<ISearch, IMetadata>>? Plugins { get; set; }
        private CompositionContainer? _container = null;
        public MainWindow()
        {
            InitializeComponent();
            tb_search.PreviewKeyDown += OnSerachKeyDown;
            RegisterPluginAsync();
            sc.ScrollChanged += OnScrollChanged;
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer sc && sc.ViewportHeight + sc.VerticalOffset >= sc.ExtentHeight)
            {

            }
        }

        private void OnSerachKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox tb && e.Key == Key.Enter && tb.IsKeyboardFocused)
            {
                if (!string.IsNullOrWhiteSpace(tb.Text))
                {
                    
                }
                e.Handled = true;
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
    }
}