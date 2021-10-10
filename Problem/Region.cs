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
        public List<Item> items { get; private set; } = new List<Item>();
        public List<Tuple<double, double>> xy { get; private set; } = new List<Tuple<double, double>>();

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

        public void AddItem(Item item, double x, double y)
        {
            items.Add(item);
            xy.Add(new Tuple<double, double>(x, y));
        }

        public override string ToString()
        {
            double sum = items.Sum(x => x.area);
            string s = $"[{area}][{height},{width}][{x},{y}][{used}]";
            s = s + "\n" + items.Count + " : " + (sum / area).ToString("F") + $" [{area}]:";
            for (int k = 0; k < items.Count; k++) s = s + "\n" + items[k].ToString() + " " + xy[k].Item1 + " " + xy[k].Item2;
            return s + "\n";
        }

        public void Use()
        {
            used = true;
        }

        public static int CompareRegionByArea(Region a, Region b)
        {
            if (a == null)
            {
                if (b == null) return 0;
                else return 1;
            }
            else
            {
                int result = b.area.CompareTo(a.area);
                if (result != 0) return result;
                else return b.area.CompareTo(a.area);
            }
        }

    }
}
