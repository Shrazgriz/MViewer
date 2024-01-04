using AnyCAD.Foundation;
using AnyCAD.WPF;
using MVUnity;
using MVUnity.PointCloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace MViewer.Graphics
{
    internal class Graphic_CMTX
    {
        const ulong CubicMapID = 12;
        RenderControl render;
        LineMaterial lineMat;
        MeshStandardMaterial CubeDiff;
        GroupSceneNode plotModel;        
        public CubicMap MapValue { get; set; }

        public Graphic_CMTX(RenderControl control, CubicMap Value)
        {
            render = control;            
            lineMat = LineMaterial.Create("MatCubeLine");
            lineMat.SetColor(ColorTable.Crimson);
            lineMat.SetLineWidth(4);
            CubeDiff = MeshStandardMaterial.Create("MatCubeDiff");
            CubeDiff.SetColor(ColorTable.Crimson);            
            MapValue = Value;
            
            var node = render.Scene.FindNodeByUserId(CubicMapID);
            if(node != null) { render.Scene.RemoveNode(node); }
            plotModel = new GroupSceneNode();
            plotModel.SetUserId(CubicMapID);
            render.ShowSceneNode(plotModel);
        }

        public void DrawCubicRLMtx(int Threshold)
        {
            CubicRLMtx mtx = MapValue.GetResults(Threshold);
            var ll = MapValue.LowLimit;
            var rslZ = MapValue.RSLZ;
            var rslY= MapValue.RSLY;
            var rslX= MapValue.RSLX;
            foreach (var lPair in mtx.GetAllLayers())
            {
                var lay = lPair.Value;
                double zll = ll.Z+ lPair.Key*rslZ;
                double zul = zll + rslZ;
                foreach (var cPair in lay.GetRLColumns())
                {
                    var col= cPair.Value;
                    double xll = ll.X + cPair.Key * rslX;
                    double xul = xll + rslX;
                    foreach (var rl in col.GetRunLengths())
                    {
                        double yll = ll.Y + rl.Start * rslY;
                        double yul = ll.Y + (rl.End + 1) * rslY;
                        DrawBoxSolid(xll,xul,yll,yul,zll,zul);
                    }
                }
            }
        }
        private void DrawBoxLine(V3 ll, V3 ul)
        {
            {
                //Z
                TopoShape line = SketchBuilder.MakeLine(new GPnt(ll.X, ll.Y, ll.Z), new GPnt(ll.X, ll.Y, ul.Z));
                BrepSceneNode lineNode = BrepSceneNode.Create(line, lineMat, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);

                line = SketchBuilder.MakeLine(new GPnt(ul.X, ll.Y, ll.Z), new GPnt(ul.X, ll.Y, ul.Z));
                lineNode = BrepSceneNode.Create(line, lineMat, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);

                line = SketchBuilder.MakeLine(new GPnt(ul.X, ul.Y, ll.Z), new GPnt(ul.X, ul.Y, ul.Z));
                lineNode = BrepSceneNode.Create(line, lineMat, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);

                line = SketchBuilder.MakeLine(new GPnt(ll.X, ul.Y, ll.Z), new GPnt(ll.X, ul.Y, ul.Z));
                lineNode = BrepSceneNode.Create(line, lineMat, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);
            }
            {
                //Y
                TopoShape line = SketchBuilder.MakeLine(new GPnt(ll.X, ll.Y, ll.Z), new GPnt(ll.X, ul.Y, ll.Z));
                BrepSceneNode lineNode = BrepSceneNode.Create(line, lineMat, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);

                line = SketchBuilder.MakeLine(new GPnt(ul.X, ll.Y, ll.Z), new GPnt(ul.X, ul.Y, ll.Z));
                lineNode = BrepSceneNode.Create(line, lineMat, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);

                line = SketchBuilder.MakeLine(new GPnt(ul.X, ll.Y, ul.Z), new GPnt(ul.X, ul.Y, ul.Z));
                lineNode = BrepSceneNode.Create(line, lineMat, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);

                line = SketchBuilder.MakeLine(new GPnt(ll.X, ll.Y, ul.Z), new GPnt(ll.X, ul.Y, ul.Z));
                lineNode = BrepSceneNode.Create(line, lineMat, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);
            }
            {
                //X
                TopoShape line = SketchBuilder.MakeLine(new GPnt(ll.X, ll.Y, ll.Z), new GPnt(ul.X, ll.Y, ll.Z));
                BrepSceneNode lineNode = BrepSceneNode.Create(line, lineMat, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);

                line = SketchBuilder.MakeLine(new GPnt(ll.X, ul.Y, ll.Z), new GPnt(ul.X, ul.Y, ll.Z));
                lineNode = BrepSceneNode.Create(line, lineMat, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);

                line = SketchBuilder.MakeLine(new GPnt(ll.X, ll.Y, ul.Z), new GPnt(ul.X, ll.Y, ul.Z));
                lineNode = BrepSceneNode.Create(line, lineMat, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);

                line = SketchBuilder.MakeLine(new GPnt(ll.X, ul.Y, ul.Z), new GPnt(ul.X, ul.Y, ul.Z));
                lineNode = BrepSceneNode.Create(line, lineMat, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);
            }
        }
        private void DrawBoxSolid(double xll,double xul,double yll, double yul, double zll,double zul)
        {
            GAx2 ax =new GAx2();
            ax.SetLocation(new GPnt(xll,yll, zll));
            ax.SetXDirection(new GDir(1,0,0));
            ax.SetYDirection(new GDir(0,1,0));
            TopoShape box = ShapeBuilder.MakeBox(ax, xul - xll, yul - yll, zul - zll);
            BrepSceneNode node = BrepSceneNode.Create(box, CubeDiff, lineMat);
            node.SetPickable(false);
            plotModel.AddNode(node);
        }
    }
}
