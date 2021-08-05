﻿using System;
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

            //Create a model with the class CplexBP by passing it an instance
            //AssigmentSolution model = new AssigmentSolution(instance);
            //model.Solve();

            //List<Item> items = new List<Item>();

            //items.Add(new Item(1, 2, 2));
            //items.Add(new Item(2, 1, 2));
            //items.Add(new Item(3, 3, 2));
            //items.Add(new Item(4, 4, 2));

            //Bin bin = new Bin(new BinType(10, 10));

            //FeasibilityCheck.Packing(items, bin);

            //Console.ReadLine();

            List<Item> items = new List<Item>();
            int[] width = { 5, 10, 12, 15 };
            for(int i = 0; i < width.Length; i++)
            {
                items.Add(new Item(i,0, width[i]));
            }

            List<int> result = FeasibilityCheck.NormalPattern(items, 27, false);

            for(int i = 0; i < result.Count; i++)
            {
                Console.WriteLine(result[i]);
            }

            Console.ReadLine();


        }
    }
}
