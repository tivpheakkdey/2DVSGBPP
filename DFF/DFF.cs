using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _2DWVSBPP_with_Visualizer.Problem;

namespace _2DWVSBPP_with_Visualizer.DFF
{
    public class FeasibilityConstraint
    {
        private delegate double function(double p, double height, double width);

        private static DFFRowDescendingOrder comparer = new DFFRowDescendingOrder();
        public static Stopwatch stopWatch = new Stopwatch();
        public static double u(double k, double x)
        {
            if (k < 0) throw new ArgumentException("k must be a natural number");
            if (((x * (k + 1)) % 1) == 0) return x;
            else return Math.Floor((k + 1) * x) / k;
        }

        public static double U(double e, double x)
        {
            if (e < 0 || e > 0.5) throw new ArgumentException("e has to be between 0-0.5 inclusive");
            else if (x > (1 - e)) return 1;
            else if (x >= e && x <= (1 - e)) return x;
            else return 0;
        }

        public static double phi(double e, double x)
        {
            if (e < 0 || e >= 0.5) throw new ArgumentException("e has to be between [0-0.5)");
            else if (x > 0.5) return 1 - Math.Floor((1 - x) * Math.Pow(e, -1)) / Math.Floor(Math.Pow(e, -1));
            else if (x >= e && x <= 0.5) return 1 / Math.Floor(Math.Pow(e, -1));
            else return 0;
        }

        private static double w1(double p, double height, double width)
        {
            double result = u(1, height) * U(p, width);
            return result;
        }

        private static double w2(double p, double height, double width)
        {
            double result = U(p, height) * u(1, width);
            return result;
        }

        private static double w3(double p, double height, double width)
        {
            double result = u(1, height) * phi(p, width);
            return result;
        }

        private static double w4(double p, double height, double width)
        {
            double result = phi(p, height) * u(1, width);
            return result;
        }

        private static double w5(double p, double height, double width)
        {
            double result = height * U(p, width);
            return result;
        }

        private static double w6(double p, double height, double width)
        {
            double result = U(p, height) * width;
            return result;
        }

        private static double w7(double p, double height, double width, double q)
        {
            double result = phi(p, height) * phi(q, width);
            return result;
        }

        private static List<Item> Scale(List<Item> items, Rectangle region)
        {
            List<Item> scaledItems = new List<Item>();
            foreach (Item item in items)
            {
                scaledItems.Add(new Item(0, item.height / region.height, item.width / region.width));
            }
            return scaledItems;
        }

        private static List<DFFRow> Filter(List<DFFRow> list)
        {
            List<DFFRow> result = new List<DFFRow>();

            for (int i = 0; i < list.Count; i++) if (list[i].profitness <= 1) list[i].Label();

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].deleted) continue;
                for (int j = 0; j < list.Count; j++)
                {
                    if (i == j) continue;
                    if (list[i].CompareTo(list[j]) < 0)
                    {
                        list[i].Label();
                        break;
                    }
                }
            }

            for (int i = 0; i < list.Count; i++) if (!list[i].deleted) result.Add(list[i]);

            //Sort result in descending order
            result.Sort(comparer);
            return result;
        }

        public static List<DFFRow> Generate(List<Item> items, Rectangle region, double start, double end, double step)
        {
            //stopWatch.Start();

            List<DFFRow> result = new List<DFFRow>();
            List<Item> scaledItems = Scale(items, region);
            List<function> functions = new List<function>();
            functions.Add(new function(w1));
            functions.Add(new function(w2));
            functions.Add(new function(w3));
            functions.Add(new function(w4));
            functions.Add(new function(w5));
            functions.Add(new function(w6));

            int funcCounter = 1;
            //Creating the matrix from the first 6 functions
            foreach (function func in functions)
            {
                for (double p = start; p <= end; p += step)
                {
                    DFFRow stepResult = new DFFRow(funcCounter, p, -1);
                    foreach (Item item in scaledItems) stepResult.row.Add(func(p, item.height, item.width));
                    result.Add(stepResult);
                }
                funcCounter++;
            }

            for (double p = start; p <= end; p += step)
            {
                for (double q = start; q <= end; q += step)
                {
                    DFFRow stepResult = new DFFRow(7, 1, q);
                    foreach (Item item in scaledItems) stepResult.row.Add(w7(p, item.height, item.width, q));
                    result.Add(stepResult);
                }
            }

            result = Filter(result);
            //stopWatch.Stop();
            return result;
        }

        public class DFFRowDescendingOrder : IComparer<DFFRow>
        {
            public int Compare(DFFRow x, DFFRow y)
            {
                return Math.Sign(y.profitness - x.profitness);
            }
        }

    }
}
