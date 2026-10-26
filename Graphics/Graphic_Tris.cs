using AnyCAD.Foundation;
using AnyCAD.WPF;
using MVUnity;
using MVUnity.Geometry3D;
using MVUnity.PointCloud;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace MViewer.Graphics
{
    internal class Graphic_Tris
    {
        static Vector3[] colortable = new Vector3[] { ColorTable.OrangeRed,ColorTable.LimeGreen,ColorTable.RoyalBlue,
        ColorTable.Gold,ColorTable.BlueViolet,ColorTable.Aqua, ColorTable.Sienna,ColorTable.Turquoise};
        public Color PColor;
        Float32Buffer mPositions;
        Float32Buffer mColors;
        public double MaxValue;
        public double MinValue;
        MaterialInstance mat;
        const ulong MeshObjID = 3;
        public ColorMode VertDisplay;
        public ViewMode ModelDisplay;
        bool Append = false;

        public Graphic_Tris(double min, double max)
        {
            MaxValue = max;
            MinValue = min;
            mPositions = new Float32Buffer(0);
            mColors = new Float32Buffer(0);
            mat = MeshPhongMaterial.Create("matTris");
            mat.SetVertexColors(false);
            mat.SetColor(ColorTable.RoyalBlue);
            mat.SetFaceSide(EnumFaceSide.DoubleSide);
            mat.SetLineWidth(2);
        }
        public Graphic_Tris(MeshPara para)
        {
            mPositions = new Float32Buffer(0);
            mColors = new Float32Buffer(0);
            PColor = para.MeshColor;
            mat = MeshPhongMaterial.Create("matTris");
            if (para.ModelDisplay == ViewMode.Shaded)
                mat.SetVertexColors(false);
            else mat.SetVertexColors(true);
            float r = PColor.R / 255f;
            float g = PColor.G / 255f;
            float b = PColor.B / 255f;
            mat.SetColor(new Vector3(r, g, b));
            mat.SetFaceSide(EnumFaceSide.DoubleSide);
            mat.SetLineWidth((byte)para.Thickness);
            Append = para.Append;
            VertDisplay = para.VertDisplay;
            ModelDisplay = para.ModelDisplay;
        }
        public void Run(RenderControl renderControl, List<V3> tris)
        {
            Vector3 color = ColorTable.RoyalBlue;
            for (int i = 0; i < tris.Count; i++)
            {
                mPositions.Append((float)tris[i].X);
                mPositions.Append((float)tris[i].Y);
                mPositions.Append((float)tris[i].Z);
                mColors.Append(color.x);
                mColors.Append(color.y);
                mColors.Append(color.z);
            }
            BufferGeometry buff = new BufferGeometry(EnumPrimitiveType.TRIANGLES);
            buff.AddAttribute(EnumAttributeSemantic.Position, EnumAttributeComponents.Three, mPositions);
            NormalCalculator.ComputeVertexNormals(buff);
            PrimitiveSceneNode geoNode = new PrimitiveSceneNode(buff, mat);
            renderControl.ShowSceneNode(geoNode);
            renderControl.RequestDraw(EnumUpdateFlags.Scene);
        }
        public void Run(RenderControl renderControl, GroupSceneNode root, List<Triangle2D> tris, List<double> Values)
        {
            for (int i = 0; i < tris.Count; i++)
            {
                var tri = tris[i];
                var pts = tri.Vertices;
                Vector3 color;
                //var color = mColorTable.GetColor(Math.Log(Values[i]));
                if (Values[i] > MaxValue) { color = ColorTable.Red; }
                else if (Values[i] < MinValue) { color = ColorTable.Blue; }
                else { color = ColorTable.Green; }
                for (int j = 0; j < 3; j++)
                {
                    mPositions.Append((float)pts[j].X);
                    mPositions.Append((float)pts[j].Y);
                    mPositions.Append(0);
                    mColors.Append(color.x);
                    mColors.Append(color.y);
                    mColors.Append(color.z);
                }
            }
            BufferGeometry buff = new BufferGeometry(EnumPrimitiveType.TRIANGLES);
            buff.AddAttribute(EnumAttributeSemantic.Position, EnumAttributeComponents.Three, mPositions);
            buff.AddAttribute(EnumAttributeSemantic.Color, EnumAttributeComponents.Three, mColors);
            NormalCalculator.ComputeVertexNormals(buff);
            PrimitiveSceneNode geoNode = new PrimitiveSceneNode(buff, mat);
            root.AddNode(geoNode);
            renderControl.RequestDraw(EnumUpdateFlags.Scene);
        }

        public void RunContourMap(RenderControl renderControl, GroupSceneNode root, List<Triangle2D> tris, List<double> Values)
        {
            root.Clear();
            ColorLookupTable mColorTable = new ColorLookupTable();
            mColorTable.SetMinValue(MinValue);
            mColorTable.SetMaxValue(MaxValue);
            mColorTable.SetColorMap(ColorMapKeyword.Create(EnumSystemColorMap.Rainbow));
            for (int i = 0; i < tris.Count; i++)
            {
                var tri = tris[i];
                var pts = tri.Vertices;
                var color = mColorTable.GetColor(Values[i]);
                for (int j = 0; j < 3; j++)
                {
                    mPositions.Append((float)pts[j].X);
                    mPositions.Append((float)pts[j].Y);
                    mPositions.Append(0);
                    mColors.Append(color.x);
                    mColors.Append(color.y);
                    mColors.Append(color.z);
                }
            }
            BufferGeometry buff = new BufferGeometry(EnumPrimitiveType.TRIANGLES);
            buff.AddAttribute(EnumAttributeSemantic.Position, EnumAttributeComponents.Three, mPositions);
            buff.AddAttribute(EnumAttributeSemantic.Color, EnumAttributeComponents.Three, mColors);
            NormalCalculator.ComputeVertexNormals(buff);
            PrimitiveSceneNode geoNode = new PrimitiveSceneNode(buff, mat);
            root.AddNode(geoNode);
            var prev = renderControl.Scene.FindNodeByUserId(MeshObjID);
            if (!(prev is null))
            {
                renderControl.Scene.RemoveNode(prev);
            }
            PaletteWidget pw = new PaletteWidget();
            pw.Update(mColorTable);
            pw.SetUserId(MeshObjID);
            renderControl.ShowSceneNode(pw);
            renderControl.RequestDraw(EnumUpdateFlags.Scene);
        }

        public void RunTris(RenderControl renderControl, GroupSceneNode root, List<List<Triangle2D>> trigroups)
        {
            for (int k = 0; k < trigroups.Count; k++)
            {
                Vector3 color = colortable[k % colortable.Count()];
                var tris = trigroups[k];
                for (int i = 0; i < tris.Count; i++)
                {
                    var tri = tris[i];
                    var pts = tri.Vertices;
                    for (int j = 0; j < 3; j++)
                    {
                        mPositions.Append((float)pts[j].X);
                        mPositions.Append((float)pts[j].Y);
                        mPositions.Append(0);
                        mColors.Append(color.x);
                        mColors.Append(color.y);
                        mColors.Append(color.z);
                    }
                }
            }

            BufferGeometry buff = new BufferGeometry(EnumPrimitiveType.TRIANGLES);
            buff.AddAttribute(EnumAttributeSemantic.Position, EnumAttributeComponents.Three, mPositions);
            buff.AddAttribute(EnumAttributeSemantic.Color, EnumAttributeComponents.Three, mColors);
            NormalCalculator.ComputeVertexNormals(buff);
            PrimitiveSceneNode wireNode = new PrimitiveSceneNode(buff, mat);
            root.AddNode(wireNode);
            renderControl.RequestDraw(EnumUpdateFlags.Scene);
        }

        public static List<Triangle> DecomposeFace(TopoShape Shape)
        {            
            var gshape = GRepShape.Create(Shape, null, null, 0, false);            
            return DecomposeFace(gshape);
        }

        public static List<Triangle> DecomposeFace(GRepShape gshape)
        {
            List<Triangle> facets = new List<Triangle>();
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
                    Triangle tri = new Triangle(new V3[3] { verts[(int)id0], verts[(int)id1], verts[(int)id2] });
                    facets.Add(tri);
                }
            }
            return facets;
        }
        public void DisplayMesh(RenderControl renderControl, TriangleMesh mesh)
        {
            var prevNode = GroupSceneNode.Cast(renderControl.Scene.FindNodeByUserId(MeshObjID));
            if (prevNode == null)
            {
                prevNode = new GroupSceneNode();
                prevNode.SetUserId(MeshObjID);
                renderControl.Scene.AddNode(prevNode);
            }
            if(!Append) prevNode.Clear();
            BufferGeometry buff;
            switch (ModelDisplay)
            {
                case ViewMode.Wireframe:
                    buff = new BufferGeometry(EnumPrimitiveType.LINES);
                    mat.SetVertexColors(true);
                    break;
                default:
                case ViewMode.Shaded:
                    buff = new BufferGeometry(EnumPrimitiveType.TRIANGLES);
                    mat.SetVertexColors(false);
                    break;
            }
            
            var pts = mesh.GetPoints();
            var indices = mesh.GetTriangleIndices().Select(e=>(uint)e).ToArray();
            mPositions = new Float32Buffer(0);
            Uint32Buffer ib = new Uint32Buffer((uint)indices.Length);
            for (uint i = 0; i < pts.Length; i++)
            {
                mPositions.Append((float)pts[i].X);
                mPositions.Append((float)pts[i].Y);
                mPositions.Append((float)pts[i].Z);
            }
            for(uint i = 0; i < indices.Length; i++)
            {
                ib.Append(indices[i]);
            }
            //mPositions = new Float32Buffer(2 * 3 * (uint)pts.Length);
            //for (uint i = 0; i < mesh.GetTriCount(); i++)
            //{
            //    uint i0 = indices[i * 3];
            //    uint i1 = indices[i * 3 + 1];
            //    uint i2 = indices[i * 3 + 2];
            //    mPositions.Append3(new Vector3((float)pts[i0].X, (float)pts[i0].Y, (float)pts[i0].Z));
            //    mPositions.Append3(new Vector3((float)pts[i1].X, (float)pts[i1].Y, (float)pts[i1].Z));
            //    mPositions.Append3(new Vector3((float)pts[i1].X, (float)pts[i1].Y, (float)pts[i1].Z));
            //    mPositions.Append3(new Vector3((float)pts[i2].X, (float)pts[i2].Y, (float)pts[i2].Z));
            //    mPositions.Append3(new Vector3((float)pts[i2].X, (float)pts[i2].Y, (float)pts[i2].Z));
            //    mPositions.Append3(new Vector3((float)pts[i0].X, (float)pts[i0].Y, (float)pts[i0].Z));
            //}
            buff = GeometryBuilder.CreateGeometry(EnumPrimitiveType.TRIANGLES, mPositions, null, null, ib);
            //buff.AddAttribute(EnumAttributeSemantic.Position, EnumAttributeComponents.Three, mPositions);
            NormalCalculator.ComputeVertexNormals(buff);
            PrimitiveSceneNode wireNode = new PrimitiveSceneNode(buff, mat);
            prevNode.AddNode(wireNode);
            renderControl.RequestDraw(EnumUpdateFlags.Scene);
        }
    }
}
