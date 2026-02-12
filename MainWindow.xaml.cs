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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public ICommand CADCommand { get; set; }
        public ICommand PCDCommand { get; set; }
        public ICommand Seg2Command { get; set; }
        public ICommand Seg3Command { get; set; }
        public ICommand CirCommand { get; set; }
        public ICommand ExpPtsCommand { get; set; }
        public ICommand M2CCommand { get; set; }
        public ICommand CalibCommand { get; set; }
        public ICommand SeleByNBCommand { get; set; }
        public ICommand SeleByROICommand { get; set; }
        public ICommand FitCirCommand { get; set; }
        public ICommand MeshCommand { get; set; }
        public ICommand SeleByCirCommand { get; set; }
        GroupSceneNode cloudroot;
        const ulong CloudID = 1;
        const ulong ModelID = 2;
        const ulong ClipID = 5;
        bool showPoints;
        Graphic_Clip clip;
        static MaterialInstance matLine;
        static MaterialInstance matFace;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            CADCommand = new Command(param => ReadCAD());
            M2CCommand = new Command(param => ConvertModel());
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

            matLine = LineMaterial.Create("matLine");
            matLine.SetVertexColors(false);
            matLine.SetLineWidth(1);
            matLine.SetColor(ColorTable.Black);
            matFace = MeshStandardMaterial.Create("matFace");
            matFace.SetVertexColors(false);
            matFace.SetColor(ColorTable.WhiteSmoke);
            matFace.SetFaceSide(EnumFaceSide.DoubleSide);
            matFace.SetLineWidth(2);
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
                    GC.Collect();
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
                case ".stl":
                case ".STL":
                    {
                        var shape = StepIO.Open(fileName);
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
            GroupSceneNode modelRoot = GroupSceneNode.Cast(mRenderCtrl.Scene.FindNodeByUserId(ModelID));
            if (modelRoot != null) modelRoot.Clear();
            else
            {
                modelRoot = new GroupSceneNode();
                modelRoot.SetUserId(ModelID);
                mRenderCtrl.Scene.AddNode(modelRoot);
            }
            GC.Collect();
            SceneNode node = null;
            switch (System.IO.Path.GetExtension(dlg.FileName))
            {
                case ".stp":
                case ".step":
                case ".STEP":
                case ".iges":
                case ".igs":
                case ".stl":
                case ".STL":
                    {
                        var shape = readTopo(dlg.FileName);
                        node = BrepSceneNode.Create(shape, matFace, matLine);
                    }
                    break;
                default:
                    {
                        node = SceneIO.Load(dlg.FileName);
                    }
                    break;
            }
            if (node == null)
                return;
            
            modelRoot.AddNode(node);
            mRenderCtrl.RequestDraw(EnumUpdateFlags.Scene);
        }
        private List<V3> rayCastModel(List<Triangle>facets, CatersianSys raySys, double umin, double umax, double vmin ,double vmax, double rayDense, bool AlignCenter)
        {
            V3 rayDir = raySys.ZAxis;
            V3 u = raySys.XAxis;
            V3 v = raySys.YAxis;
            List<Line> rayList = new List<Line>();
            #region 生成射线
            for (double i = umin; i < umax; i += rayDense)
            {
                for (double j = vmin; j < vmax; j += rayDense)
                {
                    V3 ptOnLine = i * u + j * v;
                    Line ray = new Line(rayDir, ptOnLine);
                    rayList.Add(ray);
                }
            }            
            #endregion
            #region 射线检测
            List<V3> hitPts = new List<V3>();
            foreach (var ray in rayList)
            {
                List<V3> xPts = new List<V3>();
                foreach (var face in facets)
                {
                    if (face.IsPointInside(ray.Approach))
                    {
                        MVUnity.Plane triBase = MVUnity.Plane.CreatePlane(face.Vertices[0], face.Vertices[1], face.Vertices[2]);
                        var xpt = triBase.IntersectPoint(ray);
                        xPts.Add(xpt);
                    }
                }
                if (xPts.Count > 0)
                {
                    var dotsvalue = xPts.Select(p => p.Dot(rayDir)).ToList();
                    var maxid = dotsvalue.IndexOf(dotsvalue.Max());
                    hitPts.Add(xPts[maxid]);
                }
            }
            if (AlignCenter) { return hitPts.Select(p => raySys.ToLocalCoord(p)).ToList(); }
            else { return hitPts; }
            #endregion
        }
        private void ConvertModel()
        {
            bool findModel = false;
            TopoShape mshape = null;
            GroupSceneNode modelRoot = GroupSceneNode.Cast(mRenderCtrl.Scene.FindNodeByUserId(ModelID));
            if (modelRoot != null)
            {
                var iter = modelRoot.CreateIterator();
                if (iter.More()) {
                    var child = BrepSceneNode.Cast(iter.Current());
                    if (child != null) {
                        findModel = true;
                        mshape = child.GetTopoShape();
                    }
                }
            }
            if (findModel)
            {
                DateTime s = DateTime.Now;
                WRayHits wRay = new WRayHits(new RayHitsPara());
                if (wRay.ShowDialog() != true) return;
                var w = wRay.Para.Direction;
                V3 rayDir = w;
                var d = wRay.Para.Resolution;
                double rayDense = d;
                #region 估算射线范围
                List<V3> axis = new List<V3>() { V3.Forward, V3.Right, V3.Up };
                List<double> axisDot = axis.Select(a => Math.Abs(a.Dot(rayDir))).ToList();
                int minId = axisDot.IndexOf(axisDot.Min());
                V3 v = rayDir.Cross(axis[minId]).Normalized();
                V3 u = v.Cross(rayDir).Normalized();
                ShapeExplor sExp = new ShapeExplor();
                sExp.AddShape(mshape);
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
                List<V3> boxPts = new List<V3>() { p0, p1, p2, p3, p4, p5, p6, p7 };
                List<double> uValues = boxPts.Select(e => e.Dot(u)).ToList();
                List<double> vValues = boxPts.Select(e => e.Dot(v)).ToList();
                CatersianSys raySys0 = CatersianSys.CreateSysOXN(mid, u, rayDir);
                CatersianSys raySys1 = CatersianSys.CreateSysOXN(mid, u, rayDir);
                double umid = 0.5f * (uValues.Min() + uValues.Max());
                #endregion

                #region 模型分解
                List<Triangle> facets0 = new List<Triangle>();
                List<Triangle> facets1 = new List<Triangle>();
                var gshape = GRepShape.Create(mshape, matFace, matLine, 0, false);
                var success = gshape.Build();
                GRepIterator iter = new GRepIterator();
                for (bool init = iter.Initialize(gshape, EnumShapeFilter.Face); iter.More(); iter.Next())
                {
                    var postions = iter.GetPositions();
                    var idx = iter.GetIndex();
                    uint pnum = postions.GetItemCount();
                    uint idnum = idx.GetItemCount();
                    List<V3> verts = new List<V3>();
                    for (uint i = 0; i < pnum / 3; i++)
                    {
                        var vert = postions.GetVec3(i * 3);
                        verts.Add(ConvertVector3.ToV3(vert));
                    }
                    for (uint i = 0; i < idnum / 3; i++)
                    {
                        uint id0 = idx.GetValue(i * 3);
                        uint id1 = idx.GetValue(i * 3 + 1);
                        uint id2 = idx.GetValue(i * 3 + 2);
                        V3 v0 = verts[(int)id0];
                        V3 v1 = verts[(int)id1];
                        V3 v2 = verts[(int)id2];
                        if (Math.Abs(((v0-v1).Cross(v1-v2)).Normalized().Dot(rayDir)) < 0.01f) continue;
                        Triangle tri = new Triangle(new V3[3] { verts[(int)id0], verts[(int)id1], verts[(int)id2] });                        
                        facets0.Add(tri);
                        facets1.Add(new Triangle(tri));
                    }
                }
                #endregion
                List<V3> hit0 = null;
                List<V3> hit1 = null;
                List<V3> hitpts = new List<V3>();
                V3 rayDir0 = new V3(rayDir);
                V3 rayDir1 = new V3(rayDir);
                try
                {
                    // 5. 配置并行选项：强制使用2个核心（MaxDegreeOfParallelism=2）
                    ParallelOptions parallelOptions = new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 2 // 明确指定仅使用2个CPU核心
                    };

                    // 6. 并行执行两个任务
                    Parallel.Invoke(parallelOptions,
                        // 任务1：处理第一部分点
                        () => { hit0 = rayCastModel(facets0, raySys0, uValues.Min(), umid, vValues.Min(), vValues.Max(),d, wRay.Para.AlignCenter); },
                        // 任务2：处理第二部分点
                        () => { hit1 = rayCastModel(facets1, raySys1, umid, uValues.Max(), vValues.Min(), vValues.Max(),d, wRay.Para.AlignCenter); }
                    );
                    if (hit0 != null & hit1 != null)
                    {
                        hitpts = hit0;
                        hitpts.AddRange(hit1);
                    }
                }
                catch (AggregateException ex)
                {
                    // 捕获并行任务中的异常
                    foreach (var innerEx in ex.InnerExceptions)
                    {
                        Console.WriteLine($"离散失败:{innerEx.Message}");
                    }
                }
                #region 可视化
                Graphic_Cloud cloud = new Graphic_Cloud();
                cloud.ShowCloud(hitpts, mRenderCtrl);
                #endregion
                TimeSpan ts = DateTime.Now - s;
                WriteLine($"points {hitpts.Count}, time cost:{ts.TotalMilliseconds}");
            }
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
