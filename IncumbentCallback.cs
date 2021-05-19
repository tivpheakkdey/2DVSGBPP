using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILOG.Concert;
using ILOG.CPLEX;
using _2DWVSBPP_with_Visualizer.Problem;
using _2DWVSBPP_with_Visualizer.DFF;
using static ILOG.CPLEX.Cplex.Callback;

namespace _2DWVSBPP_with_Visualizer
{
    class IncumbentCallback : Cplex.Callback.Function
    {
        private double obj;

        public IncumbentCallback(double obj)
        {
            this.obj = obj;
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

                //if it's candidate is better that current best obj value
                if (context.GetCandidateObjective() < obj)
                {
                    //TODO: Check Feasibility
                    Console.WriteLine("callback working");
                }
            }
            catch (ILOG.Concert.Exception exc)
            {
                System.Console.WriteLine("Concert exception " + exc + " caught");
            }
        }
    }
}
