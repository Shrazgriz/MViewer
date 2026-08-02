using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MViewer
{
    /// <summary>
    /// WReadMesh.xaml 的交互逻辑
    /// </summary>
    public partial class WReadMesh : Window
    {
        public MeshPara Para { get; private set; }
        public WReadMesh(MeshPara value)
        {
            InitializeComponent();
            Para = value;
            DataContext = Para;
            LB_ColorMode.SelectedIndex = (int)(value.ColorMode);
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
            Para.MeshColor = Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B);
        }
        private void SavePara()
        {
            ConfigurationManager.RefreshSection("appSettings");
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            cfa.AppSettings.Settings["ColorMode"].Value = Para.ColorMode.ToString();            
            cfa.AppSettings.Settings["Thickness"].Value = Para.Thickness.ToString();
            cfa.AppSettings.Settings["MeshBrush"].Value = Para.MeshBrush.ToString();            
            cfa.Save();
        }
        private void LB_ColorMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Para != null)
            {
                Para.ColorMode = (ColorMode)LB_ColorMode.SelectedIndex;
            }
        }
    }
}
