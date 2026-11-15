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
        GroupSceneNode meshRoot;
        public ColorMode VertDisplay;
        public ViewMode ModelDisplay;
        bool Append = false;

        public Graphic_Tris(MeshPara para, GroupSceneNode meshRoot)
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
            this.meshRoot = meshRoot;
        }
        public void ShowTriangleSoup(List<V3> tris)
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
            meshRoot.AddNode(geoNode);
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

        public static List<TriangleMesh> DecomposeFace(TopoShape Shape)
        {            
            var gshape = GRepShape.Create(Shape, null, null, 0, false);
            return DecomposeFace(gshape);
        }

        public static List<TriangleMesh> DecomposeFace(GRepShape gshape)
        {
            List<TriangleMesh> faceMeshs = new List<TriangleMesh>();
            var success = gshape.Build();
            GRepIterator iter = new GRepIterator();

            for (bool init = iter.Initialize(gshape, EnumShapeFilter.Face); iter.More(); iter.Next())
            {                
                var postions = iter.GetPositions();
                var idx = iter.GetIndex();
                uint pnum = postions.GetItemCount();
                uint idnum = idx.GetItemCount();
                V3[] verts = new V3[pnum/3];
                int[] indices = new int[idnum];
                for (uint i = 0; i < pnum / 3; i++)
                {
                    var vert = postions.GetVec3(i * 3);
                    verts[i] = (ConvertVector3.ToV3(vert));
                }
                for (uint i = 0; i < idnum; i++)
                {
                    uint id0 = idx.GetValue(i );
                    indices[i] =(int)id0;
                }
                TriangleMesh mesh = new TriangleMesh(verts, indices);
                faceMeshs.Add(mesh);
            }
            return faceMeshs;
        }
        public void DisplayMesh(TriangleMesh mesh)
        {
            switch (ModelDisplay)
            { 
                default:
                case ViewMode.Wireframe:
                    WireframeMode(mesh); break;
                case ViewMode.Shaded: 
                    ShadedMode(mesh); break;
            }
                
        }
        public void ShadedMode(TriangleMesh mesh)
        {
            if(!Append) meshRoot.Clear();
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
            BufferGeometry buff;
            buff = GeometryBuilder.CreateGeometry(EnumPrimitiveType.TRIANGLES, mPositions, null, null, ib);
            mat.SetVertexColors(false);
            
            NormalCalculator.ComputeVertexNormals(buff);
            PrimitiveSceneNode wireNode = new PrimitiveSceneNode(buff, mat);
            meshRoot.AddNode(wireNode);
        }
        public void WireframeMode(TriangleMesh mesh)
        {
            if (!Append) meshRoot.Clear();
            var pts = mesh.GetPoints();
            var indices = mesh.GetTriangleIndices().Select(e => (uint)e).ToArray();
            mPositions = new Float32Buffer(0);
            Uint32Buffer ib = new Uint32Buffer(2 * (uint)indices.Length);
            for (uint i = 0; i < pts.Length; i++)
            {
                mPositions.Append((float)pts[i].X);
                mPositions.Append((float)pts[i].Y);
                mPositions.Append((float)pts[i].Z);
            }
            for (uint i = 0; i < indices.Length/3; i++)
            {
                uint i0 = indices[i * 3];
                uint i1 = indices[i * 3 + 1];
                uint i2 = indices[i * 3 + 2];
                ib.Append(i0); ib.Append(i1);
                ib.Append(i1); ib.Append(i2);
                ib.Append(i2); ib.Append(i0);
            }
            BufferGeometry buff;
            buff = GeometryBuilder.CreateGeometry(EnumPrimitiveType.LINES, mPositions, null, null, ib);
            mat.SetVertexColors(false);

            NormalCalculator.ComputeVertexNormals(buff);
            PrimitiveSceneNode wireNode = new PrimitiveSceneNode(buff, mat);
            meshRoot.AddNode(wireNode);
        }
    }
}
