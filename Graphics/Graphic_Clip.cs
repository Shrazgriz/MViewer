using AnyCAD.Foundation;
using AnyCAD.WPF;
using MVUnity.Geometry3D;
using MVUnity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVUnity.PointCloud;

namespace MViewer.Graphics
{
    internal class Graphic_Clip
    {
        const ulong CloudID = 1;
        const ulong TestObjID = 3;
        const ulong ColorCloudID = 4;
        const ulong ClipID = 5;
        const ulong OBBID = 6;
        const ulong MeshID = 10;
        const ulong PolygonID = 1000;
        const ulong StiffID = 100;
        Float32Buffer mPositions;
        Float32Buffer mColors;
        System.Windows.Media.Color pColor;
        int Size;
        private PointCloud clipNode;
        Polygon polyBound;
        List<V3> pts;
        RenderControl render;

        public Graphic_Clip(RenderControl control)
        {
            pColor = System.Windows.Media.Colors.Purple;
            Size = 4;
            mPositions = new Float32Buffer(0);
            mColors = new Float32Buffer(0);
            render = control;
        }
        public void BoxSelection(PointCloud cloud, Box Selection)
        {
            var clipPrev = render.Scene.FindNodeByUserId(ClipID);
            if (clipPrev != null)
            {
                render.RemoveSceneNode(clipPrev.GetUuid());
            }
            var count = cloud.GetPointCount();
            for (uint i = 0; i < count; i++)
            {
                var position = cloud.GetPosition(i);
                V3 point = ConvertVector3.ToV3(position);
                if (Inbox(Selection, point))
                {
                    mPositions.Append((float)point.X);
                    mPositions.Append((float)point.Y);
                    mPositions.Append((float)point.Z);
                    mColors.Append(pColor.R);
                    mColors.Append(pColor.G);
                    mColors.Append(pColor.B);

                }
            }
            clipNode = PointCloud.Create(mPositions, mColors, null, Size);
            clipNode.SetUserId(ClipID);
            render.ShowSceneNode(clipNode);
        }

        private bool Inbox(Box box, V3 point)
        {
            V3 relative = point - box.Center;
            double x = relative.Dot(box.MajorOrientation);
            double y = relative.Dot(box.SecondaryOrientation);
            double z = relative.Dot(box.MinorOrientation);
            return (Math.Abs(x) < 0.5f * box.Size.X && Math.Abs(y) < 0.5 * box.Size.Y && Math.Abs(z) < 0.5 * box.Size.Z);
        }

        public void PolySelection(PointCloud cloud, Polygon Selection)
        {
            var clipPrev = render.Scene.FindNodeByUserId(ClipID);
            if (clipPrev != null)
            {
                render.RemoveSceneNode(clipPrev.GetUuid());
            }
            #region 筛选点
            var count = cloud.GetPointCount();
            List<V3> rowPts= new List<V3>();
            for (uint i = 0; i < count; i++)
            {
                var position = cloud.GetPosition(i);
                rowPts.Add(ConvertVector3.ToV3(position));
            }
            List<V3> points= Polygon.PointsInPoly(Selection,rowPts);
            List<V2> projected = new List<V2>();
            foreach (var point in points)
            {
                projected.Add(new V2(point.X, point.Y));
            }
            Polygon simplyBound = Polygon.Simplify(Selection, Tolerance.PolygonSimplifyAreaRation);
            V2[] bound = simplyBound.GetAllVertice().Select(e => new V2(e.X, e.Y)).ToArray();
            #endregion

            #region 构造经线网格longitudes, 输入:points projected bound cellSize nLongi nLatti
            int cellSize = 100;
            int nLongi = 10;
            int nLatti = 10;
            double size = 1f / nLatti;
            RectMap map = RectMap.CreateRectMap(projected, cellSize);
            List<CubicSpline> longitudes = new List<CubicSpline>();
            Segment2D low = new Segment2D(bound[0], bound[1]);
            Segment2D top = new Segment2D(bound[3], bound[2]);
            for (int i = 0; i < nLongi + 1; i++)
            {
                V2 v0 = low.Lerp(size * i);
                V2 v1 = top.Lerp(size * i);
                Segment2D longit = new Segment2D(v0, v1);
                List<V3> knots = new List<V3>();
                for (int j = 0; j < nLatti + 1; j++)
                {
                    V2 XY = longit.Lerp(size * j);
                    var near = map.NeighbourList(XY, 1);
                    if (near.Count == 0) continue;
                    var mah = near.Select(i1 => V2.Manhattan(projected[i1].X, projected[i1].Y, XY.X, XY.Y)).ToList();
                    var ind = near[mah.IndexOf(mah.Min())];
                    knots.Add(points[ind]);
                }
                if (knots.Count < 3) continue;
                CubicSpline longiLine = new CubicSpline(knots);
                longitudes.Add(longiLine);
            }
            #endregion
            #region 基于longtitudes插值, 输入longtitudes, rslx, rsly
            int rslx = 40; int rsly = 40;
            List<ScanRow> scanRows = new List<ScanRow>();
            int rowid = 0;
            int vertid = 0;
            CubicSplineSurface surf = new CubicSplineSurface(longitudes);
            var longtiLines = surf.GetLongtitudes(rslx);
            foreach (var longti in longtiLines)
            {
                var pts = longti.Interpolate(rsly);
                ScanRow row = new ScanRow(rowid++);
                foreach (var vert in pts)
                {
                    Vertex vt = new Vertex(vert, vertid++);
                    row.AppendVertex(vt);
                }
                scanRows.Add(row);
            }
            CellGrid grid = CellGrid.CompileFromAligned(scanRows);
            #endregion
            #region cellgrid to anycad scene node
            
                var positions = CellGrid.ToAnyCADPosition(grid);
                MeshStandardMaterial material = MeshStandardMaterial.Create("cae-material");
                material.SetFaceSide(EnumFaceSide.DoubleSide);
                material.SetVertexColors(true);
                var vPositions = new Float32Buffer(0);
                var vColors = new Float32Buffer(0);
                var color = AnyCAD.Foundation.ColorTable.Auqamarin;
                foreach (var v in positions)
                {
                    vPositions.Append((float)v.X);
                    vPositions.Append((float)v.Y);
                    vPositions.Append((float)v.Z);

                    vColors.Append(color.x);
                    vColors.Append(color.y);
                    vColors.Append(color.z);
                }

                BufferGeometry geometry = new BufferGeometry(EnumPrimitiveType.TRIANGLES);
                geometry.AddAttribute(EnumAttributeSemantic.Position, EnumAttributeComponents.Three, vPositions);
                geometry.AddAttribute(EnumAttributeSemantic.Color, EnumAttributeComponents.Three, vColors);
                NormalCalculator.ComputeVertexNormals(geometry);
                var node = new PrimitiveSceneNode(geometry, material);
                node.SetPickable(false);
                render.ShowSceneNode(node);
            
            #endregion
        }

        //private List<V3> PointsInPoly(List<V3> points, Polygon3D poly, int rsl)
        //{
        //    MVUnity.Plane plane = MVUnity.Plane.CreatePlane(poly.Center, poly.Norm);
        //    ProjectedCloud prj = ProjectedCloud.CreateProjectedCloud(plane, points, rsl);
        //    List<Point3D> projects = prj.GetProjectedPoints();
        //    IEnumerable<Point2D> p2d = projects.Select(e => new Point2D(e.X, e.Y, e.ID));
        //    RectMap map = RectMap.CreateRectMap(p2d, rsl);

        //}
    }
}
