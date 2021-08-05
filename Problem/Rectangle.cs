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

        public static int ComparebyArea(Rectangle a, Rectangle b)
        {
            if (a == null)
            {
                if (b == null) return 0;
                else return -1;
            }
            else
            {
                double aArea = a.height * a.width;
                double bArea = b.height * b.width;

                return aArea.CompareTo(bArea);
            }
        }
    }
}
