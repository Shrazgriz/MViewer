using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVUnity;

namespace MViewer.Graphics
{
    internal class G_ColorMap
    {
        public double MaxValue { get; set; }
        public double MinValue { get; set; }
        public V3[] Colors { get; set; }
        public double[] Anchors { get; set; }
        public G_ColorMap() { MaxValue = 1;MinValue = 0; }
    }
}
