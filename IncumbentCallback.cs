using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILOG.Concert;
using ILOG.CPLEX;
using _2DWVSBPP_with_Visualizer.Problem;
using static ILOG.CPLEX.Cplex.Callback;

namespace _2DWVSBPP_with_Visualizer
{
    class IncumbentCallback : Cplex.Callback.Function
    {
        private double obj;
        private IIntVar[][][] x;
        private List<Item> items;
        private Instance inst;

        public IncumbentCallback(double obj, IIntVar[][][] x, Instance inst)
        {
            this.obj = obj;
            this.x = x;
            this.items = inst.items;
            this.inst = inst;
        }
        public void Invoke(Context context)
        {
            try
            {
                if (context.InCandidate) solveIncumbent(context);
            }
            catch (ILOG.Concert.Exception exc)
            {
                System.Console.WriteLine("Concert exception " + exc + " caught");
            }
        }

        private void solveIncumbent(Context context)
        {
            try
            {
                //obtain the context model
                Cplex model = context.GetCplex();

                //reconstructing the model
                for(int binType = 0; binType < inst.m; binType++)
                {
                    for(int bin = 0; bin < inst.n; bin++)
                    {
                        List<Item> assignment = new List<Item>();
                        for(int item = 0; item < inst.n; item++)
                        {
                            if ((int)Math.Round(context.GetCandidatePoint(x[item][bin][binType])) == 1) assignment.Add(items[item]);
                        }

                        //Checking the feasibility TODO
                        if (!FeasibilityCheck.MIPPacking(assignment, inst.types[binType]))
                        {
                            ILinearNumExpr cut = model.LinearNumExpr();
                            for (int i = 0; i < assignment.Count(); i++)
                            {
                                cut.AddTerm(1, x[assignment[i].index][bin][binType]);
                            }

                            context.RejectCandidate(model.Le(cut, assignment.Count() - 1));

                            //for (int j = 0; j < inst.n; j++)
                            //{
                            //    ILinearNumExpr cut = model.LinearNumExpr();

                            //    for (int i = 0; i < assignment.Count(); i++) cut.AddTerm(1, x[assignment[i].index][j][binType]);

                            //    context.RejectCandidate(model.Le(cut, assignment.Count() - 1));
                            //}
                        }


                    }
                }
                

            }
            catch (ILOG.Concert.Exception exc)
            {
                System.Console.WriteLine("Concert exception " + exc + " caught");
            }
        }
    }
}

/*TODO*/
/*
 * recontruct the context
 * For each bin and its solution
 */
