using System.ComponentModel;
using System.Configuration;
using System.Windows.Media;
using MVUnity;

namespace MViewer
{
    public class CloudPara : INotifyPropertyChanged
    {
        private V3 cloudscale;
        private string cloudformat;
        private string cloudFilePath;
        private Color pointColor;
        private int pointSize;

        public V3 Cloudscale { get => cloudscale; set => cloudscale = value; }
        public string Cloudformat { get => cloudformat; set => cloudformat = value; }
        public string CloudFilePath { get => cloudFilePath; set => cloudFilePath = value; }
        public int PointSize { get => pointSize; set => pointSize = value; }
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

        public event PropertyChangedEventHandler PropertyChanged;

        public CloudPara(string filename)
        {
            CloudFilePath = filename;
            Cloudformat = ConfigurationManager.AppSettings["CloudFormat"];
            Cloudscale = new V3(ConfigurationManager.AppSettings["CloudScale"], ',');
            PointSize = int.Parse(ConfigurationManager.AppSettings["PointSize"]);
            string burshString = ConfigurationManager.AppSettings["PointBrush"];
            PointColor = (Color)ColorConverter.ConvertFromString(burshString);
        }
    }
}
