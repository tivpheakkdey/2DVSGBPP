using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DWVSBPP_with_Visualizer.Problem
{
    public class Rectangle
    {
        public double height { get; protected set; }
        public double width { get; protected set; }

        public Rectangle (double height, double width)
        {
            this.height = height;
            this.width = width;
        }
    }
}
