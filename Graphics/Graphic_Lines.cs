using AnyCAD.WPF;
using AnyCAD.Foundation;
using System.Collections.Generic;
using System.Linq;
using MVUnity;
using MVUnity.Geometry3D;

namespace MViewer.Graphics
{
    internal class Graphic_Lines
    {
        const ulong MeshObjID = 10;
        const ulong LineObjID = 100;
        Float32Buffer mPositions;
        Float32Buffer mColors;
        public double MaxValue;
        public double MinValue;
        MaterialInstance mat;
        public ColorLookupTable colorLookupTable = new ColorLookupTable();
        static Vector3[] colortable = new Vector3[] { ColorTable.OrangeRed,ColorTable.LimeGreen,ColorTable.RoyalBlue,
        ColorTable.Gold,ColorTable.BlueViolet,ColorTable.Aqua, ColorTable.Sienna,ColorTable.Turquoise};
        public Graphic_Lines()
        {
            mPositions = new Float32Buffer(0);
            mColors = new Float32Buffer(0);
            mat = BasicMaterial.Create("matline");
            mat.SetVertexColors(true);
            mat.SetLineWidth(1);
        }

        public void Run(RenderControl renderControl, GroupSceneNode root, List<Segment> Segments, List<double> Values)
        {
            ColorLookupTable mColorTable = new ColorLookupTable();
            mColorTable.SetMinValue(Values.Min());
            mColorTable.SetMaxValue(Values.Max());
            mColorTable.SetColorMap(ColorMapKeyword.Create(EnumSystemColorMap.Rainbow));
            for (int i = 0; i < Segments.Count; i++)
            {
                Segment edge = Segments[i];
                mPositions.Append((float)edge.Start.X);
                mPositions.Append((float)edge.Start.Y);
                mPositions.Append((float)edge.Start.Z);
                mPositions.Append((float)edge.Destination.X);
                mPositions.Append((float)edge.Destination.Y);
                mPositions.Append((float)edge.Destination.Z);
                var color = mColorTable.GetColor(Values[i * 2]);
                mColors.Append(color.x);
                mColors.Append(color.y);
                mColors.Append(color.z);
                var color2 = mColorTable.GetColor(Values[i * 2 + 1]);
                mColors.Append(color2.x);
                mColors.Append(color2.y);
                mColors.Append(color2.z);
            }
            BufferGeometry buff = new BufferGeometry(EnumPrimitiveType.LINES);
            buff.AddAttribute(EnumAttributeSemantic.Position, EnumAttributeComponents.Three, mPositions);
            buff.AddAttribute(EnumAttributeSemantic.Color, EnumAttributeComponents.Three, mColors);
            PrimitiveSceneNode wireNode = new PrimitiveSceneNode(buff, mat);
            root.AddNode(wireNode);
            renderControl.RequestDraw(EnumUpdateFlags.Scene);
        }

