using System.Configuration;
using System.Windows;

namespace MViewer
{
    /// <summary>
    /// WSelection.xaml 的交互逻辑
    /// </summary>
    public partial class WSelection : Window
    {
        public SelectionPara Para { get; private set; }
        public WSelection(SelectionPara value)
        {
            InitializeComponent();
            Para = value;
            DataContext = Para;
        }
        private void BN_OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            SavePara();
        }
        private void BN_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        private void SavePara()
        {
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cfa.AppSettings.Settings["UserNormal"].Value = Para.UserNormal.ToString();
            cfa.AppSettings.Settings["UseUserNorm"].Value = Para.UseUserNorm.ToString();
            cfa.AppSettings.Settings["NormDotTol"].Value = Para.NormDotTol.ToString();
            cfa.Save();
        }
    }
}
