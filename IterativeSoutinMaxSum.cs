using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _2DWVSBPP_with_Visualizer.Problem;
using _2DWVSBPP_with_Visualizer.DFF;
using ILOG.Concert;
using ILOG.CPLEX;

namespace _2DWVSBPP_with_Visualizer
{
    public class IterativeSolutionMaxSum : IterativeSolution
    {
        private Instance inst;
        private Cplex cplex_bp;
        public List<Bin> bins { get; private set; }

        public IterativeSolutionMaxSum(Instance instance)
        {
            inst = instance;
            try
            {
                cplex_bp = new Cplex();
                cplex_bp.SetParam(Cplex.IntParam.MIPDisplay, 0);
                cplex_bp.SetParam(Cplex.Param.TimeLimit, 2);
                cplex_bp.SetParam(Cplex.DoubleParam.EpGap, 0.05);
                cplex_bp.SetParam(Cplex.Param.Emphasis.MIP, 3);
                cplex_bp.SetOut(System.IO.TextWriter.Null);
                cplex_bp.SetWarning(System.IO.TextWriter.Null);
            }
            catch (ILOG.Concert.Exception exc)
            {
                System.Console.WriteLine("Concert exception " + exc + " caught");
            }
        }

        private void SplitRegion(Region region, Item item, List<Region> unique_regions_temp, List<Region> paired_regions_temp)
        {
            double area;
            Region r_0 = ((area = region.height * (region.width - item.width)) == 0) ? null : new Region(region.bin, region.height, region.width - item.width, region.x, region.y + item.width);
            Region r_1 = ((area = item.width * (region.height - item.height)) == 0) ? null : new Region(region.bin, region.height - item.height, item.width, region.x + item.height, region.y);
            Region r_2 = ((area = item.height * (region.width - item.width)) == 0) ? null : new Region(region.bin, item.height, region.width - item.width, region.x, region.y + item.width);
            Region r_3 = ((area = region.width * (region.height - item.height)) == 0) ? null : new Region(region.bin, region.height - item.height, region.width, region.x + item.height, region.y);

            if (r_0 == null)
            {
                if (r_1 == null) return;
                unique_regions_temp.Add(r_1);
                return;
            }
            if (r_1 == null)
            {
                unique_regions_temp.Add(r_0);
                return;
            }
            paired_regions_temp.Add(r_0);
            paired_regions_temp.Add(r_1);
            paired_regions_temp.Add(r_2);
            paired_regions_temp.Add(r_3);
        }

        private bool RegionRedundancyCheck(List<Item> sorted_items, Region region)
        {
            for (int i = 0; i < sorted_items.Count; i++)
            {
                // commented if items are not sorted in area accending order
                //if (region.area < sorted_items[i].area) return false;
                if (sorted_items[i].CanFit(region)) return true;
            }
            return false;
        }

