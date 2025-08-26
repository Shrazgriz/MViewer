using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MViewer
{
    /// <summary>
    /// WRayHits.xaml 的交互逻辑
    /// </summary>
    public partial class WRayHits : Window
    {
        public RayHitsPara Para;
        public WRayHits(RayHitsPara value)
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
            cfa.AppSettings.Settings["Direction"].Value = Para.Direction.ToString();
            cfa.AppSettings.Settings["_rayDir"].Value = Para._rayDir.ToString();
            cfa.Save();
        }
    }
}
