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
    class AssigmentSolution
    {
        private Instance inst;
        private Cplex cplex_bp;

        public List<Bin> bins { get; private set; }

        public AssigmentSolution(Instance instance)
        {
            inst = instance;
            try
            {
                //init model
                cplex_bp = new Cplex();

                //set CPlex parameters
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

        public void Solve()
        {
            try
            {
                //reset model for use
                cplex_bp.ClearModel();

                //adding a temp var to store constraints that need to be added
                ILinearIntExpr constraint = cplex_bp.LinearIntExpr();

                //variables to set the number bin type K is used
                IIntVar[] z = new IIntVar[inst.m];

                //decision variables to assign items (i) to bin types (k)
                IIntVar[][] x = new IIntVar[inst.n][];

                //init the var declared above in the cplex model instance
                for(int k = 0; k < inst.m; k++) z[k] = cplex_bp.IntVar(0, inst.n, $"z[{k}]");
                
                for(int i = 0; i < inst.n; i++)
                {
                    for(int k = 0; k < inst.m; k++) x[i][k] = cplex_bp.BoolVar($"x[{i}][{k}]");
                }

                //add objective
                ILinearNumExpr obj = cplex_bp.LinearNumExpr();

                for(int k = 0; k < inst.m; k++) obj.AddTerm(inst.types[k].cost, z[k]);

                cplex_bp.AddMinimize(obj);

                /*Constraint #1: ensure that each item is assigned only once and to only one bin type*/ 
                for(int i = 0; i < inst.n; i++)
                {
                    constraint = cplex_bp.LinearIntExpr();

                    for (int k = 0; k < inst.m; k++) constraint.AddTerm(1, x[i][k]);

                    cplex_bp.AddEq(constraint, 1);
                }
                 
                /*Constraint #2: ensure that there are enough bin type k opened for the assigned items*/
                for(int k = 0; k < inst.m; k++)
                {
                    constraint = cplex_bp.LinearIntExpr();

                    for (int i = 0; i < inst.n; i++) constraint.AddTerm((int)inst.items[i].area, x[i][k]);

                    cplex_bp.AddLe(constraint, inst.types[k].cost);
                }

                /*Constraint #3: DFF constraints*/
                for (int k = 0; k < inst.m; k++)
                {
                    List<DFFRow> constraints = FeasibilityConstraint.Generate(inst.items, inst.types[k], 0.01, 0.49, 0.05);
                }


            }
            catch (ILOG.Concert.Exception exc)
            {
                System.Console.WriteLine("Concert exception '" + exc + "' caught");
            }
        }
    }
}