        public double Solve(List<Item> _items, double UB, int[] encoding, out double LB, out List<Item> unpacked)
        {
            LB = -1; unpacked = null;
            List<Item> items = new List<Item>(_items); items.Sort();
            List<Region> regions = new List<Region>(); int s = 0, b = 0;
            double total = 0;
            if (encoding == null) for (int k = 0; k < inst.m; k++) regions.Add(new Region(new Bin(inst.types[k]), inst.types[k]));
            else
            {
                for (int k = 0; k < encoding.Length; k++)
                {
                    total += inst.types[k].cost * encoding[k];
                    for (int j = 0; j < encoding[k]; j++) regions.Add(new Region(new Bin(inst.types[k]), inst.types[k]));
                }
                b = regions.Count;
                for (int k = 0; k < inst.m; k++) regions.Add(new Region(new Bin(inst.types[k]), inst.types[k]));
            }
            int n = items.Count;
            int r = regions.Count;
            bins = new List<Bin>(n);

            //int iter = 0;
            try
            {
                while (true)
                {
                    cplex_bp.ClearModel();
                    List<Item>[] item_options = new List<Item>[r]; for (int k = 0; k < r; k++) item_options[k] = new List<Item>(n);

                    // variables to assign items (i) to regions (k)
                    IIntVar[][] x = new IIntVar[n][];
                    IIntVar[][] q = new IIntVar[n][];
                    IIntVar[] u = new IIntVar[n];

                    ILinearNumExpr objective = cplex_bp.LinearNumExpr(); double negative_cost = 0;

                    for (int i = 0; i < n; i++)
                    {
                        items[i].tag = i;
                        x[i] = new IIntVar[r];
                        q[i] = new IIntVar[r];
                        u[i] = cplex_bp.BoolVar();
                        ILinearIntExpr single_region_only = cplex_bp.LinearIntExpr();
                        single_region_only.AddTerm(1, u[i]);
                        for (int k = 0; k < r; k++)
                        {
                            if (regions[k] == null) continue;
                            if (items[i].CanFit(regions[k]))
                            {
                                item_options[k].Add(items[i]);
                                x[i][k] = cplex_bp.BoolVar();
                                q[i][k] = cplex_bp.BoolVar();
                                single_region_only.AddTerm(1, x[i][k]);
                                single_region_only.AddTerm(1, q[i][k]);
                            }
                        }
                        cplex_bp.AddEq(single_region_only, 1);
                    }

                    IIntVar[] y = new IIntVar[r - b];
                    ILinearNumExpr cost_limit_open_bins = cplex_bp.LinearNumExpr();
                    for (int k = 0; k < r - b; k++)
                    {
                        y[k] = cplex_bp.IntVar(0, item_options[b + k].Count);
                        cost_limit_open_bins.AddTerm(inst.types[k].cost, y[k]);
                    }
                    cplex_bp.AddLe(cost_limit_open_bins, Math.Max(0, UB - total - 1));

                    for (int k = 0; k < r; k++)
                    {
                        if (regions[k] == null) continue;
                        ILinearIntExpr expr = cplex_bp.LinearIntExpr();
                        for (int i = 0; i < item_options[k].Count; i++) if (x[item_options[k][i].tag][k] != null) expr.AddTerm(1, x[item_options[k][i].tag][k]);
                        cplex_bp.AddLe(expr, 1);
                    }

                    ILinearIntExpr expr1 = cplex_bp.LinearIntExpr();
                    for (int k = b; k < r; k++)
                    {
                        if (regions[k] == null) continue;
                        for (int i = 0; i < item_options[k].Count; i++)
                            if (x[item_options[k][i].tag][k] != null)
                                expr1.AddTerm(1, x[item_options[k][i].tag][k]);
                    }
                    cplex_bp.AddLe(expr1, 1);

                    // introduce DFF constraints
                    for (int k = 0; k < r; k++)
                    {
                        if (regions[k] == null) continue;
                        List<DFFRow> constraints = FeasibilityConstraint.Generate(item_options[k], regions[k], 0.01, 0.49, 0.05);
                        for (int f = 0; f < Math.Min(constraints.Count, 10); f++)
                        {
                            ILinearNumExpr single_item_only = cplex_bp.LinearNumExpr();
                            for (int i = 0; i < item_options[k].Count; i++)
                            {
                                int ptr = item_options[k][i].tag;
                                if (constraints[f].row[i] == 0) continue;
                                single_item_only.AddTerm(constraints[f].row[i], x[ptr][k]);
                                single_item_only.AddTerm(constraints[f].row[i], q[ptr][k]);
                            }
                            if (k < b) cplex_bp.AddLe(single_item_only, 1);
                            else
                            {
                                single_item_only.AddTerm(-1, y[k - b]);
                                cplex_bp.AddLe(single_item_only, 0);
                            }
                        }

                        ILinearNumExpr area_constraint = cplex_bp.LinearNumExpr();
                        for (int i = 0; i < item_options[k].Count; i++)
                        {
                            int ptr = item_options[k][i].tag;
                            area_constraint.AddTerm(items[ptr].area, x[ptr][k]);
                            area_constraint.AddTerm(items[ptr].area, q[ptr][k]);

                            double cost = items[ptr].price / regions[k].area;
                            negative_cost += cost;
                            objective.AddTerm(cost, x[ptr][k]);
                        }
                        if (k < b) cplex_bp.AddLe(area_constraint, regions[k].area);
                        else
                        {
                            area_constraint.AddTerm(-regions[k].area, y[k - b]);
                            cplex_bp.AddLe(area_constraint, 0);
                        }
                    }

                    for (int i = 0; i < n; i++) objective.AddTerm(Math.Min(-negative_cost, -1), u[i]);

                    // cut type (z) for region (k) is in use
                    IIntVar[] z = new IIntVar[s];
                    for (int k = 0; 4 * k < s; k++)
                    {
                        z[k] = cplex_bp.BoolVar();
                        int count_a = 0;
                        int count_b = 0;
                        ILinearIntExpr cutA_in_use = cplex_bp.LinearIntExpr();
                        ILinearIntExpr cutB_in_use = cplex_bp.LinearIntExpr();
                        for (int i = 0; i < n; i++)
                        {
                            int ptr = 4 * k;
                            if (x[i][ptr] != null) { cutA_in_use.AddTerm(1, x[i][ptr]); cutA_in_use.AddTerm(1, q[i][ptr]); count_a++; }
                            if (x[i][ptr + 1] != null) { cutA_in_use.AddTerm(1, x[i][ptr + 1]); cutA_in_use.AddTerm(1, q[i][ptr + 1]); count_a++; }
                            if (x[i][ptr + 2] != null) { cutB_in_use.AddTerm(1, x[i][ptr + 2]); cutB_in_use.AddTerm(1, q[i][ptr + 2]); count_b++; }
                            if (x[i][ptr + 3] != null) { cutB_in_use.AddTerm(1, x[i][ptr + 3]); cutB_in_use.AddTerm(1, q[i][ptr + 3]); count_b++; }
                        }
                        cutA_in_use.AddTerm(-count_a, z[k]);
                        cutB_in_use.AddTerm(count_b, z[k]);
                        cplex_bp.AddLe(cutA_in_use, 0);
                        cplex_bp.AddLe(cutB_in_use, count_b);
                    }

                    cplex_bp.AddMaximize(objective);

                    // execute the MIP model
                    if (cplex_bp.Solve())
                    {
                        //Console.WriteLine("cost:\t" + cplex_bp.GetObjValue());

                        List<Item> items_temp = new List<Item>(items.Count);
                        List<Region> unique_regions_temp = new List<Region>(regions.Count);
                        List<Region> paired_regions_temp = new List<Region>(regions.Count);

                        int[] item_mask = new int[n];
                        bool[] paired_regions_mask = new bool[s / 4];

                        for (int k = 0; k < b; k++)
                        {
                            for (int i = item_options[k].Count - 1; i >= 0; i--)
                            {
                                int ptr = item_options[k][i].tag;
                                if (item_mask[ptr] == 2) continue;
                                if ((int)Math.Round(cplex_bp.GetValue(x[ptr][k])) != 1) continue;
                                regions[k].Use();
                                SplitRegion(regions[k], items[ptr], unique_regions_temp, paired_regions_temp);
                                if (k < s) paired_regions_mask[k / 4] = true;
                                item_mask[ptr] = 2;
                                regions[k].bin.AddItem(items[ptr], regions[k].x, regions[k].y);
                                break;
                            }
                        }

                        for (int k = b; k < r; k++)
                        {
                            bool f = false;
                            for (int i = item_options[k].Count - 1; i >= 0; i--)
                            {
                                int ptr = item_options[k][i].tag;
                                if (item_mask[ptr] == 2) continue;
                                if ((int)Math.Round(cplex_bp.GetValue(x[ptr][k])) != 1) continue;
                                regions[k].Use();
                                SplitRegion(regions[k], items[ptr], unique_regions_temp, paired_regions_temp);
                                if (b <= k)
                                {
                                    total += inst.types[k - b].cost;
                                    bins.Add(regions[k].bin);
                                }
                                item_mask[ptr] = 2;
                                regions[k].bin.AddItem(items[ptr], regions[k].x, regions[k].y);
                                f = true;
                                break;
                            }
                            if (f) break;
                        }

                        if (cplex_bp.GetObjValue() < 0)
                        {
                            unpacked = new List<Item>(items.Count);
                            for (int i = 0; i < n; i++) if ((int)Math.Round(cplex_bp.GetValue(u[i])) == 1) unpacked.Add(items[i]);
                            if (unpacked.Count > 0)
                            {
                                //Console.WriteLine("Unpacked List:\t" + String.Join(" ", unpacked) + " | total: " +K +" of " + unpacked.Count);
                                //Console.WriteLine(total + "|" + UB);
                                LB = total;
                                return double.MaxValue;
                            }
                        }

                        //for (int k = 0; k < r; k++)
                        //{
                        //    if (regions[k] == null) continue;
                        //    double cost = 0;
                        //    for (int i = item_options[k].Count - 1; i >= 0; i--)
                        //    {
                        //        int ptr = item_options[k][i].tag;
                        //        if ((int)Math.Round(cplex_bp.GetValue(x[ptr][k])) != 1 && (int)Math.Round(cplex_bp.GetValue(q[ptr][k])) != 1) continue;
                        //        cost += items[ptr].area;
                        //        //Console.WriteLine($"item {items[ptr].ToString()}: {regions[k].ToString()}");
                        //    }
                        //    //Console.WriteLine($"===>{iter}\t {regions[k].ToString()}: {(cost / regions[k].area).ToString("F")} " + (b > k ? "simple" : "bin"));
                        //}
                        //iter++;

                        for (int i = 0; i < n; i++) if (item_mask[i] != 2) items_temp.Add(items[i]);
                        int c = unique_regions_temp.Count;

                        for (int k = s; k < b; k++)
                        {
                            if (regions[k].used) continue;
                            if (!RegionRedundancyCheck(items_temp, regions[k])) continue;
                            unique_regions_temp.Add(regions[k]);
                        }

                        for (int k = 0; 4 * k < s; k++)
                        {
                            int ptr = 4 * k;
                            if (paired_regions_mask[k])
                            {
                                int start = ((int)Math.Round(cplex_bp.GetValue(z[k])) == 1) ? 0 : 2;
                                for (int j = start; j < start + 2; j++)
                                {
                                    if (regions[ptr + j] == null) continue;
                                    if (regions[ptr + j].used) continue;
                                    if (!RegionRedundancyCheck(items_temp, regions[ptr + j])) continue;
                                    unique_regions_temp.Add(regions[ptr + j]);
                                }
                            }
                            else
                            {
                                for (int j = 0; j < 4; j++) paired_regions_temp.Add(regions[ptr + j]);
                            }
                        }

                        regions = new List<Region>(unique_regions_temp.Count + 4 * paired_regions_temp.Count + 1);
                        for (int k = 0; 4 * k < paired_regions_temp.Count; k++)
                        {
                            int ptr = 4 * k;
                            int counter = 0;
                            Region[] temp = new Region[4];

                            bool region_0 = false;
                            bool region_3 = false;

                            if (paired_regions_temp[ptr + 2] != null)
                            {
                                if (RegionRedundancyCheck(items_temp, paired_regions_temp[ptr + 2]))
                                {
                                    temp[2] = paired_regions_temp[ptr + 2];
                                    temp[0] = paired_regions_temp[ptr];
                                    counter += 2;
                                    region_0 = true;
                                }
                            }

                            if (paired_regions_temp[ptr] != null && !region_0)
                            {
                                if (RegionRedundancyCheck(items_temp, paired_regions_temp[ptr]))
                                {
                                    temp[0] = paired_regions_temp[ptr];
                                    counter += 1;
                                }
                            }

                            if (paired_regions_temp[ptr + 1] != null)
                            {
                                if (RegionRedundancyCheck(items_temp, paired_regions_temp[ptr + 1]))
                                {
                                    temp[1] = paired_regions_temp[ptr + 1];
                                    temp[3] = paired_regions_temp[ptr + 3];
                                    counter += 2;
                                    region_3 = true;
                                }
                            }

                            if (paired_regions_temp[ptr] != null && !region_3)
                            {
                                if (RegionRedundancyCheck(items_temp, paired_regions_temp[ptr + 3]))
                                {
                                    temp[3] = paired_regions_temp[ptr + 3];
                                    counter += 1;
                                }
                            }

                            if (counter == 0) continue;
                            if (counter == 1)
                            {
                                for (int j = 0; j < 4; j++)
                                {
                                    if (temp[j] != null)
                                    {
                                        unique_regions_temp.Add(temp[j]);
                                        break;
                                    }
                                }
                                continue;
                            }
                            if (counter == 2)
                            {
                                if (temp[0] == null && temp[2] == null) { unique_regions_temp.Add(temp[3]); continue; }
                                if (temp[1] == null && temp[3] == null) { unique_regions_temp.Add(temp[0]); continue; }
                            }
                            regions.AddRange(temp);
                        }

                        s = regions.Count;

                        for (int k = 0; k < unique_regions_temp.Count; k++)
                        {
                            if (k < c && !RegionRedundancyCheck(items_temp, unique_regions_temp[k])) continue;
                            regions.Add(unique_regions_temp[k]);
                        }

                        items = items_temp;
                        n = items.Count;
                        if (n == 0) break;
                        b = regions.Count;
                        if (total != UB) for (int k = 0; k < inst.m; k++) regions.Add(new Region(new Bin(inst.types[k]), inst.types[k]));
                        r = regions.Count;
                    }
                    else
                    {
                        Console.Error.WriteLine("INFEASIBLE");
                    }

                }
                return total;
            }
            catch (ILOG.Concert.Exception exc)
            {
                System.Console.WriteLine("Concert exception '" + exc + "' caught");
                LB = total;
                return double.MaxValue;
            }
        }

    }
}
