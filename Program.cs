using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _2DWVSBPP_with_Visualizer.Problem;
using System.Diagnostics;
using ILOG.Concert;
using ILOG.CPLEX;
using _2DWVSBPP_with_Visualizer.DFF;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;


namespace _2DWVSBPP_with_Visualizer
{
    class Program
    {
        private static Instance instance;

        static private Random rnd = new Random(100);
        private const int ITER_LIMIT = 2500000;
        private static int MAX_ITER = 8;
        private static int MAX_ITER1 = 5;
        private static int MAX_ITER2 = 10;

        private static int time_limit = 4000;

        private static StringBuilder result = new StringBuilder();
        private static Stopwatch stopWatch = new Stopwatch();

        static private double total_area;
        private static IterativeSolution model1;

        private static List<List<Bin>> solutionList = new List<List<Bin>>();
        private static int solutionCounter = 0;

        private static bool allowed = false;

        private static double ratio = 0;

        //[STAThread]
        //static void Main()
        //{
        //    //Application.SetHighDpiMode(HighDpiMode.SystemAware);
        //    Application.EnableVisualStyles();
        //    Application.SetCompatibleTextRenderingDefault(false);
        //    Application.Run(new Visualizser());
        //}

        static void Main(string[] args)
        {             
            instance = new Instance(@"D:\Deakin\Honours\Trimester 1\SIT723 Research Project A\Code\Instances\2dvsbp_data\mult1.bpp");
            //instance = new Instance(args[0]); 
            //Console.WriteLine(instance.ToString());
            //time_limit = Int32.Parse(args[1]);
            List<DFFRow> constraints = FeasibilityConstraint.Generate(instance.items, new Region(null, instance.types[0]), 0.001, 0.499, 0.005);

            total_area = instance.items.Sum(x => x.area);

            //Create a model with the class CplexBP by passing it an instance
            double UB = double.MaxValue;
            model1 = new IterativeSolutionMaxSum(instance);

            result.Append((int)Math.Ceiling(constraints.Count != 0 ? constraints[0].profitness : 1)); result.Append(" ");
            result.Append(instance.problem_class); result.Append(" ");
            result.Append(instance.absolute_index); result.Append(" ");
            result.Append(instance.relative_index); result.Append(" ");
            result.Append(instance.n.ToString()); result.Append(" ");
            result.Append(total_area); result.Append(" ");
            result.Append((100 * total_area / UB).ToString("F")); result.Append(" ");
            int l = result.Length;

            for (int i = 0; i < instance.items.Count; i++)
            {
                instance.items[i].price = instance.items[i].area;
                instance.items[i].multiplier = 1;
                instance.items[i].scale = instance.items[i].area / instance.max_area;
            }

            stopWatch.Start();
            UB = SubMain(UB);
            result.Insert(l, (UB) + " " + (100 * total_area / UB).ToString("F") + " ");
            System.Console.WriteLine(result.ToString());
            System.Console.WriteLine(DFF.FeasibilityConstraint.stopWatch.Elapsed.TotalSeconds.ToString("F"));
        }

        static private double SubMain(double UB)
        {
            double bound = UB;
            for (int i = 0; i < ITER_LIMIT; i++)
            {
                stopWatch.Stop();
                if (stopWatch.Elapsed.TotalSeconds >= time_limit) return bound;
                stopWatch.Start();

                //Console.WriteLine($"\niteration {i}");
                List<Item> items = new List<Item>(instance.items);
                double obj = Forward(items, bound, 0, new List<Bin>(instance.n));

                if (bound > obj)
                {
                    bound = obj;
                    //System.Console.WriteLine(stopWatch.Elapsed.TotalSeconds.ToString("F"));
                    //Console.WriteLine($"\n*****************************************************************************************************");
                    //Console.WriteLine($"*****************************************************************************************************");
                    //Console.WriteLine($"*****************************************************************************************************");
                    //Console.WriteLine($"*****************************************************************************************************\n");
                    //Console.WriteLine($"\n------------------\nNew primal bound found:\n*** Total cost:\t{bound}\n*** Packing density:\t" + 100 * total_area / bound);
                }
                allowed = true;
                MAX_ITER = 11; MAX_ITER1 = 5; MAX_ITER2 = 9;
            }
            return bound;
        }

