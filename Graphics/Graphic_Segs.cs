using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyCAD.Foundation;
using AnyCAD.WPF;
using MVUnity.Geometry3D;
using MVUnity;
using System.IO;
using System.Windows.Interop;
using System.Windows.Media;

namespace MViewer.Graphics
{
    internal class Graphic_Segs
    {
        const ulong CloudID = 1;
        const ulong ColorCloudID = 4;
        const ulong ClipID = 5;
        const ulong OBBID = 6;
        const ulong Seg2ID = 11;
        const ulong Seg3ID = 12;
        const ulong PolygonID = 1000;
        const ulong MeshObjID = 10;
        const ulong LineObjID = 100;

        RenderControl render;
        LineMaterial lineMat;
        List<Segment2D> seg2s;
        List<Segment> seg3s;
        List<Arc2D> arc2s;
        GroupSceneNode plot2Model;
        GroupSceneNode plot3Model;
        public Graphic_Segs(RenderControl control)
        {
            render=control;
            seg2s = new List<Segment2D>();
            seg3s = new List<Segment>();
            arc2s = new List<Arc2D>();
            lineMat = LineMaterial.Create("MatSeg2");
            lineMat.SetColor(ColorTable.Crimson);
            lineMat.SetLineWidth(4);
        }

        public bool ReadSeg2(string Filename)
        {
            StreamReader reader = new StreamReader(Filename);
            seg2s.Clear();
            arc2s.Clear();
            string line = reader.ReadLine();
            while (line != null && line.Length!=0)
            {
                var split = line.Split(';');
                switch (split[0])
                {
                    case "Segment2D":
                        Segment2D seg2 = Segment2D.CreateSegment2D(line);
                        seg2s.Add(seg2);
                        break;
                    case "Arc2D":
                        Arc2D arc2 = Arc2D.CreateArc2D(line);
                        arc2s.Add(arc2);
                        break;
                    default:
                        break;
                }
                line = reader.ReadLine();
            }
            return true;
        }
        public bool ReadSeg3(string Filename)
        {
            StreamReader reader = new StreamReader(Filename);
            seg3s.Clear();
            string line = reader.ReadLine();
            while (line != null && line.Length != 0)
            {
                Segment seg3 = Segment.CreateSegment(line);
                seg3s.Add(seg3);
                line = reader.ReadLine();
            }
            return true;
        }
        public void DrawSeg2()
        {
            var prev = render.Scene.FindNodeByUserId(Seg2ID);
            if(prev != null)
            {
                render.Scene.RemoveNode(prev);
            }
            plot2Model = new GroupSceneNode();
            plot2Model.SetUserId(Seg2ID);
            
            GPntList pts = new GPntList();
            foreach (var seg in seg2s)
            {
                GPnt s = new GPnt(seg.Start.X, seg.Start.Y, 0);
                GPnt e = new GPnt(seg.Destination.X, seg.Destination.Y, 0);
                TopoShape line = SketchBuilder.MakeLine(s,e);
                BrepSceneNode lineNode = BrepSceneNode.Create(line, null, lineMat);
                lineNode.SetPickable(false);
                plot2Model.AddNode(lineNode);
            }
            foreach (var arc in arc2s)
            {
                GPnt s = new GPnt(arc.Start.X, arc.Start.Y, 0);
                GPnt e = new GPnt(arc.Destination.X, arc.Destination.Y, 0);
                //V2 m = arc.Slerp(0.5f);
                //GPnt c = new GPnt(m.X, m.Y, 0);
                GPnt c = new GPnt(arc.Center.X, arc.Center.Y, 0);
                TopoShape outline = SketchBuilder.MakeArcOfCircle(s, e, c);
                BrepSceneNode entity;
                entity = BrepSceneNode.Create(outline, null, lineMat);                
                if (entity is null) return;
                entity.SetPickable(false);
                plot2Model.AddNode(entity);
            }
            render.ShowSceneNode(plot2Model);
        }

        public void DrawSeg3()
        {
            var prev = render.Scene.FindNodeByUserId(Seg2ID);
            if (prev != null)
            {
                render.Scene.RemoveNode(prev);
            }
            plot3Model = new GroupSceneNode();
            plot3Model.SetUserId(Seg3ID);

            GPntList pts = new GPntList();
            foreach (var seg in seg3s)
            {
                GPnt s = new GPnt(seg.Start.X, seg.Start.Y, seg.Start.Z);
                GPnt e = new GPnt(seg.Destination.X, seg.Destination.Y, seg.Destination.Z);
                TopoShape line = SketchBuilder.MakeLine(s, e);
                BrepSceneNode lineNode = BrepSceneNode.Create(line, null, lineMat);
                lineNode.SetPickable(false);
                plot3Model.AddNode(lineNode);
            }
            render.ShowSceneNode(plot3Model);
        }
        public void Run2(string filename)
        {
            if(ReadSeg2(filename))
            {
                DrawSeg2();
            }
        }
        public void Run3(string filename)
        {
            if (ReadSeg3(filename))
            {
                DrawSeg3();
            }
        }
    }
}
