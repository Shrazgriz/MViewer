using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using MVUnity;
using MVUnity.Exchange;

namespace MViewer
{
    /// <summary>
    /// WCalib.xaml 的交互逻辑
    /// </summary>
    public partial class WCalib : Window
    {
        ObservableCollection<PointInfo> inputPts = new ObservableCollection<PointInfo>();
        ObservableCollection<PointInfo> targetPts = new ObservableCollection<PointInfo>();
        private WCalib()
        {
            InitializeComponent();
        }

        public WCalib(IEnumerable<V3> Points)
        {
            InitializeComponent();
            inputPts = new ObservableCollection<PointInfo>();
            foreach (var pt in Points)
            {
                inputPts.Add(new PointInfo(pt));
            }
            targetPts = new ObservableCollection<PointInfo>();
            LB_InputCoords.ItemsSource = inputPts;
            LB_TargetCoords.ItemsSource = targetPts;
        }

        private void BN_Calculation_Click(object sender, RoutedEventArgs e)
        {
            List<V3> input0 = inputPts.Select(p=>new V3(p.Value)).ToList();
            List<V3> target0 = targetPts.Select(p => new V3(p.Value)).ToList();
            int num = Math.Min(input0.Count, target0.Count);
            EuclideanTransform et = EuclideanTransform.SVD(input0, target0);
            TB_RotaM.Text = et.R.ToString();
            TB_DispV.Text = et.T.ToString();
        }

        private void BN_Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter= "文本文件|*.txt"};
            if(saveFileDialog.ShowDialog()== true)
            {
                string fn = saveFileDialog.FileName;
                StreamWriter writer = new StreamWriter(fn);
                writer.WriteLine(TB_DispV.Text);
                writer.WriteLine(TB_RotaM.Text);
                writer.Close();
                MessageBox.Show("写入完成");
            }
        }

        private void BN_ClipM_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(TB_RotaM.Text);
        }

        private void BN_ClipV_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(TB_DispV.Text);
        }

        private void BN_newT_Click(object sender, RoutedEventArgs e)
        {
            PointInfo newInfo = new PointInfo(V3.Zero);
            targetPts.Add(newInfo);
        }

        private void BN_newI_Click(object sender, RoutedEventArgs e)
        {
            PointInfo newInfo = new PointInfo(V3.Zero);
            inputPts.Add(newInfo);
        }

        private void BN_ImportInput_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog() { Filter = "xyz文件|*.xyz" };
            if(openFile.ShowDialog()== true)
            {
                var filereader = new CloudReader
                {
                    Scale = V3.Identity,
                    FileName = openFile.FileName,
                    Format = "xyz",
                    VertSkip = 0
                };
                var pts = filereader.ReadXYZ();
                foreach (var pt in pts)
                {
                    inputPts.Add(new PointInfo(pt));
                }
            }
        }

        private void BN_ImportTarget_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog() { Filter = "xyz文件|*.xyz" };
            if (openFile.ShowDialog() == true)
            {
                var filereader = new CloudReader
                {
                    Scale = V3.Identity,
                    FileName = openFile.FileName,
                    Format = "xyz",
                    VertSkip = 0
                };
                var pts = filereader.ReadXYZ();
                foreach (var pt in pts)
                {
                    targetPts.Add(new PointInfo(pt));
                }
            }
        }
    }
}
