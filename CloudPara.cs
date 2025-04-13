﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Windows.Media;
using MVUnity;
using MVUnity.Geometry3D;

namespace MViewer
{
    /// <summary>
    /// 着色方式
    /// </summary>
    public enum ColorMode { 
        /// <summary>
        /// 单色
        /// </summary>
        Mono,
        /// <summary>
        /// 云图
        /// </summary>
    Contour,
    /// <summary>
    /// 点云纹理
    /// </summary>
    Texture
    }
    public class CloudPara : INotifyPropertyChanged
    {
        private V3 cloudscale;
        private string cloudformat;
        private string[] cloudFilePath;
        private Color pointColor;
        private int pointSize;
        
        public V3 Cloudscale { get => cloudscale; set => cloudscale = value; }
        public string Cloudformat { get => cloudformat; set => cloudformat = value; }
        public string[] CloudFilePath { get => cloudFilePath; set => cloudFilePath = value; }
        public int PointSize { get => pointSize; set => pointSize = value; }
        public int VertSkip { get; set; }
        public SolidColorBrush PointBrush
        {
            get
            {
                return new SolidColorBrush(pointColor);
            }
        }
        public Color PointColor
        {
            get
            {
                return pointColor;
            }
            set
            {
                pointColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PointColor"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PointBrush"));
            }
        }
        /// <summary>
        /// 点云着色方式
        /// </summary>
        public ColorMode ColorMode { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool UseROI { get; set; }
        public V3 UL { get; set; }

        public V3 LL { get; set; }

        public CloudPara(string[] filename)
        {
            CloudFilePath = filename;
            Cloudformat = ConfigurationManager.AppSettings["CloudFormat"];
            Cloudscale = new V3(ConfigurationManager.AppSettings["CloudScale"], ',');
            ColorMode = (ColorMode)System.Enum.Parse(typeof(ColorMode),ConfigurationManager.AppSettings["ColorMode"]);
            LL = new V3(ConfigurationManager.AppSettings["LL"], ',');
            PointSize = int.Parse(ConfigurationManager.AppSettings["PointSize"]);
            string burshString = ConfigurationManager.AppSettings["PointBrush"];
            PointColor = (Color)ColorConverter.ConvertFromString(burshString);
            UL = new V3(ConfigurationManager.AppSettings["UL"], ',');
            UseROI = bool.Parse(ConfigurationManager.AppSettings["UseROI"]);
            VertSkip = int.Parse(ConfigurationManager.AppSettings["VertSkip"]);
        }
        public CloudPara(string filename)
        {
            CloudFilePath = new string[] { filename };
            Cloudformat = ConfigurationManager.AppSettings["CloudFormat"];
            Cloudscale = new V3(ConfigurationManager.AppSettings["CloudScale"], ',');
            ColorMode = (ColorMode)System.Enum.Parse(typeof(ColorMode), ConfigurationManager.AppSettings["ColorMode"]);
            LL = new V3(ConfigurationManager.AppSettings["LL"], ',');
            PointSize = int.Parse(ConfigurationManager.AppSettings["PointSize"]);
            string burshString = ConfigurationManager.AppSettings["PointBrush"];
            PointColor = (Color)ColorConverter.ConvertFromString(burshString);
            UL = new V3(ConfigurationManager.AppSettings["UL"], ',');
            UseROI = bool.Parse(ConfigurationManager.AppSettings["UseROI"]);
            VertSkip = int.Parse(ConfigurationManager.AppSettings["VertSkip"]);
        }
    }

    public class SelectionPara
    {
        /// <summary>
        /// 检查法向
        /// </summary>
        public bool NormCheck { get; set; }
        /// <summary>
        /// 是否由指定法向
        /// </summary>
        public bool UseUserNorm { get; set; }
        /// <summary>
        /// 用户指定法向
        /// </summary>
        public V3 UserNormal { get; set;}
        /// <summary>
        /// 法向点乘容差
        /// </summary>
        public double NormDotTol { get; set; }
        public bool RadiusCheck { get; set; }
        public double AlphaRadius { get; set; }
        public bool NLCheck { get; set; }
        public double NLowLimit { get; set; }
        public bool NUCheck { get; set; }
        public double NUpLimit { get; set; }
        public SelectionPara()
        {
            NormCheck = bool.Parse(ConfigurationManager.AppSettings["NormCheck"]);
            UserNormal = new V3(ConfigurationManager.AppSettings["UserNormal"], ',');
            UseUserNorm = bool.Parse(ConfigurationManager.AppSettings["UseUserNorm"]);
            NormDotTol = double.Parse(ConfigurationManager.AppSettings["NormDotTol"]);
            RadiusCheck = bool.Parse(ConfigurationManager.AppSettings["RadiusCheck"]);
            AlphaRadius = double.Parse(ConfigurationManager.AppSettings["AlphaRadius"]);
            NLCheck = bool.Parse(ConfigurationManager.AppSettings["NLCheck"]);
            NLowLimit = double.Parse(ConfigurationManager.AppSettings["NLowLimit"]);
            NUCheck = bool.Parse(ConfigurationManager.AppSettings["NUCheck"]);
            NUpLimit = double.Parse(ConfigurationManager.AppSettings["NUpLimit"]);
        }
    }
}
