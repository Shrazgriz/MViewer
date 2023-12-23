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
        const ulong MeshID = 10;
        const ulong Seg2ID = 11;
        const ulong PolygonID = 1000;
        const ulong StiffID = 100;
        RenderControl render;
        LineMaterial lineMat;
        List<Segment2D> seg2s;
        GroupSceneNode plotModel;
        public Graphic_Segs(RenderControl control)
        {
            render=control;
            seg2s = new List<Segment2D>();
            lineMat = LineMaterial.Create("MatSeg2");
            lineMat.SetColor(ColorTable.Crimson);
            lineMat.SetLineWidth(4);
        }

        public bool ReadSeg2(string Filename)
        {
            StreamReader reader = new StreamReader(Filename);
            seg2s=new List<Segment2D>();
            string line = reader.ReadLine();
            while (line != null && line.Length!=0)
            {
                Segment2D seg2 = Segment2D.CreateSegment2D(line);
                seg2s.Add(seg2);
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
            plotModel = new GroupSceneNode();
            plotModel.SetUserId(Seg2ID);
            
            GPntList pts = new GPntList();
            foreach (var seg in seg2s)
            {
                GPnt s = new GPnt(seg.Start.X, seg.Start.Y, 0);
                GPnt e = new GPnt(seg.Destination.X, seg.Destination.Y, 0);
                TopoShape line = SketchBuilder.MakeLine(s,e);
                BrepSceneNode lineNode = BrepSceneNode.Create(line, null, lineMat);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);
            }
            render.ShowSceneNode(plotModel);
        }

        public void Run(string filename)
        {
            if(ReadSeg2(filename))
            {
                DrawSeg2();
            }
        }
    }
}
