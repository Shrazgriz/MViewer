using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVUnity;
using MVUnity.Geometry3D;
using AnyCAD.Foundation;
using AnyCAD.WPF;

namespace MViewer.Graphics
{
    internal class Graphic_AABox:ABB
    {
        LineDashedMaterial _material;
        public Graphic_AABox() : base()
        {
            _material = LineDashedMaterial.Create("AABoxMat");
            _material.SetColor(ColorTable.Black);
            _material.SetLineWidth(2);
            _material.SetDashSize(12);
        }
        public void ShowBox(RenderControl render , GroupSceneNode root)
        {
            var edges = this.Get12Edges();
            Segs(render,root, edges);
        }

        private void Segs(RenderControl renderControl, GroupSceneNode root, List<Segment> Segments)
        {
            Float32Buffer mPositions = new Float32Buffer(0);
            for (int i = 0; i < Segments.Count; i++)
            {
                Segment edge = Segments[i];
                mPositions.Append((float)edge.Start.X);
                mPositions.Append((float)edge.Start.Y);
                mPositions.Append((float)edge.Start.Z);
                mPositions.Append((float)edge.Destination.X);
                mPositions.Append((float)edge.Destination.Y);
                mPositions.Append((float)edge.Destination.Z);
            }
            BufferGeometry buff = new BufferGeometry(EnumPrimitiveType.LINES);
            buff.AddAttribute(EnumAttributeSemantic.Position, EnumAttributeComponents.Three, mPositions);
            PrimitiveSceneNode wireNode = new PrimitiveSceneNode(buff, _material);
            root.AddNode(wireNode);
            renderControl.RequestDraw(EnumUpdateFlags.Scene);
        }
    }
}
