using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _2DWVSBPP_with_Visualizer.Problem;

namespace _2DWVSBPP_with_Visualizer
{
    public interface IterativeSolution
    {
        double Solve(List<Item> _items, double UB, int[] encoding, out double LB, out List<Item> unpacked);
        List<Bin> bins { get; }
    }
}
  