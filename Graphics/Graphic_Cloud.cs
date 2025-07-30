using AnyCAD.Foundation;
using AnyCAD.WPF;
using MVUnity;
using MVUnity.Geometry3D;
using MVUnity.PointCloud;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MViewer.Graphics
{
    internal class Graphic_Cloud
    {
        const int CloudID = 1;
        const ulong OBBID = 6;
        static Float32Buffer mPositions;
        static Float32Buffer mColors;
        MVUnity.Exchange.CloudReader filereader;
        public System.Windows.Media.Color PColor;
        public int Size;
        public int IntervalX;
        public int IntervalY;
        public int IntervalZ;
        public ColorMode ColorMode;
        public bool UseROI;
        public ABB ROI;
        public Graphic_Cloud(MVUnity.Exchange.CloudReader cloud)
        {
            filereader = cloud;
            PColor = System.Windows.Media.Colors.Red;
            Size = 2;
            IntervalX = 100;
            IntervalY = 100;
            IntervalZ = 100;
            ColorMode = ColorMode.Mono;
            UseROI = false;
        }
        public Graphic_Cloud()
        {
            PColor = System.Windows.Media.Colors.Red;
            Size = 2;
        }
        bool ReadData()
        {
            mPositions = new Float32Buffer(0);
            mColors = new Float32Buffer(0);
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
            bool structed = false;
            if (filereader.Format.Contains('r') & filereader.Format.Contains('c')) { structed = true; }
            ColorLookupTable mColorTable = new ColorLookupTable();
            mColorTable.SetColorMap(ColorMapKeyword.Create(EnumSystemColorMap.Rainbow));
            switch (ColorMode)
            {
                default:
                case ColorMode.Mono:
                    if (UseROI)
                    {
                        if (structed)
                        {
                            #region 有序点云
                            List<List<ScanRow>> cloud = filereader.ReadMultipleCloudOpton();
                            foreach (List<ScanRow> rows in cloud)
                            {
                                foreach (ScanRow row in rows)
                                {
                                    int f = 256;
                                    foreach (Vertex vertex in row.Vertices)
                                    {
                                        if (!ROI.Cover(vertex)) continue;
                                        mPositions.Append((float)vertex.X);
                                        mPositions.Append((float)vertex.Y);
                                        mPositions.Append((float)vertex.Z);
                                        mColors.Append(PColor.R * f / 65535f);
                                        mColors.Append(PColor.G * f / 65535f);
                                        mColors.Append(PColor.B * f / 65535f);
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
                            foreach (Point3D pt in points)
                            {
                                if (!ROI.Cover(pt)) continue;
                                mPositions.Append((float)pt.X);
                                mPositions.Append((float)pt.Y);
                                mPositions.Append((float)pt.Z);
                                mColors.Append(PColor.R / 255f);
                                mColors.Append(PColor.G / 255f);
                                mColors.Append(PColor.B / 255f);
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        if (structed)
                        {
                            #region 有序点云
                            List<List<ScanRow>> cloud = filereader.ReadMultipleCloudOpton();
                            foreach (List<ScanRow> rows in cloud)
                            {
                                foreach (ScanRow row in rows)
                                {
                                    int f = 256;
                                    foreach (Vertex vertex in row.Vertices)
                                    {
                                        mPositions.Append((float)vertex.X);
                                        mPositions.Append((float)vertex.Y);
                                        mPositions.Append((float)vertex.Z);
                                        mColors.Append(PColor.R * f / 65535f);
                                        mColors.Append(PColor.G * f / 65535f);
                                        mColors.Append(PColor.B * f / 65535f);
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
                            foreach (Point3D pt in points)
                            {
                                mPositions.Append((float)pt.X);
                                mPositions.Append((float)pt.Y);
                                mPositions.Append((float)pt.Z);
                                mColors.Append(PColor.R/255f);
                                mColors.Append(PColor.G/255f);
                                mColors.Append(PColor.B / 255f);
                            }
                            #endregion
                        }
                    }
                    break;
                case ColorMode.X:
                    {
                        Point3D[] points = filereader.ReadCloud(filereader.VertSkip);
                        if (UseROI)
                        {
                            mColorTable.SetMinValue((float)ROI.LowerLimit.X);
                            mColorTable.SetMaxValue((float)ROI.UpperLimit.X);
                            foreach (Point3D pt in points)
                            {
                                if (!ROI.Cover(pt)) continue;
                                mPositions.Append((float)pt.X);
                                mPositions.Append((float)pt.Y);
                                mPositions.Append((float)pt.Z);
                                var color = mColorTable.GetColor((float)pt.X);
                                mColors.Append(color.x);
                                mColors.Append(color.y);
                                mColors.Append(color.z);
                            }
                        }
                        else
                        {
                            mColorTable.SetMinValue((float)filereader.Min.X);
                            mColorTable.SetMaxValue((float)filereader.Max.X);
                            foreach (Point3D pt in points)
                            {
                                mPositions.Append((float)pt.X);
                                mPositions.Append((float)pt.Y);
                                mPositions.Append((float)pt.Z);
                                var color = mColorTable.GetColor((float)pt.X);
                                mColors.Append(color.x);
                                mColors.Append(color.y);
                                mColors.Append(color.z);
                            }
                        }
                    }
                    break;
                case ColorMode.Y:
                    {
                        Point3D[] points = filereader.ReadCloud(filereader.VertSkip);
                        if (UseROI)
                        {
                            
                            mColorTable.SetMinValue((float)ROI.LowerLimit.Y);
                            mColorTable.SetMaxValue((float)ROI.UpperLimit.Y);
                            foreach (Point3D pt in points)
                            {
                                if (!ROI.Cover(pt)) continue;
                                mPositions.Append((float)pt.X);
                                mPositions.Append((float)pt.Y);
                                mPositions.Append((float)pt.Z);
                                var color = mColorTable.GetColor((float)pt.Y);
                                mColors.Append(color.x);
                                mColors.Append(color.y);
                                mColors.Append(color.z);
                            }
                        }
                        else
                        {
                            mColorTable.SetMinValue((float)filereader.Min.Y);
                            mColorTable.SetMaxValue((float)filereader.Max.Y);
                            foreach (Point3D pt in points)
                            {
                                mPositions.Append((float)pt.X);
                                mPositions.Append((float)pt.Y);
                                mPositions.Append((float)pt.Z);
                                var color = mColorTable.GetColor((float)pt.Y);
                                mColors.Append(color.x);
                                mColors.Append(color.y);
                                mColors.Append(color.z);
                            }
                        }
                    }
                    break;
                case ColorMode.Z:
                    {
                        Point3D[] points = filereader.ReadCloud(filereader.VertSkip);
                        if (UseROI)
                        {

                            mColorTable.SetMinValue((float)ROI.LowerLimit.Z);
                            mColorTable.SetMaxValue((float)ROI.UpperLimit.Z);
                            foreach (Point3D pt in points)
                            {
                                if (!ROI.Cover(pt)) continue;
                                mPositions.Append((float)pt.X);
                                mPositions.Append((float)pt.Y);
                                mPositions.Append((float)pt.Z);
                                var color = mColorTable.GetColor((float)pt.Z);
                                mColors.Append(color.x);
                                mColors.Append(color.y);
                                mColors.Append(color.z);
                            }
                        }
                        else
                        {
                            mColorTable.SetMinValue((float)filereader.Min.Z);
                            mColorTable.SetMaxValue((float)filereader.Max.Z);
                            foreach (Point3D pt in points)
                            {
                                mPositions.Append((float)pt.X);
                                mPositions.Append((float)pt.Y);
                                mPositions.Append((float)pt.Z);
                                var color = mColorTable.GetColor((float)pt.Z);
                                mColors.Append(color.x);
                                mColors.Append(color.y);
                                mColors.Append(color.z);
                            }
                        }
                    }
                    break;
                case ColorMode.Texture:
                    if (UseROI)
                    {
                        Point3D[] points = filereader.ReadCloud(filereader.VertSkip);
                        for (int i = 0; i < points.Length; i++)
                        {
                            Point3D pt = points[i];
                            if (!ROI.Cover(pt)) continue;
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            mColors.Append((float)filereader.RGBs[i].X);
                            mColors.Append((float)filereader.RGBs[i].Y);
                            mColors.Append((float)filereader.RGBs[i].Z);
                        }
                    }
                    else
                    {
                        Point3D[] points = filereader.ReadCloud(filereader.VertSkip);
                        for (int i = 0; i < points.Length; i++)
                        {
                            Point3D pt = points[i];
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            mColors.Append((float)filereader.RGBs[i].X);
                            mColors.Append((float)filereader.RGBs[i].Y);
                            mColors.Append((float)filereader.RGBs[i].Z);
                        }
                    }
                    break;
            }
            return true;
        }
        private bool ReadASC()
        {
            var points = filereader.ReadASC(filereader.VertSkip);
            ColorLookupTable mColorTable = new ColorLookupTable();
            mColorTable.SetColorMap(ColorMapKeyword.Create(EnumSystemColorMap.Rainbow));
            if (UseROI)
            {
                mColorTable.SetMinValue((float)ROI.LowerLimit.Z);
                mColorTable.SetMaxValue((float)ROI.UpperLimit.Z);
                foreach (V3 pt in points)
                {
                    if (!ROI.Cover(pt)) continue;
                    mPositions.Append((float)pt.X);
                    mPositions.Append((float)pt.Y);
                    mPositions.Append((float)pt.Z);
                    var color = mColorTable.GetColor((float)pt.Z);
                    mColors.Append(color.x);
                    mColors.Append(color.y);
                    mColors.Append(color.z);
                }
            }
            else
            {
                mColorTable.SetMinValue((float)filereader.Min.Z);
                mColorTable.SetMaxValue((float)filereader.Max.Z);
                foreach (V3 pt in points)
                {
                    mPositions.Append((float)pt.X);
                    mPositions.Append((float)pt.Y);
                    mPositions.Append((float)pt.Z);
                    var color = mColorTable.GetColor((float)pt.Z);
                    mColors.Append(color.x);
                    mColors.Append(color.y);
                    mColors.Append(color.z);
                }
            }
            return true;
        }

        private bool ReadPCD()
        {
            List<V3> verts = filereader.ReadPCD(filereader.VertSkip);
            ColorLookupTable mColorTable = new ColorLookupTable();
            mColorTable.SetColorMap(ColorMapKeyword.Create(EnumSystemColorMap.Rainbow));
            if (UseROI)
            {
                switch (ColorMode)
                {
                    case ColorMode.X:
                        mColorTable.SetMinValue((float)ROI.LowerLimit.X);
                        mColorTable.SetMaxValue((float)ROI.UpperLimit.X);
                        foreach (V3 pt in verts)
                        {
                            if (!ROI.Cover(pt)) continue;
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            var color = mColorTable.GetColor((float)pt.X);
                            mColors.Append(color.x);
                            mColors.Append(color.y);
                            mColors.Append(color.z);
                        }
                        break;
                    case ColorMode.Y:
                        mColorTable.SetMinValue((float)ROI.LowerLimit.Y);
                        mColorTable.SetMaxValue((float)ROI.UpperLimit.Y);
                        foreach (V3 pt in verts)
                        {
                            if (!ROI.Cover(pt)) continue;
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            var color = mColorTable.GetColor((float)pt.Y);
                            mColors.Append(color.x);
                            mColors.Append(color.y);
                            mColors.Append(color.z);
                        }
                        break;
                    case ColorMode.Z:
                        mColorTable.SetMinValue((float)ROI.LowerLimit.Z);
                        mColorTable.SetMaxValue((float)ROI.UpperLimit.Z);
                        foreach (V3 pt in verts)
                        {
                            if (!ROI.Cover(pt)) continue;
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            var color = mColorTable.GetColor((float)pt.Z);
                            mColors.Append(color.x);
                            mColors.Append(color.y);
                            mColors.Append(color.z);
                        }
                        break;
                    case ColorMode.Texture:
                        for (int i = 0; i < verts.Count; i++)
                        {
                            V3 pt = verts[i];
                            if (!ROI.Cover(pt)) continue;
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            var color = filereader.RGBs[i];
                            mColors.Append((float)color.X / 256f);
                            mColors.Append((float)color.Y / 256f);
                            mColors.Append((float)color.Z / 256f);
                        }
                        break;
                    case ColorMode.Mono:
                    default:
                        foreach (V3 pt in verts)
                        {
                            if (!ROI.Cover(pt)) continue;
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            mColors.Append(PColor.R / 255f);
                            mColors.Append(PColor.G / 255f);
                            mColors.Append(PColor.B / 255f);
                        }
                        break;
                }
            }
            else
            {
                switch (ColorMode)
                {
                    case ColorMode.X:
                        mColorTable.SetMinValue((float)filereader.Min.X);
                        mColorTable.SetMaxValue((float)filereader.Max.X);
                        foreach (V3 pt in verts)
                        {
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            var color = mColorTable.GetColor((float)pt.X);
                            mColors.Append(color.x);
                            mColors.Append(color.y);
                            mColors.Append(color.z);
                        }
                        break;
                    case ColorMode.Y:
                        mColorTable.SetMinValue((float)filereader.Min.Y);
                        mColorTable.SetMaxValue((float)filereader.Max.Y);
                        foreach (V3 pt in verts)
                        {
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            var color = mColorTable.GetColor((float)pt.Y);
                            mColors.Append(color.x);
                            mColors.Append(color.y);
                            mColors.Append(color.z);
                        }
                        break;
                    case ColorMode.Z:
                        mColorTable.SetMinValue((float)filereader.Min.Z);
                        mColorTable.SetMaxValue((float)filereader.Max.Z);
                        foreach (V3 pt in verts)
                        {
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            var color = mColorTable.GetColor((float)pt.Z);
                            mColors.Append(color.x);
                            mColors.Append(color.y);
                            mColors.Append(color.z);
                        }
                        break;
                    case ColorMode.Texture:
                        for (int i = 0; i < verts.Count; i++)
                        {
                            V3 pt = verts[i];
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            var color = filereader.RGBs[i];
                            mColors.Append((float)color.X);
                            mColors.Append((float)color.Y);
                            mColors.Append((float)color.Z);
                        }
                        break;
                    case ColorMode.Mono:
                    default:
                        foreach (V3 pt in verts)
                        {
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            mColors.Append(PColor.R / 255f);
                            mColors.Append(PColor.G / 255f);
                            mColors.Append(PColor.B / 255f);
                        }
                        break;
                }
            }
            return true;
        }
        private bool ReadPLY()
        {
            List<V3> verts = filereader.ReadPLY(filereader.VertSkip);
            ColorLookupTable mColorTable = new ColorLookupTable();
            mColorTable.SetColorMap(ColorMapKeyword.Create(EnumSystemColorMap.Rainbow));
            if (UseROI)
            {
                switch (ColorMode)
                {
                    case ColorMode.X:
                        mColorTable.SetMinValue((float)ROI.LowerLimit.X);
                        mColorTable.SetMaxValue((float)ROI.UpperLimit.X);
                        foreach (V3 pt in verts)
                        {
                            if (!ROI.Cover(pt)) continue;
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            var color = mColorTable.GetColor((float)pt.X);
                            mColors.Append(color.x);
                            mColors.Append(color.y);
                            mColors.Append(color.z);
                        }
                        break;
                    case ColorMode.Y:
                        mColorTable.SetMinValue((float)ROI.LowerLimit.Y);
                        mColorTable.SetMaxValue((float)ROI.UpperLimit.Y);
                        foreach (V3 pt in verts)
                        {
                            if (!ROI.Cover(pt)) continue;
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            var color = mColorTable.GetColor((float)pt.Y);
                            mColors.Append(color.x);
                            mColors.Append(color.y);
                            mColors.Append(color.z);
                        }
                        break;
                    case ColorMode.Z:
                        mColorTable.SetMinValue((float)ROI.LowerLimit.Z);
                        mColorTable.SetMaxValue((float)ROI.UpperLimit.Z);
                        foreach (V3 pt in verts)
                        {
                            if (!ROI.Cover(pt)) continue;
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            var color = mColorTable.GetColor((float)pt.Z);
                            mColors.Append(color.x);
                            mColors.Append(color.y);
                            mColors.Append(color.z);
                        }
                        break;
                    case ColorMode.Texture:
                        for (int i = 0; i < verts.Count; i++)
                        {
                            V3 pt = verts[i];
                            if (!ROI.Cover(pt)) continue;
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            var color = filereader.RGBs[i];
                            mColors.Append((float)color.X / 256f);
                            mColors.Append((float)color.Y / 256f);
                            mColors.Append((float)color.Z / 256f);
                        }
                        break;
                    case ColorMode.Mono:
                    default:
                        foreach (V3 pt in verts)
                        {
                            if (!ROI.Cover(pt)) continue;
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            mColors.Append(PColor.R / 255f);
                            mColors.Append(PColor.G / 255f);
                            mColors.Append(PColor.B / 255f);
                        }
                        break;
                }
            }
            else
            {
                switch (ColorMode)
                {
                    case ColorMode.X:
                        mColorTable.SetMinValue((float)filereader.Min.X);
                        mColorTable.SetMaxValue((float)filereader.Max.X);
                        foreach (V3 pt in verts)
                        {
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            var color = mColorTable.GetColor((float)pt.X);
                            mColors.Append(color.x);
                            mColors.Append(color.y);
                            mColors.Append(color.z);
                        }
                        break;
                    case ColorMode.Y:
                        mColorTable.SetMinValue((float)filereader.Min.Y);
                        mColorTable.SetMaxValue((float)filereader.Max.Y);
                        foreach (V3 pt in verts)
                        {
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            var color = mColorTable.GetColor((float)pt.Y);
                            mColors.Append(color.x);
                            mColors.Append(color.y);
                            mColors.Append(color.z);
                        }
                        break;
                    case ColorMode.Z:
                        mColorTable.SetMinValue((float)filereader.Min.Z);
                        mColorTable.SetMaxValue((float)filereader.Max.Z);
                        foreach (V3 pt in verts)
                        {
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            var color = mColorTable.GetColor((float)pt.Z);
                            mColors.Append(color.x);
                            mColors.Append(color.y);
                            mColors.Append(color.z);
                        }
                        break;
                    case ColorMode.Texture:
                        for (int i = 0; i < verts.Count; i++)
                        {
                            V3 pt = verts[i];
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            var color = filereader.RGBs[i];
                            mColors.Append((float)color.X);
                            mColors.Append((float)color.Y);
                            mColors.Append((float)color.Z);
                        }
                        break;
                    case ColorMode.Mono:
                    default:
                        foreach (V3 pt in verts)
                        {
                            mPositions.Append((float)pt.X);
                            mPositions.Append((float)pt.Y);
                            mPositions.Append((float)pt.Z);
                            mColors.Append(PColor.R / 255f);
                            mColors.Append(PColor.G / 255f);
                            mColors.Append(PColor.B / 255f);
                        }
                        break;
                }
            }
            return true;
        }
        public void ShowCloud(List<V3> verts, RenderControl render)
        {
            var prevNode = GroupSceneNode.Cast(render.Scene.FindNodeByUserId(CloudID));
            if (prevNode != null)
            {
                prevNode.Clear();
            }
            else
            {
                prevNode = new GroupSceneNode();
                prevNode.SetUserId(CloudID);
                render.Scene.AddNode(prevNode);
            }
            mPositions = new Float32Buffer(0);
            mColors = new Float32Buffer(0);
            foreach (V3 pt in verts)
            {
                mPositions.Append((float)pt.X);
                mPositions.Append((float)pt.Y);
                mPositions.Append((float)pt.Z);

                mColors.Append(PColor.R / 255f);
                mColors.Append(PColor.G / 255f);
                mColors.Append(PColor.B / 255f);
            }
            PointCloud node = PointCloud.Create(mPositions, mColors, null, Size);
            prevNode.AddNode(node);
            render.RequestDraw(EnumUpdateFlags.Scene);
        }
        public void Run(RenderControl render)
        {
            if (!ReadData())
                return;
            var prevNode = GroupSceneNode.Cast(render.Scene.FindNodeByUserId(CloudID));
            if (prevNode != null)
            {
                prevNode.Clear();
            }
            else
            {
                prevNode = new GroupSceneNode();
                prevNode.SetUserId(CloudID);
                render.Scene.AddNode(prevNode);
            }
            PointCloud node = PointCloud.Create(mPositions, mColors, null, Size);
            prevNode.AddNode(node);
        }

        public void Append(RenderControl render)
        {
            if (!ReadData())
                return;
            var prevNode = GroupSceneNode.Cast(render.Scene.FindNodeByUserId(CloudID));
            if (prevNode == null)
            {
                prevNode = new GroupSceneNode();
                prevNode.SetUserId(CloudID);
                render.Scene.AddNode(prevNode);
            }
            PointCloud node = PointCloud.Create(mPositions, mColors, null, Size);
            prevNode.AddNode(node);
        }

        public void DrawBoundingBox(RenderControl render, float FontSize)
        {
            if (filereader == null)
            {
                throw new Exception("需要先指定点云来源");
            }
            var boxNode = DrawBoundBox(filereader, FontSize);
            boxNode.SetUserId(OBBID);
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
    }
}
