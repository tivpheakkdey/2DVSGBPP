using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DWVSBPP_with_Visualizer.Problem
{
    public class Bin : IComparable<Bin>
    {
        public BinType type { get; private set; }
        public List<Item> items { get; private set; } = new List<Item>();
        public List<Tuple<double, double>> xy { get; private set; } = new List<Tuple<double, double>>();

        public double ratio
        {
            get { return (items.Sum(x => x.area) / type.cost); }
        }

        public Bin(BinType type)
        {
            this.type = type;
        }

        public void AddItem(Item item, double x, double y)
        {
            items.Add(item);
            xy.Add(new Tuple<double, double>(x, y));
        }

        override public string ToString()
        {
            double sum = items.Sum(x => x.area);
            string s = items.Count + " : " + (sum / type.cost).ToString("F") + $" [{type.cost}]:";
            for (int k = 0; k < items.Count; k++) s = s + "\n" + items[k].ToString() + " " + xy[k].Item1 + " " + xy[k].Item2;
            return s + "\n";
        }

        public int CompareTo(Bin other)
        {
            if (other.ratio > this.ratio) return 1;
            else return -1;
        }
    }
}
 