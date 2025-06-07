using AnyCAD.Foundation;
using AnyCAD.WPF;
using MVUnity;
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
            mat = BasicMaterial.Create("matTris");
            mat.SetVertexColors(true);
            mat.SetLineWidth(2);
        }

        public void Run(RenderControl renderControl, GroupSceneNode root, List<Triangle2D> tris, List<double> Values)
        {
            //double minR = Math.Log(2f);
            //double maxR = Math.Log(100f);
            //ColorLookupTable mColorTable = new ColorLookupTable();
            //mColorTable.SetMinValue(minR);
            //mColorTable.SetMaxValue(maxR);
            //mColorTable.SetColorMap(ColorMapKeyword.Create(EnumSystemColorMap.Rainbow));
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
    }
}
