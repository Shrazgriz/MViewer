using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Linq;
using MVUnity;
using MVUnity.PointCloud;
using MVUnity.Exchange;
using System.IO;
using Microsoft.Win32;
using AnyCAD.Foundation;
using AnyCAD.WPF;
using System.Security.Cryptography;
using MVUnity.Geometry3D;
using MViewer.Graphics;
using System.Drawing;

namespace MViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public int StartAngle { get; set; } = 0;
        public int LastAngle { get; set; } = 120;
        public int AngleStep { get; set; } = 10;

        public int Thickness { get; set; } = 10;
        public double Radius { get; set; } = 150;
        public int PointCount { get; set; } = 200;

        public ICommand XYZCommand { get; set; }
        public ICommand ASCCommand { get; set; }
        public ICommand CADCommand { get; set; }

        private V3 step;
        List<V2> pts;
        IEnumerator<V2> enumerator;
        DelaunayTriangulator triangulator;
        const ulong CloudID = 1;
        bool showPoints;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            XYZCommand = new Command(param => ReadXYZ());
            ASCCommand = new Command(param => ReadASC());
            CADCommand = new Command(param => ReadCAD());
            showPoints = true;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void ReadASC()
        {
            OpenFileDialog openfile = new OpenFileDialog() { Filter = "asc文件|*.asc" };
            if (openfile.ShowDialog() == true)
            {
                WReadCloud readCloud = new WReadCloud(new CloudPara(openfile.FileName));
                if (readCloud.ShowDialog() == true)
                {
                    mRenderCtrl.ClearScene();
                    var filereader = new CloudReader
                    {
                        Scale = readCloud.Para.Cloudscale,
                        FileName = readCloud.Para.CloudFilePath,
                        Format = readCloud.Para.Cloudformat,
                        VertSkip= readCloud.Para.VertSkip
                    };
                    Graphic_Cloud cloud = new Graphic_Cloud(filereader);
                    cloud.Run(mRenderCtrl);
                }
            }
        }
        
        private void ReadCAD()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = SceneIO.FormatFilters();
            if (dlg.ShowDialog() != true)
                return;

            var node = SceneIO.Load(dlg.FileName);
            if (node == null)
                return;

            mRenderCtrl.ShowSceneNode(node);
            mRenderCtrl.ZoomAll();
        }

        private void mRenderCtrl_ViewerReady()
        {
            mRenderCtrl.ViewContext.SetRectPick(true);
            mRenderCtrl.ClearPickFilters();
            mRenderCtrl.AddPickFilter(EnumShapeFilter.Vertex);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SceneNode node = mRenderCtrl.Scene.FindNodeByUserId(CloudID);
            if (node == null)
            {
                return;
            }
            PointCloud cloud = PointCloud.Cast(node);
            var mng = mRenderCtrl.ViewContext.GetSelectionManager();
            var selection = mng.GetSelection();
            var iter = selection.CreateIterator();
            List<V3> points = new List<V3>();
            while (iter.More())
            {
                var item = iter.Current();
                var value = ConvertVector3.ToV3(item.GetPosition());
                points.Add(new V3(value.X, value.Y, 1));
                iter.Next();
            }
            MVUnity.Geometry3D.Polygon bound = new MVUnity.Geometry3D.Polygon(points);
            Graphic_Clip clip = new Graphic_Clip(mRenderCtrl);
            clip.PolySelection(cloud, bound);
        }

        private void ReadXYZ()
        {
            Microsoft.Win32.OpenFileDialog open = new Microsoft.Win32.OpenFileDialog() {
                Filter = "xyz文件|*.xyz"
            };
            if (open.ShowDialog() == true)
            {
                WReadCloud readCloud = new WReadCloud(new CloudPara(open.FileName));
                if(readCloud.ShowDialog() == true)
                {
                    mRenderCtrl.ClearScene();
                    var filereader = new CloudReader
                    {
                        Scale = readCloud.Para.Cloudscale,
                        FileName = readCloud.Para.CloudFilePath,
                        Format = readCloud.Para.Cloudformat,
                        VertSkip = readCloud.Para.VertSkip
                    };
                    Graphic_Cloud cloud = new Graphic_Cloud(filereader);
                    cloud.Run(mRenderCtrl);
                }
            }
        }

        private void ButtonPoints_Click(object sender, RoutedEventArgs e)
        {
            SceneNode node = mRenderCtrl.Scene.FindNodeByUserId(CloudID);
            if (node == null)
            {
                return;
            }
            showPoints = !showPoints;
            node.SetVisible(showPoints);
            mRenderCtrl.RequestDraw(EnumUpdateFlags.Scene);
        }

        private void BN_PointMeasure_Click(object sender, RoutedEventArgs e)
        {
            var mng = mRenderCtrl.ViewContext.GetSelectionManager();
            var selection = mng.GetSelection();
            var iter = selection.CreateIterator();
            List<V3> points = new List<V3>();
            while (iter.More())
            {
                var item = iter.Current();
                var value = item.GetPosition();
                points.Add(ConvertVector3.ToV3(value));
                iter.Next();
            }
            switch (points.Count)
            {
                case 0:
                    break;
                case 1:
                    WMessage wMessage1 = new WMessage(points[0].ToString());
                    wMessage1.Show();
                    break;
                case 2:
                    break;
                case 3:
                    V3 vec1 = points[1] - points[0];
                    V3 vec2 = points[2] - points[1];
                    V3 cross= vec1.Cross(vec2).Normalized;
                    WMessage wMessage3 = new WMessage(cross.ToString());
                    wMessage3.Show();
                    break;
                default:
                    break;
            }
        }
    }

    public class Command : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public Command(Action<object> execute)
            : this(execute, param => true)
        {
        }

        public Command(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return (_canExecute == null) || _canExecute(parameter);
        }
    }
}
