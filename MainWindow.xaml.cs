using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using Microsoft.Win32;
using AnyCAD.Foundation;
using MVUnity;
using MVUnity.PointCloud;
using MVUnity.Exchange;
using MVUnity.Geometry3D;
using MViewer.Graphics;

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

        public ICommand CADCommand { get; set; }
        public ICommand PCDCommand { get; set; }
        public ICommand Seg2Command { get; set; }
        public ICommand Seg3Command { get; set; }
        public ICommand ExpPtsCommand { get; set; }
        public ICommand M2CloudCommand { get; set; }
        public ICommand CalibCommand { get; set; }
        private V3 step;
        List<V2> pts;
        IEnumerator<V2> enumerator;
        GroupSceneNode cloudroot;
        const ulong CloudID = 1;
        const ulong CubicMapID = 12;
        bool showPoints;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            CADCommand = new Command(param => ReadCAD());
            M2CloudCommand = new Command(param=>Model2Cloud());
            Seg2Command = new Command(param => ReadSeg2());
            Seg3Command = new Command(param => ReadSeg3());
            PCDCommand = new Command(param => ReadPCD());
            ExpPtsCommand = new Command(param=> ExpPts());
            CalibCommand = new Command(param => Calib());
            showPoints = true;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void ReadPCD()
        {
            OpenFileDialog dlg = new OpenFileDialog() { Filter = "点云文件|*.pcd;*.asc;*.xyz;*.ply", Multiselect = true };
            if (dlg.ShowDialog() == true)
            {
                WReadCloud readCloud = new WReadCloud(new CloudPara(dlg.FileNames));
                if (readCloud.ShowDialog() == true)
                {
                    var prevNode = GroupSceneNode.Cast(mRenderCtrl.Scene.FindNodeByUserId(CloudID));
                    if (prevNode != null)
                    {
                        prevNode.Clear();
                    }
                    else
                    {
                        prevNode = new GroupSceneNode();
                        prevNode.SetUserId(CloudID);
                        mRenderCtrl.Scene.AddNode(prevNode);
                    }
                    foreach (var fn in readCloud.Para.CloudFilePath)
                    {
                        var filereader = new CloudReader
                        {
                            Scale = readCloud.Para.Cloudscale,
                            FileName = fn,
                            Format = readCloud.Para.Cloudformat,
                            VertSkip = readCloud.Para.VertSkip
                        };
                        Graphic_Cloud cloud = new Graphic_Cloud(filereader);
                        cloud.ColorMode = readCloud.Para.ColorMode;
                        if (readCloud.Para.UseROI)
                        {
                            ABB roi = new ABB(readCloud.Para.UL, readCloud.Para.LL);
                            cloud.ROI = roi;
                            cloud.UseROI = true;
                        }
                        cloud.Append(mRenderCtrl);
                    }
                    TVI_root.Header = System.IO.Path.GetFileName(dlg.FileName);
                    mRenderCtrl.Viewer.RequestUpdate(EnumUpdateFlags.Scene);
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
        private void Model2Cloud()
        {
            OpenFileDialog dlg = new OpenFileDialog() { Filter="stl模型|*.stl"};
            //dlg.Filter = SceneIO.FormatFilters();
            if (dlg.ShowDialog() != true)
                return;
            SaveFileDialog dlg2 = new SaveFileDialog() { Filter = "xyz文件|*.xyz" };
            if (dlg2.ShowDialog() != true)return;
            WReadCloud wRead = new WReadCloud(new CloudPara(dlg2.FileName));
            wRead.ShowDialog();
            var node = SceneIO.Load(dlg.FileName);
            if (node == null)
                return;
            var para = wRead.Para;
            int step = para.VertSkip + 1;
            TopoShape shape = StlIO.Open(dlg.FileName);
            ShapeExplor sExp = new ShapeExplor();
            sExp.AddShape(shape);
            var count = sExp.GetFaceCount();
            List<V3> pts = new List<V3>();
            StreamWriter w = new StreamWriter(dlg2.FileName);
            for (uint i = 0; i < count; i++)
            {
                var face = sExp.GetFace(i);
                var u0 = face.FirstUParameter();
                var v0 = face.FirstVParameter();
                var u1 = face.LastUParameter();
                var v1 = face.LastVParameter();
                for (int m = (int)Math.Ceiling(u0); m < u1; m += step)
                {
                    for (int n = (int)Math.Ceiling(v0); n < v1; n += step)
                    {
                        var p = face.D0(m, n);
                        pts.Add(new V3(p.x, p.y, p.z));
                        w.WriteLine(string.Format("{0},{1},{2}", p.x, p.y, p.z));
                    }
                }
            }
            w.Close();
            Graphic_Cloud cloud = new Graphic_Cloud();
            cloud.ShowCloud(pts, mRenderCtrl);
            mRenderCtrl.ZoomAll();
        }
        private void ReadSeg2()
        {
            OpenFileDialog openfile = new OpenFileDialog() { Filter = "线段数据|*.txt" };
            if (openfile.ShowDialog() == true)
            {
                Graphic_Segs seg = new Graphic_Segs(mRenderCtrl);
                seg.Run2(openfile.FileName);
            }
        }
        private void ReadSeg3()
        {
            OpenFileDialog openfile = new OpenFileDialog() { Filter = "线段数据|*.txt" };
            if (openfile.ShowDialog() == true)
            {
                Graphic_Segs seg = new Graphic_Segs(mRenderCtrl);
                seg.Run3(openfile.FileName);
            }
        }
        private void ExpPts()
        {
            SaveFileDialog savefile = new SaveFileDialog() {Filter="xyz文件|*.xyz" };
            if(savefile.ShowDialog() == true)
            {
                StreamWriter writer = new StreamWriter(savefile.FileName);
                var mng = mRenderCtrl.ViewContext.GetSelectionManager();
                var selection = mng.GetSelection();
                if(selection.GetCount() > 0)
                {
                    var iter = selection.CreateIterator();
                    while (iter.More())
                    {
                        var item = iter.Current();
                        var value = item.GetPosition();
                        writer.WriteLine(value.ToString());
                        iter.Next();
                    }
                    writer.Close();
                }
                else
                {
                    var pcn = mRenderCtrl.Scene.FindNodeByUserId(CloudID);
                    if(pcn != null)
                    {
                        GroupSceneNode gp = GroupSceneNode.Cast(pcn);
                        var iter =gp.CreateIterator();
                        while (iter.More())
                        {
                            var item = iter.Current();
                            var n = PointCloud.Cast(item);
                            if(n != null)
                            {
                                var count = n.GetPointCount();
                                for (uint i = 0; i < count; i++)
                                {
                                    var value = n.GetPosition(i);
                                    writer.WriteLine(value.ToString());
                                }
                            }
                            iter.Next();
                        }
                    }
                    writer.Close();
                }
            }
        }
        private void Calib()
        {

        }
        private void mRenderCtrl_ViewerReady()
        {
            mRenderCtrl.ViewContext.SetRectPick(false);
            mRenderCtrl.ClearPickFilters();
            mRenderCtrl.AddPickFilter(EnumShapeFilter.Vertex);
            cloudroot = new GroupSceneNode();
            mRenderCtrl.ShowSceneNode(cloudroot);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SceneNode node = mRenderCtrl.Scene.FindNodeByUserId(CloudID);
            if (node == null)
            {
                return;
            }
            var group = GroupSceneNode.Cast(node);
            var citer =group.CreateIterator();            
            PointCloud cloud = PointCloud.Cast(citer.Current());
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
                    WriteLine(points[0].ToString());
                    break;
                case 2:
                    V3 vec0 = points[1] - points[0];
                    WriteLine(vec0.Mod.ToString());
                    break;
                case 3:
                    V3 vec1 = points[1] - points[0];
                    V3 vec2 = points[2] - points[1];
                    V3 cross= vec1.Cross(vec2).Normalized();
                    WriteLine(cross.ToString());
                    break;
                default:
                    break;
            }
        }

        private void BN_CubicMtx_Click(object sender, RoutedEventArgs e)
        {
            var cloud = mRenderCtrl.Scene.FindNodeByUserId(CloudID);
            if (cloud == null) return;
            PointCloud pcn= PointCloud.Cast(cloud);
            var n=pcn.GetPointCount();
            List<V3> points = new List<V3>();
            for (uint i = 0; i < n; i++)
            {
                var value = pcn.GetPosition(i);
                points.Add(ConvertVector3.ToV3(value));
            }
            CubicMap cMap = CubicMap.CreateCubicMap(points,5);
            Graphic_CMTX cmtx = new Graphic_CMTX(mRenderCtrl,cMap);
            cmtx.DrawCubicRLMtx(2);
        }

        private void AppendLine(string text)
        {
            TB_Output.Text += string.Format("{0}\n", text);
        }
        private void WriteLine(string text)
        {
            TB_Output.Text = string.Format("{0}\n", text);
        }

        private void BN_ToClip_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(TB_Output.Text);
        }

        private void BN_Calibra_Click(object sender, RoutedEventArgs e)
        {

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