        public void Segs(RenderControl renderControl, GroupSceneNode root, List<Segment2D> Segments)
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                Segment2D edge = Segments[i];
                mPositions.Append((float)edge.Start.X);
                mPositions.Append((float)edge.Start.Y);
                mPositions.Append(0);
                mPositions.Append((float)edge.Destination.X);
                mPositions.Append((float)edge.Destination.Y);
                mPositions.Append(0);
                var color = colortable[i % colortable.Count()];
                mColors.Append(color.x);
                mColors.Append(color.y);
                mColors.Append(color.z);
                var color2 = colortable[i % colortable.Count()];
                mColors.Append(color2.x);
                mColors.Append(color2.y);
                mColors.Append(color2.z);
            }
            BufferGeometry buff = new BufferGeometry(EnumPrimitiveType.LINES);
            buff.AddAttribute(EnumAttributeSemantic.Position, EnumAttributeComponents.Three, mPositions);
            buff.AddAttribute(EnumAttributeSemantic.Color, EnumAttributeComponents.Three, mColors);
            PrimitiveSceneNode wireNode = new PrimitiveSceneNode(buff, mat);
            root.AddNode(wireNode);
            renderControl.RequestDraw(EnumUpdateFlags.Scene);
        }
        public void Segs(RenderControl renderControl, GroupSceneNode root, List<Segment> Segments, Vector3 Color)
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                Segment edge = Segments[i];
                mPositions.Append((float)edge.Start.X);
                mPositions.Append((float)edge.Start.Y);
                mPositions.Append((float)edge.Start.Z);
                mPositions.Append((float)edge.Destination.X);
                mPositions.Append((float)edge.Destination.Y);
                mPositions.Append((float)edge.Destination.Z);
                
                mColors.Append(Color.x);
                mColors.Append(Color.y);
                mColors.Append(Color.z);
                mColors.Append(Color.x);
                mColors.Append(Color.y);
                mColors.Append(Color.z);
            }
            BufferGeometry buff = new BufferGeometry(EnumPrimitiveType.LINES);
            buff.AddAttribute(EnumAttributeSemantic.Position, EnumAttributeComponents.Three, mPositions);
            buff.AddAttribute(EnumAttributeSemantic.Color, EnumAttributeComponents.Three, mColors);
            PrimitiveSceneNode wireNode = new PrimitiveSceneNode(buff, mat);
            root.AddNode(wireNode);
            renderControl.RequestDraw(EnumUpdateFlags.Scene);
        }
        public void Segs(RenderControl renderControl, GroupSceneNode root, List<Segment> Segments)
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                Segment edge = Segments[i];
                mPositions.Append((float)edge.Start.X);
                mPositions.Append((float)edge.Start.Y);
                mPositions.Append((float)edge.Start.Z);
                mPositions.Append((float)edge.Destination.X);
                mPositions.Append((float)edge.Destination.Y);
                mPositions.Append((float)edge.Destination.Z);
                var color = colortable[i % colortable.Count()];
                mColors.Append(color.x);
                mColors.Append(color.y);
                mColors.Append(color.z);
                var color2 = colortable[i % colortable.Count()];
                mColors.Append(color2.x);
                mColors.Append(color2.y);
                mColors.Append(color2.z);
            }
            BufferGeometry buff = new BufferGeometry(EnumPrimitiveType.LINES);
            buff.AddAttribute(EnumAttributeSemantic.Position, EnumAttributeComponents.Three, mPositions);
            buff.AddAttribute(EnumAttributeSemantic.Color, EnumAttributeComponents.Three, mColors);
            PrimitiveSceneNode wireNode = new PrimitiveSceneNode(buff, mat);
            root.AddNode(wireNode);
            renderControl.RequestDraw(EnumUpdateFlags.Scene);
        }
        public void Segs(RenderControl renderControl, GroupSceneNode root, List<Segment> Segments, List<double> Values)
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                Segment edge = Segments[i];
                double value = Values[i];
                var color = colorLookupTable.GetColor(value);
                mPositions.Append((float)edge.Start.X);
                mPositions.Append((float)edge.Start.Y);
                mPositions.Append((float)edge.Start.Z);
                mPositions.Append((float)edge.Destination.X);
                mPositions.Append((float)edge.Destination.Y);
                mPositions.Append((float)edge.Destination.Z);                
                mColors.Append(color.x);
                mColors.Append(color.y);
                mColors.Append(color.z);
                var color2 = colortable[i % colortable.Count()];
                mColors.Append(color2.x);
                mColors.Append(color2.y);
                mColors.Append(color2.z);
            }
            BufferGeometry buff = new BufferGeometry(EnumPrimitiveType.LINES);
            buff.AddAttribute(EnumAttributeSemantic.Position, EnumAttributeComponents.Three, mPositions);
            buff.AddAttribute(EnumAttributeSemantic.Color, EnumAttributeComponents.Three, mColors);
            PrimitiveSceneNode wireNode = new PrimitiveSceneNode(buff, mat);
            root.AddNode(wireNode);
            renderControl.RequestDraw(EnumUpdateFlags.Scene);
        }

        public void Curve(RenderControl renderControl, GroupSceneNode root, List<MVUnity.Arc2D> arcs)
        {
            foreach (var item in arcs)
            {
                Vector3 c = new Vector3((float)item.Center.X, (float)item.Center.Y, 0);
                Vector3 s = new Vector3((float)item.Start.X, (float)item.Start.Y, 0);
                Vector3 e = new Vector3((float)item.Destination.X, (float)item.Destination.Y, 0);
                BufferGeometry shape = GeometryBuilder.CreateArc(c, s, e, 0);
                if(shape != null )
                {
                    PrimitiveSceneNode wireNode = new PrimitiveSceneNode(shape, mat);
                    root.AddNode(wireNode);
                }
            }
            renderControl.RequestDraw(EnumUpdateFlags.Scene);
        }
        public static void DrawCircle(RenderControl renderControl, Circle cir)
        {
            GroupSceneNode prevNode = GroupSceneNode.Cast(renderControl.Scene.FindNodeByUserId(LineObjID));
            if (prevNode != null) { 
                //prevNode.Clear();
                }
            else
            {
                prevNode = new GroupSceneNode();
                prevNode.SetUserId(LineObjID);
                renderControl.Scene.AddNode(prevNode);
            }
            GPnt c = new GPnt(cir.Center.X, cir.Center.Y, cir.Center.Z);
            GDir n = new GDir(cir.Normal.X, cir.Normal.Y, cir.Normal.Z);
            var shape = SketchBuilder.MakeCircle(c, cir.R, n);
            LineMaterial mat = LineMaterial.Create("my-material");
            mat.SetFaceSide(EnumFaceSide.DoubleSide);
            mat.SetOpacity(0.75f);
            mat.SetLineWidth(2);
            mat.SetTransparent(true);
            mat.SetColor(ColorTable.Black);
            var circle = BrepSceneNode.Create(shape, mat, mat);
            prevNode.AddNode(circle);
            renderControl.RequestDraw(EnumUpdateFlags.Scene);
        }
    }
}
