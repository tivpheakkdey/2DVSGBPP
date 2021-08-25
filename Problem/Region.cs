using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DWVSBPP_with_Visualizer.Problem
{
    public class Region : Rectangle
    {
        public double x { get; private set; }
        public double y { get; private set; }
        public double area { get; private set; }
        public bool used { get; set; } = false;
        public Bin bin { get; private set; }

        public Region(Bin bin, double height, double width, double x, double y) : base (height, width)
        {
            this.bin = bin;
            this.x = x;
            this.y = y;
            area = height * width;
        }

        public Region(Bin bin, BinType type) : this(bin, type.height, type.width, 0, 0)
        {

        }

        public override string ToString()
        {
            return $"[{area}][{height},{width}][{x},{y}][{used}]";
        }

        public void Use()
        {
            used = true;
        }

    }
}
