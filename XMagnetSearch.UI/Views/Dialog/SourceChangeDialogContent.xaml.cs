using System.Collections.ObjectModel;
using System.Windows.Controls;
using XMagnetSearch.UI.Models;

namespace XMagnetSearch.UI.Views.Dialog
{
    /// <summary>
    /// SourceChangeDialogContent.xaml 的交互逻辑
    /// </summary>
    public partial class SourceChangeDialogContent : UserControl
    {
        public SourceChangeDialogContent()
        {
            InitializeComponent();
        }
        public SourceChangeDialogContent(ObservableCollection<PluginModel> models) : this()
        {
           PART_ItemsControl.ItemsSource = models;
        }
    }
}
