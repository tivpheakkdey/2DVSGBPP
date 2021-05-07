using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DWVSBPP_with_Visualizer.Problem
{
    public class BinType : Rectangle
    {
        public double cost { get; private set; }

        public BinType(double height, double width) : base (height, width){}

        public void SetCost(double cost)
        {
            this.cost = cost;
        }

        public override string ToString()
        {
            return $"[{cost}][{height},{width}]";
        }

    }
}
