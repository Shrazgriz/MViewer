using AnyCAD.Foundation;
using Microsoft.Win32;
using MViewer.Graphics;
using MVUnity;
using MVUnity.Exchange;
using MVUnity.Geometry3D;
using MVUnity.PointCloud;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

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
        public ICommand CirCommand { get; set; }
        public ICommand ExpPtsCommand { get; set; }
        public ICommand M2CloudCommand { get; set; }
        public ICommand CalibCommand { get; set; }
        public ICommand SeleByNBCommand { get; set; }
        public ICommand SeleByROICommand { get; set; }
        public ICommand FitCirCommand { get; set; }
        public ICommand MeshCommand { get; set; }
        public ICommand SeleByCirCommand { get; set; }
        GroupSceneNode cloudroot;
        const ulong CloudID = 1;
        const ulong CubicMapID = 12;
        const ulong TestObjID = 3;
        const ulong ColorCloudID = 4;
        const ulong ClipID = 5;
        const ulong OBBID = 6;
        const ulong MeshObjID = 10;
        const ulong LineObjID = 100;
        bool showPoints;
        Graphic_Clip clip;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            CADCommand = new Command(param => ReadCAD());
            M2CloudCommand = new Command(param => Model2Cloud());
            Seg2Command = new Command(param => ReadSeg2());
            Seg3Command = new Command(param => ReadSeg3());
            CirCommand = new Command(param => ReadCir());
            PCDCommand = new Command(param => ReadPCD());
            ExpPtsCommand = new Command(param => ExpPts());
            CalibCommand = new Command(param => Calib());
            SeleByNBCommand = new Command(param => SelectByNorm());
            SeleByROICommand = new Command(param => SelectByROI());
            SeleByCirCommand = new Command(param => SelectByCir());
            FitCirCommand = new Command(param => FitCircle());
            MeshCommand = new Command(param => ReadMesh());
            showPoints = true;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void ReadPCD()
        {
            OpenFileDialog dlg = new OpenFileDialog() { Filter = "点云文件|*.pcd;*.asc;*.xyz;*.ply;*.txt", Multiselect = true };
            if (dlg.ShowDialog() == true)
            {
                WReadCloud readCloud = new WReadCloud(new CloudPara(dlg.FileNames));
                if (readCloud.ShowDialog() == true)
                {
                    GC.Collect();
                    var prevNode = GroupSceneNode.Cast(mRenderCtrl.Scene.FindNodeByUserId(CloudID));
                    if (prevNode != null)
                    {
                        if (!readCloud.Para.Append)
                        {
                            prevNode.Clear();
                        }
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
                            Scale = readCloud.Para.CloudScale,
                            FileName = fn,
                            Format = readCloud.Para.Cloudformat,
                            VertSkip = readCloud.Para.VertSkip
                        };
                        Graphic_Cloud cloud = new Graphic_Cloud(filereader)
                        {
                            ColorMode = readCloud.Para.ColorMode,
                            Size = readCloud.Para.PointSize,
                            PColor = readCloud.Para.PointColor
                        };
                        if (readCloud.Para.UseROI)
                        {
                            ABB roi = ABB.Create(readCloud.Para.UL, readCloud.Para.LL);
                            cloud.ROI = roi;
                            cloud.UseROI = true;
                        }
                        if (readCloud.Para.Transform)
                        {
                            cloud.Transform = true;
                            cloud.T = readCloud.Para.ETT;
                            cloud.R = readCloud.Para.ETR;
                        }
                        cloud.Append(mRenderCtrl);
                    }
                    TVI_root.Header = System.IO.Path.GetFileName(dlg.FileName);
                    mRenderCtrl.Viewer.RequestUpdate(EnumUpdateFlags.Scene);
                }
            }
        }
        private TopoShape readTopo(string fileName)
        {
            switch (System.IO.Path.GetExtension(fileName))
            {
                case ".stp":
                case ".step":
                case ".STEP":
                    {
                        var shape = StepIO.Open(fileName);
                        return shape;
                    }
                case ".iges":
                case ".igs":
                    {
                        var shape = IgesIO.Open(fileName);
                        return shape;
                    }
                default:
                    throw new NotImplementedException();
            }
        }
        private void ReadCAD()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "*.igs;*.iges;*.stp;*.step;*.brep;*.stl|*.igs;*.iges;*.stp;*.step;*.brep;*.stl";
            if (dlg.ShowDialog() != true)
                return;
            SceneNode node;
            switch (System.IO.Path.GetExtension(dlg.FileName))
            {
                case ".stp":
                case ".step":
                case ".STEP":
                case ".iges":
                case ".igs":
                    {
                        var shape = readTopo(dlg.FileName);
                        node = BrepSceneNode.Create(shape, null, null, 0, false);
                    }
                    break;
                default:
                    node = SceneIO.Load(dlg.FileName);
                    break;
            }

            if (node == null)
                return;

            mRenderCtrl.ShowSceneNode(node);
            mRenderCtrl.ZoomAll();
        }
        private void Model2Cloud()
        {
            OpenFileDialog dlg = new OpenFileDialog() { Filter = "*.igs;*.iges;*.stp;*.step;*.brep;*.stl;*.STEP|*.igs;*.iges;*.stp;*.step;*.brep;*.stl;*.STEP" };
            if (dlg.ShowDialog() != true)
                return;
            WRayHits wRay = new WRayHits(new RayHitsPara());
            if (wRay.ShowDialog() != true) return;
            TopoShape shape = readTopo(dlg.FileName);
            var w = wRay.Para.Direction;
            var d = wRay.Para.Resolution;
            #region 估算射线范围
            List<V3> axis = new List<V3>() { V3.Forward, V3.Right, V3.Up };
            List<double> axisDot = axis.Select(a => Math.Abs(a.Dot(w))).ToList();
            int minId = axisDot.IndexOf(axisDot.Min());
            V3 v = w.Cross(axis[minId]).Normalized();
            V3 u = v.Cross(w).Normalized();
            SceneNode mTargetNode = BrepSceneNode.Create(shape, null, null, 0, false);
            ShapeExplor sExp = new ShapeExplor();
            sExp.AddShape(shape);
            sExp.Build();
            GBBox mBBox = sExp.GetBoundingBox();
            var mMinCorner = mBBox.CornerMin();
            var mMaxCorner = mBBox.CornerMax();
            V3 p0 = new V3(mMinCorner.X(), mMinCorner.Y(), mMinCorner.Z());
            V3 p1 = new V3(mMinCorner.X(), mMinCorner.Y(), mMaxCorner.Z());
            V3 p2 = new V3(mMinCorner.X(), mMaxCorner.Y(), mMinCorner.Z());
            V3 p3 = new V3(mMinCorner.X(), mMaxCorner.Y(), mMaxCorner.Z());
            V3 p4 = new V3(mMaxCorner.X(), mMinCorner.Y(), mMinCorner.Z());
            V3 p5 = new V3(mMaxCorner.X(), mMinCorner.Y(), mMaxCorner.Z());
            V3 p6 = new V3(mMaxCorner.X(), mMaxCorner.Y(), mMinCorner.Z());
            V3 p7 = new V3(mMaxCorner.X(), mMaxCorner.Y(), mMaxCorner.Z());
            var mid = 0.5f * (p0 + p7);
            List<V3> boxPts=new List<V3>() { p0, p1, p2, p3, p4, p5, p6, p7 };
            List<double> uValues = boxPts.Select(e => e.Dot(u)).ToList();
            List<double> vValues = boxPts.Select(e => e.Dot(v)).ToList();
            CatersianSys raySys = CatersianSys.CreateSysOXN(mid, u, w);
            #endregion
            #region 射线检测
            List<Line> rayList = new List<Line>();
            List<Triangle> facets = new List<Triangle>();
            for (double i = uValues.Min(); i < uValues.Max(); i+=d)
            {
                for (double j = vValues.Min(); j <vValues.Max(); j+=d)
                {
                    V3 ptOnLine = i * u + j * v;
                    Line ray = new Line(w, ptOnLine);
                    rayList.Add(ray);
                }
            }

            var faces = shape.GetChildren(EnumTopoShapeType.Topo_FACE);
            var fenum = faces.GetEnumerator();
            while (fenum.MoveNext())
            {
                var face = fenum.Current;
                var tris = Graphic_Tris.DecomposeFace(face);
                facets.AddRange(tris);
            }
            List<V3> hitPts = new List<V3>();
            foreach (var ray in rayList)
            {
                List<V3> xPts = new List<V3>();
                foreach (var face in facets)
                {
                    MVUnity.Plane triBase = MVUnity.Plane.CreatePlane(face.Vertices[0], face.Vertices[1], face.Vertices[2]);
                    if (Math.Abs(triBase.Norm.Dot(w)) < 0.001f) continue;
                    var xpt = triBase.IntersectPoint(ray);
                    if (face.IsPointInside(xpt)) xPts.Add(xpt);
                }
                if(xPts.Count > 0)
                {
                    var dotsvalue = xPts.Select(p=>p.Dot(w)).ToList();
                    var maxid = dotsvalue.IndexOf(dotsvalue.Max());
                    hitPts.Add(xPts[maxid]);
                }
            }
            #endregion
            List<V3> pts;
            if (wRay.Para.AlignZ)
            {
                pts= hitPts.Select(p=>raySys.ToLocalCoord(p)).ToList();
            }else { pts= hitPts; }
            if(wRay.Para.ShowModel) mRenderCtrl.ShowSceneNode(mTargetNode);
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
        private void ReadMesh()
        {
            OpenFileDialog openfile = new OpenFileDialog() { Filter = "网格数据|*.txt" };
            if (openfile.ShowDialog() == true)
            {
                CloudReader reader = new CloudReader() { FileName = openfile.FileName, Scale = V3.Identity, Format = "xyz" };
                var verts = reader.ReadXYZ();
                Graphic_Tris tris = new Graphic_Tris(0, 100);
                tris.Run(mRenderCtrl, verts);
            }
        }
        private void ReadCir()
        {
            OpenFileDialog openfile = new OpenFileDialog() { Filter = "圆数据|*.txt" };
            if (openfile.ShowDialog() == true)
            {
                StreamReader reader = new StreamReader(openfile.FileName);
                string line = reader.ReadLine();
                while (line != null && line.Length != 0)
                {
                    Circle cir = Circle.CreateCircle(line);
                    Graphic_Lines.DrawCircle(mRenderCtrl, cir);
                    line = reader.ReadLine();
                }
                reader.Close();
            }
            mRenderCtrl.RequestDraw();
        }
        private void ExpPts()
        {
            SaveFileDialog savefile = new SaveFileDialog() { Filter = "xyz文件|*.xyz" };
            if (savefile.ShowDialog() == true)
            {
                StreamWriter writer = new StreamWriter(savefile.FileName);
                var mng = mRenderCtrl.ViewContext.GetSelectionManager();
                var selection = mng.GetSelection();
                if (selection.GetCount() > 0)
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
                    MessageBox.Show("写入完成");
                }
                else
                {
                    var pcn = mRenderCtrl.Scene.FindNodeByUserId(CloudID);
                    if (pcn != null)
                    {
                        GroupSceneNode gp = GroupSceneNode.Cast(pcn);
                        var iter = gp.CreateIterator();
                        while (iter.More())
                        {
                            var item = iter.Current();
                            var n = PointCloud.Cast(item);
                            if (n != null)
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
                    MessageBox.Show("写入完成");
                }
            }
        }
        private void Calib()
        {
            var mng = mRenderCtrl.ViewContext.GetSelectionManager();
            var selection = mng.GetSelection();
            var iter = selection.CreateIterator();
            List<V3> points = new List<V3>();
            while (iter.More())
            {
                var item = iter.Current();
                var value = ConvertVector3.ToV3(item.GetPosition());
                points.Add(value);
                iter.Next();
            }
            WCalib calib = new WCalib(points);
            calib.Show();
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
            var citer = group.CreateIterator();
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
            clip = new Graphic_Clip(mRenderCtrl);
            clip.PolySelection(cloud, bound);
            var segs = clip.GetMeshSegs(100, 100);
            GroupSceneNode lineroot = new GroupSceneNode();
            mRenderCtrl.Scene.AddNode(lineroot);
            Graphic_Lines mesh = new Graphic_Lines();
            mesh.Segs(mRenderCtrl, lineroot, segs, ColorTable.DarkGray);
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
                    V3 cross = vec1.Cross(vec2).Normalized();
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
            PointCloud pcn = PointCloud.Cast(cloud);
            var n = pcn.GetPointCount();
            List<V3> points = new List<V3>();
            for (uint i = 0; i < n; i++)
            {
                var value = pcn.GetPosition(i);
                points.Add(ConvertVector3.ToV3(value));
            }
            CubicMap cMap = CubicMap.CreateCubicMap(points, 5);
            Graphic_CMTX cmtx = new Graphic_CMTX(mRenderCtrl, cMap);
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

        private void SelectByNorm()
        {
            SelectionPara spara = new SelectionPara();
            WSelection wSelection = new WSelection(spara);
            if (wSelection.ShowDialog() == true)
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
                if (points.Count > 0)
                {
                    SceneNode node = mRenderCtrl.Scene.FindNodeByUserId(CloudID);
                    if (node == null)
                    {
                        return;
                    }
                    var group = GroupSceneNode.Cast(node);
                    var citer = group.CreateIterator();
                    PointCloud cloud = PointCloud.Cast(citer.Current());
                    clip = new Graphic_Clip(mRenderCtrl, cloud);
                    var selePts = clip.SelectNeighbours(points, wSelection.Para);
                    clip.ShowPoints(selePts);
                }
            }
        }

        private void SelectByROI()
        {
            WROI wROI = new WROI();
        }
        private void SelectByCir()
        {
            List<V3> points = new List<V3>();
            var mng = mRenderCtrl.ViewContext.GetSelectionManager();
            var selection = mng.GetSelection();
            var iter = selection.CreateIterator();
            while (iter.More())
            {
                var item = iter.Current();
                var value = item.GetPosition();
                points.Add(ConvertVector3.ToV3(value));
                iter.Next();
            }

            if (points.Count > 3)
            {
                SelectionPara spara = new SelectionPara();
                WSelection wSelection = new WSelection(spara);
                if (wSelection.ShowDialog() == true)
                {
                    SceneNode node = mRenderCtrl.Scene.FindNodeByUserId(CloudID);
                    if (node == null)
                    {
                        return;
                    }
                    Circle cir = Circle.CreateCircle(points);
                    var group = GroupSceneNode.Cast(node);
                    var citer = group.CreateIterator();
                    PointCloud cloud = PointCloud.Cast(citer.Current());
                    clip = new Graphic_Clip(mRenderCtrl, cloud);
                    var selePts = clip.SelectInCir(cir, wSelection.Para);
                    clip.ShowPoints(selePts);
                }
            }
        }
        private void FitCircle()
        {
            var prevNode = GroupSceneNode.Cast(mRenderCtrl.Scene.FindNodeByUserId(ClipID));
            if (prevNode != null)
            {
                var selectedPts2 = clip.ClipPoints;
                Circle fitted = Circle.MinimumEnclosingCircle(selectedPts2);
                MVUnity.Plane plane = MVUnity.Plane.CreatePlanePCA(selectedPts2);
                V3 prjCenter = plane.Projection(fitted.Center);
                Circle prjCir = new Circle(prjCenter, fitted.Normal, fitted.R);
                Graphic_Lines.DrawCircle(mRenderCtrl, prjCir);
                WriteLine(prjCir.ToString());
            }
            else
            {
                List<V3> points = new List<V3>();
                var mng = mRenderCtrl.ViewContext.GetSelectionManager();
                var selection = mng.GetSelection();
                var iter = selection.CreateIterator();
                while (iter.More())
                {
                    var item = iter.Current();
                    var value = item.GetPosition();
                    points.Add(ConvertVector3.ToV3(value));
                    iter.Next();
                }
                MVUnity.Plane plane = MVUnity.Plane.CreatePlanePCA(points);
                V3 cirCenter = GeometryTool3D.LeastSquareFitting_Circle(points);
                double disAve = points.Average(e => cirCenter.Distance(e));
                Circle prjCir = new Circle(cirCenter, plane.Norm, disAve);
                Graphic_Lines.DrawCircle(mRenderCtrl, prjCir);
                WriteLine(prjCir.ToString());
            }
            //WriteLine(string.Format("法向" plane.Norm.ToString());
        }

        private void BN_MidPoint_Click(object sender, RoutedEventArgs e)
        {
            List<V3> points = new List<V3>();
            var mng = mRenderCtrl.ViewContext.GetSelectionManager();
            var selection = mng.GetSelection();
            var iter = selection.CreateIterator();
            while (iter.More())
            {
                var item = iter.Current();
                var value = item.GetPosition();
                points.Add(ConvertVector3.ToV3(value));
                iter.Next();
            }
            if (points.Count > 1)
            {
                V3 sum = V3.Zero;
                foreach (var pt in points)
                {
                    sum += pt;
                }
                V3 mid = sum * (1f / points.Count);
                WriteLine(mid.ToString());
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
