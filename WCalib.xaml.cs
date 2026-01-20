using Microsoft.Win32;
using MVUnity;
using MVUnity.Exchange;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace MViewer
{
    /// <summary>
    /// WCalib.xaml 的交互逻辑
    /// </summary>
    public partial class WCalib : Window
    {
        ObservableCollection<PointInfo> inputPts = new ObservableCollection<PointInfo>();
        ObservableCollection<PointInfo> targetPts = new ObservableCollection<PointInfo>();
        ObservableCollection<PointInfo> corrections = new ObservableCollection<PointInfo>();
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
            corrections = new ObservableCollection<PointInfo>();
            LB_InputCoords.ItemsSource = inputPts;
            LB_TargetCoords.ItemsSource = targetPts;
            LB_Corrections.ItemsSource = corrections;
        }

        private void BN_Calculation_Click(object sender, RoutedEventArgs e)
        {
            List<V3> input0 = inputPts.Select(p => new V3(p.Value)).ToList();
            List<V3> target0 = new List<V3>();
            for (int i = 0; i < targetPts.Count; i++)
            {
                PointInfo pt = targetPts[i];
                V3 value = new V3(pt.Value);
                if (i < corrections.Count)
                {
                    PointInfo pt2 = corrections[i];
                    value += new V3(pt2.Value);
                }
                target0.Add(value);
            }
            int num = Math.Min(input0.Count, target0.Count);
            EuclideanTransform et = EuclideanTransform.SVD(input0, target0, 0.00001f);
            var trans = input0.Select(p => et.Transform(p)).ToList();
            var errors = new List<double>();
            StringBuilder sb =new StringBuilder();
            for (int i = 0; i < num; i++)
            {
                double dis = trans[i].Distance(target0[i]);
                sb.AppendLine(dis.ToString("F4"));
                errors.Add(dis);
            }
            //TB_MaxError.Text = errors.Max().ToString("F3");
            TB_MaxError.Text = sb.ToString();
            if (errors.Max() > 20)
            {
                TB_MaxError.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                TB_MaxError.Foreground = new SolidColorBrush(Colors.Black);
            }
            TB_RotaM.Text = et.R.ToString();
            TB_DispV.Text = et.T.ToString();
        }

        private void BN_Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter = "文本文件|*.txt" };
            if (saveFileDialog.ShowDialog() == true)
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
            OpenFileDialog openFile = new OpenFileDialog() { Filter = "文本文件|*.xyz;*.txt" };
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
                    inputPts.Add(new PointInfo(pt));
                }
            }
        }

        private void BN_ImportTarget_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog() { Filter = "文本文件|*.xyz;*.txt" };
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

        private void BN_newC_Click(object sender, RoutedEventArgs e)
        {
            PointInfo newInfo = new PointInfo(V3.Zero);
            corrections.Add(newInfo);
        }

        private void BN_ImportCorrection_Click(object sender, RoutedEventArgs e)
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
                    corrections.Add(new PointInfo(pt));
                }
            }
        }
    }
}
