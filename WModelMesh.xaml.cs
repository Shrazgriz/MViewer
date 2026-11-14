using System;
using System.Collections.Generic;
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
    /// WModelMesh.xaml 的交互逻辑
    /// </summary>
    public partial class WModelMesh : Window
    {
        public int FaceId { get; set; }
        public WModelMesh(List<int> IdList)
        {
            InitializeComponent();
            foreach (int i in IdList)
            {
                CB_FaceId.Items.Add(i);
            }
            CB_FaceId.SelectedIndex = 0;
        }
        private void BN_OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BN_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CB_FaceId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FaceId = CB_FaceId.SelectedIndex;
        }
    }
}
