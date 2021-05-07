using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using _2DWVSBPP_with_Visualizer.Problem;

namespace _2DWVSBPP_with_Visualizer.DFF
{
    public class DFFRow : IComparable<DFFRow>
    {
        public List<double> row { get; private set; } = new List<double>();
        public int funcNo { get; private set; }
        public double p { get; private set; }
        public double q { get; private set; }
        public bool deleted { get; private set; } = false;

        private double _profit = -1;
        public double profitness
        {
            get
            {
                if (_profit < 0)
                {
                    _profit = 0;
                    foreach (double element in row) _profit += element;
                    return _profit;
                }
                else return _profit;
            }
            private set
            {
                _profit = value;
            }
        }

        public DFFRow(int funcNo, double p, double q)
        {
            this.funcNo = funcNo;
            this.p = p;
            this.q = q;
        }

        public int CompareTo(DFFRow other)
        {
            for (int i = 0; i < row.Count; i++) if (this.row[i] > other.row[i]) return 1;
            return -1;
        }

        public void Label()
        {
            deleted = true;
        }

        public override string ToString()
        {
            return $"{funcNo}[{p},{q}]({_profit})";
        }
    }
}
