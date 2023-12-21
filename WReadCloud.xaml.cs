using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
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
    /// WReadCloud.xaml 的交互逻辑
    /// </summary>
    public partial class WReadCloud : Window
    {
        public CloudPara Para { get; private set; }
        public WReadCloud(CloudPara value)
        {
            InitializeComponent();
            Para = value;
            DataContext = Para;
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
        private void BN_Color_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog
            {
                AllowFullOpen = true,
                FullOpen = true,
                ShowHelp = true,
                Color = System.Drawing.Color.Black
            };
            cd.ShowDialog();
            Para.PointColor = Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B);
        }
        private void SavePara()
        {
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cfa.AppSettings.Settings["CloudFormat"].Value = Para.Cloudformat;
            cfa.AppSettings.Settings["CloudScale"].Value = Para.Cloudscale.ToString();
            cfa.AppSettings.Settings["PointSize"].Value = Para.PointSize.ToString();
            cfa.AppSettings.Settings["PointBrush"].Value = Para.PointBrush.ToString();
            cfa.AppSettings.Settings["VertSkip"].Value=Para.VertSkip.ToString();
            cfa.Save();
        }
    }
}
