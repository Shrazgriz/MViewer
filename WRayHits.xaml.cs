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
using MVUnity;

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
            CB_RayDir.SelectedIndex = (int)value.RayDirection;
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
            cfa.AppSettings.Settings["AlignZ"].Value = Para.AlignZ.ToString();
            cfa.Save();
        }

        private void CB_RayDir_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            Para.RayDirection = (RayDirection)CB_RayDir.SelectedIndex;
            switch (Para.RayDirection)
            {
                case RayDirection.PX:
                    Para.Direction = V3.Forward;
                    break;
                case RayDirection.NX:
                    Para.Direction = -1f * V3.Forward;
                    break;
                case RayDirection.PY:
                    Para.Direction = V3.Right;
                    break;
                case RayDirection.NY:
                    Para.Direction = -1f * V3.Right;
                    break;
                case RayDirection.PZ:
                    Para.Direction = V3.Up;
                    break;
                case RayDirection.NZ:
                    Para.Direction = -1f * V3.Up;
                    break;
                default:
                    break;
            }
            TB_Dir.Text = Para.Direction.ToString();
        }
    }
}