        public static bool CheckForPackingFeasibility(Bin bin)
        {
            for (int i = 0; i < bin.items.Count; i++)
            {
                double iw = bin.items[i].height + bin.xy[i].Item1;
                double ih = bin.items[i].width + bin.xy[i].Item2;
                for (int j = i + 1; j < bin.items.Count; j++)
                {
                    double jw = bin.items[j].height + bin.xy[j].Item1;
                    double jh = bin.items[j].width + bin.xy[j].Item2;
                    if ((!((iw <= bin.xy[j].Item1) || (jw <= bin.xy[i].Item1))) && (!((ih <= bin.xy[j].Item2) || (jh <= bin.xy[i].Item2)))) return false;
                }
            }
            return true;
        }

        static private double Forward(List<Item> items, double UB, double cost, List<Bin> solution)
        {
            //Console.WriteLine($"\nsize : \t{items.Count}");
            double bound = UB, limit = UB;
            List<Bin> bins = new List<Bin>(items.Count);
            bool triger = false;
            IterativeSolution model_iterative = model1;
            double LB = 0; List<Item> unpacked = new List<Item>(items.Count);
            int[] encoding = null;

            int model = 1;
            int bin_count = 0;
            for (int i = 1; i <= MAX_ITER; i++)
            {
                stopWatch.Stop();
                if (stopWatch.Elapsed.TotalSeconds >= time_limit) return bound;
                stopWatch.Start();

                //if (model == 1) Console.Write("1 ");
                //if (model == 2) Console.Write("2 ");
                //if (model == 3) Console.Write("3 ");

                double obj = model_iterative.Solve(items, bound, encoding, out LB, out unpacked);
                //if (encoding != null && obj < double.MaxValue) Console.WriteLine("Encoding List:\t" + String.Join(" ", encoding)+ "["+LB + "|" + limit + "|" + UB+"]");
                encoding = null;
                if (obj < double.MaxValue)
                {
                    bound = obj;
                    limit = obj;
                    stopWatch.Stop();
                    result.Append((cost + bound).ToString()); result.Append(" ");
                    result.Append((100 * total_area / (cost + bound)).ToString("F")); result.Append(" ");
                    result.Append(stopWatch.Elapsed.TotalSeconds.ToString("F")); result.Append(" ");
                    Console.WriteLine($"\n------------------\nNew primal bound found:\n*** Total cost:\t{cost + bound}\n*** Packing density:\t" + total_area / (cost + bound) + "\tmodel: " + model + "\tcall: " + i + "\ttime: " + stopWatch.Elapsed.TotalSeconds.ToString("F") + "\titems.Count: " + items.Count);
                    stopWatch.Start();
                    triger = true;
                    i--;
                    ratio = total_area / (cost + bound);

                    for (int k = 0; k < solution.Count; k++)
                    {
                        if (!CheckForPackingFeasibility(solution[k]))
                        {
                            result.Append("PROBLEM!!"); result.Append(" ");
                        }
                        Console.WriteLine(solution[k].ToString());
                    }


                    for (int k = 0; k < model_iterative.bins.Count; k++)
                    {
                        if (!CheckForPackingFeasibility(model_iterative.bins[k]))
                        {
                            result.Append("PROBLEM!!"); result.Append(" ");
                        }
                        Console.WriteLine(model_iterative.bins[k].ToString());
                    }

                    solutionList.Add(model_iterative.bins);
                }
                else
                {
                    for (int k = 0; k < unpacked.Count; k++)
                    {
                        Item item = unpacked[k];
                        item.multiplier = Math.Pow(item.scale + 1, 3) / item.ratio;
                        item.price = item.multiplier * item.area;
                    }
                    if (!triger && allowed)
                    {
                        limit = NextU.Find(instance.types, LB, limit, out encoding);
                        if (limit < 0)
                        {
                            encoding = null;
                            limit = bound;
                        }
                    }
                }

                bin_count = Math.Max(bin_count, model_iterative.bins.Count);
                for (int j = 0; j < model_iterative.bins.Count; j++)
                {
                    bins.Add(model_iterative.bins[j]);
                    bins.Sort();
                    if (bins[0].ratio > Math.Min(ratio * 1.1, 0.97) && allowed)
                    {
                        for (int k = 1; k < bins.Count; k++)
                        {
                            if (bins[k - 1].ratio == bins[k].ratio)
                            {
                                bins.RemoveAt(k);
                                k--;
                            }
                            if (k > 1)
                            {
                                bins.RemoveAt(k);
                                k--;
                            }
                        }
                    }
                    else for (int k = bins.Count - 1; k > 0; k--) bins.RemoveAt(k);

                    for (int k = 0; k < model_iterative.bins[j].items.Count; k++)
                    {
                        Item item = model_iterative.bins[j].items[k];
                        double r = rnd.NextDouble() * item.ratio;
                        item.multiplier = item.multiplier * r + (1 - r) * Math.Pow(item.scale + 1, 2) / model_iterative.bins[j].ratio;
                        item.price = item.multiplier * item.area;
                        item.UpdateRatio(model_iterative.bins[j].ratio);
                    }
                }

            }

            if (solutionList.Count != solutionCounter)
            {
                Application.Run(new Visualizser(solutionList, instance.types[0].width, instance.types[0].height));
                solutionCounter = solutionList.Count;
            }

            double objective = bound;
            if (bin_count <= 1) return objective;
            for (int t = 0; t < bins.Count; t++)
            {
                List<Item> _items = new List<Item>(items);
                for (int k = 0; k < bins[t].items.Count; k++) _items.Remove(bins[t].items[k]);
                if (_items.Count == 0) return objective;
                //double COST = 0;
                //for (int k = 0; k < _items.Count; k++) COST += _items[k].area;
                //if (UB < COST) return objective;
                List<Bin> _solution = new List<Bin>();
                _solution.AddRange(solution);
                _solution.Add(bins[t]);
                if (t == 0) objective = Math.Min(objective, bins[t].type.cost + Forward(_items, objective - bins[t].type.cost, cost + bins[t].type.cost, _solution));
                if (bins[0].ratio > Math.Min(ratio * 1.1, 0.97) && t > 0)
                    objective = Math.Min(objective, bins[t].type.cost + Forward(_items, objective - bins[t].type.cost, cost + bins[t].type.cost, _solution));
            }
            return objective;
        }

        public class NextU
        {
            private static Cplex cplex = new Cplex();

            public static double Find(List<BinType> types, double LB, double UB, out int[] result)
            {
                result = new int[types.Count];
                try
                {
                    cplex.ClearModel();
                    cplex.SetParam(Cplex.IntParam.MIPDisplay, 0);
                    cplex.SetOut(System.IO.TextWriter.Null);
                    ILinearNumExpr objective = cplex.LinearNumExpr();
                    IIntVar[] x = cplex.IntVarArray(types.Count, 0, Int32.MaxValue);
                    for (int i = 0; i < types.Count; i++) objective.AddTerm(types[i].cost, x[i]);
                    cplex.AddLe(objective, UB - 1);
                    cplex.AddGe(objective, LB);
                    cplex.AddMaximize(objective);
                    if (cplex.Solve())
                    {
                        for (int i = 0; i < types.Count; i++) result[i] = (int)Math.Round(cplex.GetValue(x[i]));
                        return cplex.GetObjValue();
                    }
                }
                catch (ILOG.Concert.Exception exc)
                {
                    System.Console.WriteLine("Concert exception " + exc + " caught");
                    return -1;
                }
                return -1;
            }
        }
    }
}
