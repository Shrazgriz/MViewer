﻿using AnyCAD.Foundation;
using AnyCAD.WPF;
using MVUnity.PointCloud;
using MVUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MViewer.Graphics
{
    internal class Graphic_Cloud
    {
        const int CloudID = 1;
        const int BoxID = 2;
        static Float32Buffer mPositions;
        static Float32Buffer mColors;
        MVUnity.Exchange.CloudReader filereader;
        System.Windows.Media.Color pColor;
        int Size;
        public int IntervalX;
        public int IntervalY;
        public int IntervalZ;
        public int MaxRow, MaxCol;
        List<V3> pts;
        public Graphic_Cloud(MVUnity.Exchange.CloudReader cloud)
        {
            filereader = cloud;
            pColor = System.Windows.Media.Colors.Red;
            Size = 2;
            IntervalX = 100;
            IntervalY = 100;
            IntervalZ = 100;
            MaxCol = 0;
            MaxRow = 0;
        }
        bool ReadData()
        {
            mPositions = new Float32Buffer(0);
            mColors = new Float32Buffer(0);
            pts = new List<V3>();
            var ext = Path.Extension(filereader.FileName);
            switch (ext)
            {
                case ".xyz":
                    ReadXYZ();
                    break;
                case ".txt":
                    ReadXYZ();
                    break;
                case ".pcd":
                    ReadPCD();
                    break;
                case ".asc":
                    ReadASC();
                    break;
                case ".ply":
                    ReadPLY();
                    break;
                default:
                    break;
            }

            return true;
        }
        private bool ReadXYZ()
        {
            if (filereader.Format.Contains('r') & filereader.Format.Contains('c'))
            {
                #region 有序点云
                List<List<ScanRow>> cloud = filereader.ReadMultipleCloudOpton();
                foreach (List<ScanRow> rows in cloud)
                {
                    if (MaxRow < rows.Count) MaxRow = rows.Count;
                    foreach (ScanRow row in rows)
                    {
                        int f = 256;
                        foreach (Vertex vertex in row.Vertices)
                        {
                            if (MaxCol < row.Vertices.Count) MaxCol = row.Vertices.Count;
                            mPositions.Append((float)vertex.X);
                            mPositions.Append((float)vertex.Y);
                            mPositions.Append((float)vertex.Z);
                            pts.Add(vertex);
                            mColors.Append(pColor.R * f / 65535f);
                            mColors.Append(pColor.G * f / 65535f);
                            mColors.Append(pColor.B * f / 65535f);
                            f--;
                            if (f == 127) f = 256;
                        }
                    }
                }
                #endregion
            }
            else
            {
                #region 无序点云
                Point3D[] points = filereader.ReadCloud(filereader.VertSkip);
                ColorLookupTable mColorTable = new ColorLookupTable();
                mColorTable.SetMinValue((float)filereader.Min.Z);
                mColorTable.SetMaxValue((float)filereader.Max.Z);
                mColorTable.SetColorMap(ColorMapKeyword.Create(EnumSystemColorMap.Rainbow));
                foreach (Point3D pt in points)
                {
                    mPositions.Append((float)pt.X);
                    mPositions.Append((float)pt.Y);
                    mPositions.Append((float)pt.Z);
                    var color = mColorTable.GetColor((float)pt.Z);
                    mColors.Append(color.x);
                    mColors.Append(color.y);
                    mColors.Append(color.z);
                    pts.Add(pt);
                }
                #endregion
            }
            return true;
        }
        private bool ReadASC()
        {
            var points = filereader.ReadASC(filereader.VertSkip);
            ColorLookupTable mColorTable = new ColorLookupTable();
            mColorTable.SetMinValue((float)filereader.Min.Z);
            mColorTable.SetMaxValue((float)filereader.Max.Z);
            mColorTable.SetColorMap(ColorMapKeyword.Create(EnumSystemColorMap.Rainbow));
            foreach (V3 pt in points)
            {
                mPositions.Append((float)pt.X);
                mPositions.Append((float)pt.Y);
                mPositions.Append((float)pt.Z);
                var color = mColorTable.GetColor((float)pt.Z);
                mColors.Append(color.x);
                mColors.Append(color.y);
                mColors.Append(color.z);
                pts.Add(pt);
            }
            return true;
        }

        private bool ReadPCD()
        {
            List<V3> verts = filereader.ReadPCD(filereader.VertSkip);
            ColorLookupTable mColorTable = new ColorLookupTable();
            mColorTable.SetMinValue((float)filereader.Min.Z);
            mColorTable.SetMaxValue((float)filereader.Max.Z);
            mColorTable.SetColorMap(ColorMapKeyword.Create(EnumSystemColorMap.Rainbow));
            foreach (V3 pt in verts)
            {
                mPositions.Append((float)pt.X);
                mPositions.Append((float)pt.Y);
                mPositions.Append((float)pt.Z);
                var color = mColorTable.GetColor((float)pt.Z);
                mColors.Append(color.x);
                mColors.Append(color.y);
                mColors.Append(color.z);
                pts.Add(pt);
            }
            return true;
        }
        private bool ReadPLY() {
            List<V3> verts = filereader.ReadPLY(filereader.VertSkip);
            ColorLookupTable mColorTable = new ColorLookupTable();
            mColorTable.SetMinValue((float)filereader.Min.Z);
            mColorTable.SetMaxValue((float)filereader.Max.Z);
            mColorTable.SetColorMap(ColorMapKeyword.Create(EnumSystemColorMap.Rainbow));
            foreach (V3 pt in verts)
            {
                mPositions.Append((float)pt.X);
                mPositions.Append((float)pt.Y);
                mPositions.Append((float)pt.Z);
                var color = mColorTable.GetColor((float)pt.Z);
                mColors.Append(color.x);
                mColors.Append(color.y);
                mColors.Append(color.z);
                pts.Add(pt);
            }
            return true;
        }
        public void Run(RenderControl render)
        {
            if (!ReadData())
                return;
            var prveNode = render.Scene.FindNodeByUserId(CloudID);
            if(prveNode != null)
            {
                render.Scene.RemoveNode(prveNode);
            }
            PointCloud node = PointCloud.Create(mPositions, mColors, null, Size);
            node.SetUserId(CloudID);
            render.ShowSceneNode(node);
        }

        public void DrawBoundingBox(RenderControl render, float FontSize)
        {
            if (filereader == null)
            {
                throw new Exception("需要先指定点云来源");
            }
            var boxNode = DrawBoundBox(filereader, FontSize);
            boxNode.SetUserId(BoxID);
            render.ShowSceneNode(boxNode);
        }

        private GroupSceneNode DrawBoundBox(MVUnity.Exchange.CloudReader cloud, float FontSize)
        {
            GroupSceneNode plotModel = new GroupSceneNode();
            Vector3 Floor = ConvertV3.ToVector3(cloud.Min);
            Vector3 Ceiling = ConvertV3.ToVector3(cloud.Max);
            float minX = (float)Math.Ceiling(Floor.x / IntervalX) * IntervalX;
            float minY = (float)Math.Ceiling(Floor.y / IntervalY) * IntervalY;
            float minZ = (float)Math.Ceiling(Floor.z / IntervalZ) * IntervalZ;
            var material = MeshStandardMaterial.Create("my-material");
            material.SetRoughness(0.75f);
            material.SetMetalness(0.1f);
            material.SetColor(Vector3.Zero);
            material.SetFaceSide(EnumFaceSide.DoubleSide);

            for (float x = minX; x <= Ceiling.x; x += IntervalX)
            {
                BufferGeometry geometry = FontManager.Instance().CreateMesh(x.ToString("F0"));
                TextSceneNode label = new TextSceneNode(geometry, material, FontSize, true);
                label.SetWorldTransform(Matrix4.makeTranslation(x, Floor.y - (FontSize * 2.5f), Floor.z) * Matrix4.makeRotationAxis(Vector3.UNIT_Z, (float)(-0.5f * Math.PI)) *
                    Matrix4.makeScale(FontSize * 0.01f, FontSize * 0.01f, FontSize * 0.01f));
                label.SetPickable(false);
                plotModel.AddNode(label);
            }

            {
                BufferGeometry geometry = FontManager.Instance().CreateMesh("X (mm)");
                TextSceneNode label = new TextSceneNode(geometry, material, FontSize, true);
                label.SetPickable(false);
                label.SetWorldTransform(Matrix4.makeTranslation((Floor.x + Ceiling.x) * 0.5f, Floor.y - FontSize * 6, Floor.z)
                    * Matrix4.makeRotationAxis(Vector3.UNIT_Z, (float)(-0.5f * Math.PI)) * Matrix4.makeScale(FontSize * 0.01f, FontSize * 0.01f, FontSize * 0.01f));
                plotModel.AddNode(label);
            }

            for (float y = minY; y <= Ceiling.y; y += IntervalY)
            {
                BufferGeometry geometry = FontManager.Instance().CreateMesh(y.ToString("F0"));
                TextSceneNode label = new TextSceneNode(geometry, material, FontSize, true);
                label.SetPickable(false);
                label.SetWorldTransform(Matrix4.makeTranslation(Floor.x + FontSize * 3, y, Floor.z)
                    * Matrix4.makeScale(FontSize * 0.01f, FontSize * 0.01f, FontSize * 0.01f));
                plotModel.AddNode(label);
            }
            {
                BufferGeometry geometry = FontManager.Instance().CreateMesh("Y (mm)");
                TextSceneNode label = new TextSceneNode(geometry, material, FontSize, true);
                label.SetPickable(false);
                label.SetWorldTransform(Matrix4.makeTranslation(Floor.x + FontSize * 6, (Floor.y + Ceiling.y) * 0.5f, Floor.z)
                    * Matrix4.makeScale(FontSize * 0.01f, FontSize * 0.01f, FontSize * 0.01f));
                plotModel.AddNode(label);
            }

            for (float z = minZ; z <= Ceiling.z + double.Epsilon; z += IntervalZ)
            {
                BufferGeometry geometry = FontManager.Instance().CreateMesh(z.ToString("F0"));
                TextSceneNode label = new TextSceneNode(geometry, material, FontSize, true);
                label.SetPickable(false);
                label.SetWorldTransform(Matrix4.makeTranslation(Floor.x + FontSize * 3, Ceiling.y, z)
                    * Matrix4.makeRotationAxis(Vector3.UNIT_X, (float)(0.5f * Math.PI)) * Matrix4.makeScale(FontSize * 0.01f, FontSize * 0.01f, FontSize * 0.01f));
                plotModel.AddNode(label);
            }
            {
                BufferGeometry geometry = FontManager.Instance().CreateMesh("Z (mm)");
                TextSceneNode label = new TextSceneNode(geometry, material, FontSize, true);
                label.SetPickable(false);
                label.SetWorldTransform(Matrix4.makeTranslation(Floor.x + FontSize * 6, Ceiling.y, (Floor.z + Ceiling.z) * 0.5f)
                    * Matrix4.makeRotationAxis(Vector3.UNIT_X, (float)(0.5f * Math.PI)) * Matrix4.makeScale(FontSize * 0.01f, FontSize * 0.01f, FontSize * 0.01f));
                plotModel.AddNode(label);
            }
            {
                //Z

                TopoShape line = SketchBuilder.MakeLine(new GPnt(Floor.x, Floor.y, Floor.z), new GPnt(Floor.x, Floor.y, Ceiling.z));
                BrepSceneNode lineNode = BrepSceneNode.Create(line, material, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);

                line = SketchBuilder.MakeLine(new GPnt(Ceiling.x, Floor.y, Floor.z), new GPnt(Ceiling.x, Floor.y, Ceiling.z));
                lineNode = BrepSceneNode.Create(line, material, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);

                line = SketchBuilder.MakeLine(new GPnt(Ceiling.x, Ceiling.y, Floor.z), new GPnt(Ceiling.x, Ceiling.y, Ceiling.z));
                lineNode = BrepSceneNode.Create(line, material, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);

                line = SketchBuilder.MakeLine(new GPnt(Floor.x, Ceiling.y, Floor.z), new GPnt(Floor.x, Ceiling.y, Ceiling.z));
                lineNode = BrepSceneNode.Create(line, material, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);
            }
            {
                //Y
                TopoShape line = SketchBuilder.MakeLine(new GPnt(Floor.x, Floor.y, Floor.z), new GPnt(Floor.x, Ceiling.y, Floor.z));
                BrepSceneNode lineNode = BrepSceneNode.Create(line, material, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);

                line = SketchBuilder.MakeLine(new GPnt(Ceiling.x, Floor.y, Floor.z), new GPnt(Ceiling.x, Ceiling.y, Floor.z));
                lineNode = BrepSceneNode.Create(line, material, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);

                line = SketchBuilder.MakeLine(new GPnt(Ceiling.x, Floor.y, Ceiling.z), new GPnt(Ceiling.x, Ceiling.y, Ceiling.z));
                lineNode = BrepSceneNode.Create(line, material, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);

                line = SketchBuilder.MakeLine(new GPnt(Floor.x, Floor.y, Ceiling.z), new GPnt(Floor.x, Ceiling.y, Ceiling.z));
                lineNode = BrepSceneNode.Create(line, material, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);
            }
            {
                //X
                TopoShape line = SketchBuilder.MakeLine(new GPnt(Floor.x, Floor.y, Floor.z), new GPnt(Ceiling.x, Floor.y, Floor.z));
                BrepSceneNode lineNode = BrepSceneNode.Create(line, material, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);

                line = SketchBuilder.MakeLine(new GPnt(Floor.x, Ceiling.y, Floor.z), new GPnt(Ceiling.x, Ceiling.y, Floor.z));
                lineNode = BrepSceneNode.Create(line, material, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);

                line = SketchBuilder.MakeLine(new GPnt(Floor.x, Floor.y, Ceiling.z), new GPnt(Ceiling.x, Floor.y, Ceiling.z));
                lineNode = BrepSceneNode.Create(line, material, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);

                line = SketchBuilder.MakeLine(new GPnt(Floor.x, Ceiling.y, Ceiling.z), new GPnt(Ceiling.x, Ceiling.y, Ceiling.z));
                lineNode = BrepSceneNode.Create(line, material, null);
                lineNode.SetPickable(false);
                plotModel.AddNode(lineNode);
            }
            return plotModel;
        }

        private List<int> RemovePanel(List<V3> pts, MVUnity.Exchange.CloudReader reader, int DirResolution, int NrmResolution, int DetectorSize)
        {
            CubicMap cubic = new CubicMap(DirResolution * 4, NrmResolution * 4, DirResolution * 4, reader.Min, reader.Max);
            int id = 0;
            foreach (var pt in pts)
            {
                cubic.Mark(pt, id++);
            }
            int detectorLength = (int)Math.Pow(2, DetectorSize + 1);
            CubicRLMtx voxels = cubic.GetResults(2);
            List<int> filtratedID = new List<int>();
            foreach (var layerPair in voxels.GetAllLayers())
            {
                RLMatrix layer = layerPair.Value;
                #region 检测平板
                List<KeyValuePair<int, RLColumn>> list = layer.GetRLColumns();
                RLMatrix input = layer;
                RLMatrix output = layer;
                for (int i = 0; i < DetectorSize; i++)
                {
                    output = downSample(input);
                    input = output;
                }
                List<KeyValuePair<int, RLColumn>> Buff = new List<KeyValuePair<int, RLColumn>>();
                foreach (var col in output.GetRLColumns())
                {
                    var detected = col.Value.GetRunLengths().FindAll(c => c.Length > detectorLength).ToList();
                    if (detected.Count != 0)
                    {
                        Buff.Add(col);
                    }
                }
                #endregion
                #region 过滤平板
                if (Buff.Count != 0)
                {
                    List<MVUnity.Region> regions = layer.LabelRegions8(detectorLength * detectorLength);
                    for (int i = 0; i < regions.Count; i++)
                    {
                        bool flag = false;
                        MVUnity.Region region = regions[i];
                        foreach (var pair in Buff)
                        {
                            int CID = pair.Value.ColumnID;
                            foreach (var rl in pair.Value.GetRunLengths())
                            {
                                if (region.Cover(CID, rl.Start))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag) break;
                        }
                        if (flag)
                        {
                            layer = RLMatrix.Subtract(layer, region);
                        }
                    }
                }
                #endregion
                #region 结果
                foreach (KeyValuePair<int, RLColumn> colpair in layer.GetRLColumns())
                {
                    int c = colpair.Key;
                    foreach (var rl in colpair.Value.GetRunLengths())
                    {
                        for (int r = rl.Start; r < rl.End + 1; r++)
                        {
                            filtratedID.AddRange(cubic.CellIDList(layerPair.Key, c, r));
                        }
                    }
                }
                #endregion
            }
            return filtratedID;
        }
        private RLMatrix downSample(RLMatrix mtx)
        {
            List<KeyValuePair<int, RLColumn>> list = mtx.GetRLColumns();
            List<KeyValuePair<int, RLColumn>> result = new List<KeyValuePair<int, RLColumn>>();
            for (int i = 0; i < list.Count / 2; i++)
            {
                int k0 = list[i * 2].Key;
                int k1 = list[i * 2 + 1].Key;
                if (k1 - k0 == 1)
                {
                    RLColumn col = list[i * 2].Value & list[i * 2 + 1].Value;
                    if (col.EntriesCount != 0)
                    {
                        result.Add(new KeyValuePair<int, RLColumn>(k0 / 2, col));
                    }
                }
            }
            RLMatrix AND2 = new RLMatrix(result, mtx.ColumnCount / 2, mtx.RowCount);
            return AND2;
        }

        public List<V3> GetPoints() { return pts; }
    }
}
