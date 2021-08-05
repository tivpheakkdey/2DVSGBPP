using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DWVSBPP_with_Visualizer.Problem
{
    public class Item : Rectangle,IComparable<Item>
    {
        public int index { get; set; }
        public double area { get; private set; }
        public double multiplier { get; set; } = 1;
        public int tag { get; set; }
        public double price { get; set; }
        public double ratio { get; set; } = 1;
        public double scale { get; set; } = 1;

        private LinkedList<double> ratios { get; set; } = new LinkedList<double>();

        public Item(int index, double height, double width) : base (height,width)
        {
            this.index = index;
            area = height * width;
        }

        public bool CanFit(Region region)
        {
            if (height <= region.height && width <= region.width) return true;
            return false;
        }

        public bool CanFit(BinType type)
        {
            if (height <= type.height && width <= type.width) return true;
            return false;
        }


        public override string ToString()
        {
            return $"{index}[{height},{width}]({area},{multiplier.ToString("F")})";
        }

        public int CompareTo(Item other)
        {
            //return Math.Sign(this.area*this.multiplier - other.area * other.multiplier);
            return Math.Sign(this.price - other.price);
        }

        public void UpdateRatio(double value)
        {
            ratios.AddLast(value);
            if (ratios.Count > 15)
            {
                ratio = (ratio * 15 + value - ratios.First.Value) / 15;
                ratios.RemoveFirst();
            }
            else
            {
                ratio = (ratio * ratios.Count + value) / (ratios.Count + 1);
            }
        }

        public static int CompareItemByHeight(Item a, Item b)
        {
            if (a == null)
            {
                if (b == null) return 0;
                else return 1;
            }
            else
            {
                int result = b.height.CompareTo(a.height);
                if (result != 0) return result;
                else return b.width.CompareTo(a.width);
            }
        }

        public static int CompareItemByWidth(Item a, Item b)
        {
            if (a == null)
            {
                if (b == null) return 0;
                else return 1;
            }
            else
            {
                int result = b.width.CompareTo(a.width);
                if (result != 0) return result;
                else return b.height.CompareTo(a.height);
            }
        }
    }
}
