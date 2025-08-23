using AnyCAD.Foundation;
using AnyCAD.WPF;
using MVUnity;
using MVUnity.Geometry3D;
using System.Collections.Generic;
using System.Linq;

namespace MViewer.Graphics
{
    internal class Graphic_Tris
    {
        static Vector3[] colortable = new Vector3[] { ColorTable.OrangeRed,ColorTable.LimeGreen,ColorTable.RoyalBlue,
        ColorTable.Gold,ColorTable.BlueViolet,ColorTable.Aqua, ColorTable.Sienna,ColorTable.Turquoise};
        Float32Buffer mPositions;
        Float32Buffer mColors;
        public double MaxValue;
        public double MinValue;
        MaterialInstance mat;
        const ulong MeshObjID = 10;
        const ulong LineObjID = 100;

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
            //buff.AddAttribute(EnumAttributeSemantic.Color, EnumAttributeComponents.Three, mColors);
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
            //PaletteWidget pw = new PaletteWidget();
            //pw.Update(mColorTable);
            //renderControl.ShowSceneNode(pw);
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

        public static List<Triangle> DecomposeFace(TopoShape face)
        {
            ShapeExplor fexp = new ShapeExplor();
            fexp.AddShape(face);
            fexp.Build();
            var vnum = fexp.GetVertexCount();
            List<Triangle> facets = new List<Triangle>();
            List<V3> fverts = new List<V3>();
            var fp0 = fexp.GetVertex(0).GetPoint();
            var fp1 = fexp.GetVertex(1).GetPoint();
            var pv0 = new V3(fp0.x, fp0.y, fp0.z);
            var pv1 = new V3(fp1.x, fp1.y, fp1.z);
            fverts.Add(pv0);
            fverts.Add(pv1);
            V3 fp = pv0;
            for (uint i = 2; i < vnum/2; i++)
            {
                var gp0 = fexp.GetVertex(2*i).GetPoint();
                var gp1 = fexp.GetVertex(2 * i + 1).GetPoint();
                var v0 = new V3(gp0.x, gp0.y, gp0.z);
                var v1 = new V3(gp1.x, gp1.y, gp1.z);
                bool containV0 = (pv0.Distance(v0) < 0.01f) | (pv1.Distance(v0) < 0.01f) | (fp.Distance(v0) < 0.01f);
                bool containV1 = (pv0.Distance(v1) < 0.01f) | (pv1.Distance(v1) < 0.01f) | (fp.Distance(v1) < 0.01f);
                if (!containV0) fverts.Add(v0);
                if (!containV1) fverts.Add(v1);
                pv0 = v0;
                pv1 = v1;
            }
            
            for (int i = 1; i < fverts.Count-1; i++)
            {
                Triangle tri = new Triangle(new V3[3] { fp, fverts[i], fverts[i + 1] });
                facets.Add(tri);
            }
            return facets;
        }
    }
}
