using System.Configuration;
using System.Windows;
using MVUnity;

namespace MViewer
{
    /// <summary>
    /// WROI.xaml 的交互逻辑
    /// </summary>
    public partial class WROI : Window
    {
        public V3 UL {  get; set; }
        public V3 LL { get; set; }
        public WROI()
        {
            InitializeComponent();
            DataContext = this;
        }
        public WROI(V3 ul, V3 ll)
        {
            UL = ul;
            LL = ll;
        }

        private void BN_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BN_OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            SavePara();
        }
        private void SavePara()
        {
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cfa.AppSettings.Settings["UL"].Value = UL.ToString();
            cfa.AppSettings.Settings["LL"].Value = LL.ToString();
            cfa.Save();
        }
    }
}
