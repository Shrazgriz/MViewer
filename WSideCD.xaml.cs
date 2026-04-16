using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using MVUnity;
using MVUnity.Exchange;
using System.Collections.ObjectModel;
using System.IO;

namespace MViewer
{
    /// <summary>
    /// WSideCD.xaml 的交互逻辑
    /// </summary>
    public partial class WSideCD : Window
    {
        V3 sideNorm = V3.Forward;
        ObservableCollection<PointInfo> inputPts = new ObservableCollection<PointInfo>();
        ObservableCollection<double> corrections = new ObservableCollection<double>();
        public WSideCD()
        {
            InitializeComponent();
            inputPts = new ObservableCollection<PointInfo>();
            corrections = new ObservableCollection<double>();
            LB_InputCoords.ItemsSource = inputPts;
            LB_Corrections.ItemsSource = corrections;
        }

        private void BN_Calculation_Click(object sender, RoutedEventArgs e)
        {
            List<V3> input0 = inputPts.Select(p => new V3(p.Value)).ToList();
            List<V3> target0 = new List<V3>();
            for (int i = 0; i < inputPts.Count; i++)
            {
                PointInfo pt = inputPts[i];
                V3 value = new V3(pt.Value);
                if (i < corrections.Count)
                {
                    double disp = corrections[i];
                    value += disp * sideNorm;
                }
                target0.Add(value);
            }
            int num = Math.Min(input0.Count, target0.Count);
            EuclideanTransform et = EuclideanTransform.SVD(input0, target0, 1, 0.00001f);
            var trans = input0.Select(p => et.Transform(p)).ToList();
            var errors = new List<double>();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < num; i++)
            {
                double dis = trans[i].Distance(target0[i]);
                sb.AppendLine(dis.ToString("F4"));
                errors.Add(dis);
            }
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


        private void BN_newI_Click(object sender, RoutedEventArgs e)
        {
            PointInfo newInfo = new PointInfo(V3.Zero);
            inputPts.Add(newInfo);
        }

        private void BN_newC_Click(object sender, RoutedEventArgs e)
        {            
            corrections.Add(0);
        }

        private void BN_Import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog() { Filter = "文本文件|*.txt" };
            if (openFile.ShowDialog() == true)
            {
                StreamReader reader = new StreamReader(openFile.FileName);
                string line = reader.ReadLine();
                while (line != null)
                {
                    if (line.Length == 0) break;
                    if (reader.EndOfStream) break;
                    string[] split = line.Split(new char[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length < 4) break;
                    double x = double.Parse(split[0]);
                    double y = double.Parse(split[1]);
                    double z = double.Parse(split[2]);
                    double v = double.Parse(split[3]);
                    V3 pt = new V3(x, y, z);
                    inputPts.Add(new PointInfo(pt));
                    corrections.Add(v);
                    line = reader.ReadLine();
                }
                reader.Close();
            }
        }
    }
}
