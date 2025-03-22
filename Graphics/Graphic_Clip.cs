using AnyCAD.Foundation;
using AnyCAD.WPF;
using MVUnity.Geometry3D;
using MVUnity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MVUnity.PointCloud;
using System.Runtime.CompilerServices;

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
        CubicSplineSurface surf;

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
            List<V3> rowPts = new List<V3>();
            for (uint i = 0; i < count; i++)
            {
                var position = cloud.GetPosition(i);
                rowPts.Add(ConvertVector3.ToV3(position));
            }
            List<V3> points = Polygon.PointsInPoly(Selection, rowPts);
            List<V2> projected = new List<V2>();
            foreach (var point in points)
            {
                projected.Add(new V2(point.X, point.Y));
            }
            Polygon simplyBound = Polygon.Simplify(Selection, Tolerance.PolygonSimplifyAreaRation);
            V2[] bound = simplyBound.GetAllVertice().Select(e => new V2(e.X, e.Y)).ToArray();
            #endregion

            splineSurface(bound, points, projected);
        }

        public void XYSplineSurface(List<V3> Points)
        {
            List<V2> projected = new List<V2>();
            foreach (var point in Points)
            {
                projected.Add(new V2(point.X, point.Y));
            }
            Rectangle2D rect = Rectangle2D.AlphaORect(projected, 100);
            V2[] bound = rect.Get4Points().ToArray();
            splineSurface(bound, Points, projected);
        }
        public List<Segment> GetMeshSegs(int rslx, int rsly)
        {
            var longtiLines = surf.GetLongtitudes(rslx);
            List<List<V3>> meshpts = new List<List<V3>>();
            List<Segment> meshsegs = new List<Segment>();
            foreach (var longti in longtiLines)
            {
                var pts = longti.Interpolate(rsly);
                meshpts.Add(pts);
                Polyline longtiline = new Polyline(pts);
                meshsegs.AddRange(longtiline.ToSegments());
            }
            for (int i = 0; i < rsly-1; i++)
            {
                List<V3> segs = new List<V3>();
                for (int j = 0; j < rslx; j++)
                {
                    Segment seg = new Segment(meshpts[i][j], meshpts[i + 1][j]);
                    meshsegs.Add(seg);
                }
            }
            return meshsegs;
        }
        /// <summary>
        /// 筛选平面点
        /// </summary>
        /// <param name="cloud"></param>
        /// <param name="Coord"></param>
        /// <param name="DotValue"></param>
        /// <returns></returns>
        public static List<V3> SelectByNorm(PointCloud cloud, V3 Coord, double DotValue)
        {
            #region 筛选点
            int cellSize = 100;
            var count = cloud.GetPointCount();
            List<V3> rawPts = new List<V3>();
            for (uint i = 0; i < count; i++)
            {
                var position = cloud.GetPosition(i);
                var pt = ConvertVector3.ToV3(position);
                rawPts.Add(pt);
            }
            List<double> dist = rawPts.Select(p => p.Distance(Coord)).ToList();
            int pID = dist.IndexOf(dist.Min());
            #endregion
            #region 构造网格并法向筛选
            var result = Delaunator.NeighbourPoints_NormCheck(rawPts, pID, DotValue);
            return result;
            #endregion
        }

        private void splineSurface(V2[] bound, List<V3> points, List<V2> projected)
        {
            #region 构造经线网格longitudes, 输入:points projected bound cellSize nLongi nLatti
            int cellSize = 100;
            int nLongi = 20;
            int nLatti = 20;
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
                    var mah = near.Select(i1 => V2.Manhattan(projected[i1].X, projected[i1].Y, XY.X, XY.Y)).ToArray();
                    var nearest10 = GetSmallestNumbersWithPositions(mah, 4);
                    double zave = nearest10.Average(p => points[near[p.Key]].Z);
                    V3 pt = new V3(XY.X, XY.Y, zave);
                    knots.Add(pt);
                }
                if (knots.Count < 3) continue;
                CubicSpline longiLine = new CubicSpline(knots);
                longitudes.Add(longiLine);
            }
            #endregion
            #region 基于longtitudes插值, 输入longtitudes, rslx, rsly
            int rslx = 20; int rsly = 20;
            List<ScanRow> scanRows = new List<ScanRow>();
            int rowid = 0;
            int vertid = 0;
            surf = new CubicSplineSurface(longitudes);
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
        private class NumberWithIndex
        {
            public double Value { get; set; }
            public int Index { get; set; }

            public NumberWithIndex(double value, int index)
            {
                Value = value;
                Index = index;
            }
        }

        // 自定义比较器类，实现 IComparer<NumberWithIndex> 接口
        private class NumberWithIndexComparer : IComparer<NumberWithIndex>
        {
            public int Compare(NumberWithIndex x, NumberWithIndex y)
            {
                int cmp = x.Value.CompareTo(y.Value);
                return cmp != 0 ? cmp : x.Index.CompareTo(y.Index);
            }
        }
        static Dictionary<int, double> GetSmallestNumbersWithPositions(double[] numbers, int Count)
        {
            // 最小堆（优先队列）
            var maxHeap = new SortedSet<NumberWithIndex>(new NumberWithIndexComparer());

            for (int i = 0; i < numbers.Length; i++)
            {
                if (maxHeap.Count < Count)
                {
                    maxHeap.Add(new NumberWithIndex(numbers[i], i));
                }
                else if (numbers[i] < maxHeap.Max.Value)
                {
                    maxHeap.Remove(maxHeap.Max);
                    maxHeap.Add(new NumberWithIndex(numbers[i], i));
                }
            }

            // 存储结果
            var result = new Dictionary<int, double>();
            foreach (var item in maxHeap)
            {
                result[item.Index] = item.Value;
            }
            return result;
        }
    }
}
