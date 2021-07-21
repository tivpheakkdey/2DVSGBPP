using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _2DWVSBPP_with_Visualizer.Problem;

namespace _2DWVSBPP_with_Visualizer
{
    static class FeasibilityCheck
    {
        /*Place item in height order from tallest to shortest to encourage verticle cuts
         *Priortize stacking items on top before placing on the right side.
         */
        static public bool Packing(List<Item> assignment, Bin bin)
        {
            for (int i = 0; i < assignment.Count; i++) Console.WriteLine(assignment[i].ToString());
            assignment.Sort(Item.CompareItemByHeight);
            for (int i = 0; i < assignment.Count; i++) Console.WriteLine(assignment[i].ToString());
            //Check if any item is too big for the bin
            //for(int i = 0; i < assignment.Count; i++)
            //{
            //    if (!assignment[i].CanFit(bin.type)) return false;
            //}




            return true;
        }
    }
}
