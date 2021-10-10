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
    static class FeasibilityCheck
    {
        /*Place item in height order from tallest to shortest to encourage verticle cuts
         *Priortize stacking items on top before placing on the right side.
         */
        static public bool Packing(List<Item> assignment, BinType bin)
        { 
            assignment.Sort(Item.CompareItemByHeight);
            List<Item> availableItems = assignment;
            List<Item> packedItems = new List<Item>();

            //Check if any item is too big for the bin
            for (int i = 0; i < assignment.Count; i++)
            {
                if (!assignment[i].CanFit(bin)) return false;
            }

            Rectangle binSize = new Rectangle(bin.height, bin.width);

            return Packing(ref availableItems, ref packedItems, binSize);
        }

        static public bool Packing(ref List<Item> assignment, ref List<Item> packedItems, Rectangle binSize)
        {
            if (assignment.Count == 0) return true;
            else
            {
                for (int i = 0; i < assignment.Count; i++)
                {
                    if (!assignment[i].CanFit(new BinType(binSize.height,binSize.width))) continue;
                    else
                    {
                        Item packedItem = assignment[i];
                        packedItems.Add(packedItem);
                        assignment.RemoveAt(i);
                        int packedCounter = packedItems.Count();

                        Rectangle topVert = new Rectangle(binSize.height - packedItem.height, packedItem.width);
                        Rectangle rightVert = new Rectangle(binSize.height, binSize.width - packedItem.width);
                        Rectangle topHori = new Rectangle(binSize.height - packedItem.height, binSize.width);
                        Rectangle rightHori = new Rectangle(packedItem.height, binSize.width - packedItem.width);

                        if(Rectangle.ComparebyArea(topVert,rightVert) > 0)
                        {
                            if (Packing(ref assignment, ref packedItems, rightVert) || Packing(ref assignment, ref packedItems, topVert)) return true;
                            else
                            {
                                rollBack(ref assignment, ref packedItems, packedItems.Count - packedCounter);
                                if (Rectangle.ComparebyArea(topHori, rightHori) > 0) return Packing(ref assignment, ref packedItems, rightHori) || Packing(ref assignment, ref packedItems, topHori);
                                else
                                {
                                    rollBack(ref assignment, ref packedItems, packedItems.Count - packedCounter);
                                    return Packing(ref assignment, ref packedItems, topHori) || Packing(ref assignment, ref packedItems, rightHori);
                                }
                            }
                        }
                        else
                        {
                            if (Packing(ref assignment, ref packedItems, topVert) || Packing(ref assignment, ref packedItems, rightVert)) return true;
                            else
                            {
                                rollBack(ref assignment, ref packedItems, packedItems.Count - packedCounter);
                                if (Rectangle.ComparebyArea(topHori, rightHori) > 0) return Packing(ref assignment, ref packedItems, rightHori) || Packing(ref assignment, ref packedItems, topHori);
                                else
                                {
                                    rollBack(ref assignment, ref packedItems, packedItems.Count - packedCounter);
                                    return Packing(ref assignment, ref packedItems, topHori) || Packing(ref assignment, ref packedItems, rightHori);
                                }
                            }
                        }
                    }
                }

                return false;
            }
        }
        static private void rollBack(ref List<Item> a, ref List<Item> b, int times)
        {
            if (times == 0) return;
            for(int i = 0; i < times; i++)
            {
                a.Add(b[b.Count - 1]);
                b.RemoveAt(b.Count - 1);
            }
            a.Sort(Item.CompareItemByHeight);
        }

        static public bool MIPPacking(List<Item> assignment, BinType binType)
        {
            if (assignment.Count() == 0) return true;

            Bin bin = new Bin(binType);

            //List of region/s with confirmed cut/s
            List<Region> currentRegions = new List<Region>();

            //Adding whole bin as the first region to be cut
            currentRegions.Add(new Region(bin, binType));

            bool feasibilityFlag = false;
            bool exitFlag = true;

            do
            {
                List<Region> bestRegions = new List<Region>();
                double bestObj = 2;
                foreach (Region region in currentRegions)
                {
                    List<int> cutVertical = NormalPattern(assignment, region, false);
                    List<int> cutHorizontal = NormalPattern(assignment, region, true);

                    List<Region> packingRegions = new List<Region>();
                    foreach (Region tempRegion in currentRegions) packingRegions.Add(tempRegion);
                    packingRegions.Remove(region);

                    foreach (int cut in cutVertical)
                    {
                        Region regionLeft= new Region(bin, region.height, cut, region.x, region.y);
                        Region regionRight = new Region(bin, region.height, region.width - cut, region.x + cut, region.y);

                        packingRegions.Add(regionLeft);
                        packingRegions.Add(regionRight);

                        double MIPObj = MinSum(assignment, packingRegions, binType, bestObj);

                        if (MIPObj < bestObj)
                        {
                            bestObj = MIPObj;
                            bestRegions.Clear();

                            foreach (Region tempRegion in packingRegions) bestRegions.Add(tempRegion);
                        }

                        packingRegions.Remove(regionLeft);
                        packingRegions.Remove(regionRight);

                    }

                    foreach (int cut in cutHorizontal)
                    {
                        Region regionBot = new Region(bin, cut, region.width, region.x, region.y);
                        Region regionTop = new Region(bin, region.height - cut, region.width, region.x, region.y + cut);

                        packingRegions.Add(regionBot);
                        packingRegions.Add(regionTop);

                        double MIPObj = MinSum(assignment, packingRegions, binType, bestObj);

                        if (MIPObj < bestObj)
                        {
                            bestObj = MIPObj;
                            bestRegions.Clear();

                            foreach (Region tempRegion in packingRegions) bestRegions.Add(tempRegion);
                        }

                        packingRegions.Remove(regionBot);
                        packingRegions.Remove(regionTop);
                    }
                }
                if (bestObj == 2 || bestRegions.Count >= assignment.Count)
                {
                    foreach (Region tempRegion in bestRegions) currentRegions.Add(tempRegion);
                    exitFlag = false;
                }
                else
                {
                    foreach (Region r in bestRegions) Console.WriteLine(r.ToString());
                    if (!feasibilityFlag) feasibilityFlag = true;
                    currentRegions.Clear();
                }
            }
            while (exitFlag);

            return feasibilityFlag;
        }


        static public bool MIPPackingQueue(List<Item> assignment, BinType binType)
        {
            if (assignment.Count() == 0) return true;

            bool feasibilityFlag = false;
            Bin bin = new Bin(binType);

            //Store all regions that need to be cut and checked
            Queue<Region> availableRegions = new Queue<Region>();

            //Store final regions that cannot be cut any further
            Queue<Region> finalRegions = new Queue<Region>();

            //Adding whole bin as the first region to be cut
            availableRegions.Enqueue(new Region(bin, binType));

            do
            {
                Region region = availableRegions.Dequeue();

                double bestObj = 2;
                double MIPObj;
                List <Region> bestRegions = new List<Region>();

                List<int> cutVertical = NormalPattern(assignment, region, false);
                List<int> cutHorizontal = NormalPattern(assignment, region, true);

                List<Region> packingRegions = new List<Region>(availableRegions);
                packingRegions.AddRange(finalRegions);
                //foreach(Region r in packingRegions) Console.WriteLine(r.ToString());

                foreach (int cut in cutVertical)
                {
                    Region regionLeft = new Region(bin, region.height, cut, region.x, region.y);
                    Region regionRight = new Region(bin, region.height, region.width - cut, region.x + cut, region.y);

                    packingRegions.Add(regionLeft);
                    packingRegions.Add(regionRight);

                    MIPObj = MinSum(assignment, packingRegions, binType, bestObj);

                    if(MIPObj< bestObj)
                    {
                        bestObj = MIPObj;
                        bestRegions.Clear();

                        bestRegions.Add(regionLeft);
                        bestRegions.Add(regionRight);

                    }

                    packingRegions.Remove(regionLeft);
                    packingRegions.Remove(regionRight);
                }

                foreach (int cut in cutHorizontal)
                {
                    Region regionBot = new Region(bin, cut, region.width, region.x, region.y);
                    Region regionTop = new Region(bin, region.height - cut, region.width, region.x, region.y + cut);

                    packingRegions.Add(regionBot);
                    packingRegions.Add(regionTop);

                    MIPObj = MinSum(assignment, packingRegions, binType, bestObj);

                    if (MIPObj < bestObj)
                    {
                        bestObj = MIPObj;
                        bestRegions.Clear();

                        bestRegions.Add(regionBot);
                        bestRegions.Add(regionTop);

                    }

                    packingRegions.Remove(regionBot);
                    packingRegions.Remove(regionTop);
                }


                
                if(bestObj == 2)
                {
                    finalRegions.Enqueue(region);
                    packingRegions.Add(region);
                }
                else
                {
                    if (!feasibilityFlag) feasibilityFlag = true;

                    bestRegions.Sort(Region.ComparebyArea);
                    foreach (Region r in bestRegions) availableRegions.Enqueue(r);
                }

                foreach (Region r in bestRegions) packingRegions.Add(r);
                //foreach (Region r in packingRegions) Console.WriteLine(r.ToString());
                foreach (Region r in packingRegions) r.items.Clear();
                MIPObj = MinSum(assignment, packingRegions, binType, 2);

                foreach (Region r in packingRegions) Console.WriteLine(r.ToString());
                Console.WriteLine("---------------");
            }  
            while (availableRegions.Count != 0);

            List<Region> result = new List<Region>(finalRegions);

            foreach (Region r in result) Console.WriteLine(r.ToString());

            return feasibilityFlag;
        }

        static public List<int> NormalPattern(List<Item> items, Rectangle rec, bool heightWise)
        {
            List<int> result = new List<int>();
            int side;

            List<int> itemLength = new List<int>();
            if (heightWise)
            {
                foreach(Item item in items) itemLength.Add((int)item.height);
                side = (int)rec.height;
            }
            else
            {
                foreach (Item item in items) itemLength.Add((int)item.width);
                side = (int)rec.width;
            }

            int[] T = new int[side + 1];
            Array.Clear(T, 0, T.Length);
            T[0] = 1;
            int threshold = side - itemLength.Min();

            foreach (int length in itemLength)
            {
                for (int i = side - length; i >= 0; i--)
                {
                    int cut = i + length;
                    if ((T[i] == 1) && (cut <= threshold)) T[cut] = 1;
                }
            }

            T[0] = 0;

            for (int i = side; i >= 0; i--)
            {
                if (T[i] == 1) result.Add(i);
            }

            result.Sort();

            return result;
        }

        static private double MinSum(List<Item> items, List<Region> regions, BinType binType, double bestObj)
        {
            try
            {
                //init model
                Cplex cplex_bp = new Cplex();

                //set CPlex parameters
                cplex_bp.SetParam(Cplex.IntParam.MIPDisplay, 0);
                cplex_bp.SetParam(Cplex.Param.TimeLimit, 2);
                cplex_bp.SetParam(Cplex.DoubleParam.EpGap, 0.05);
                cplex_bp.SetParam(Cplex.Param.Emphasis.MIP, 3);
                cplex_bp.SetOut(System.IO.TextWriter.Null);
                cplex_bp.SetWarning(System.IO.TextWriter.Null);

                //reset model for use
                cplex_bp.ClearModel();

                //cost variable to be minimised
                INumVar C = cplex_bp.NumVar(0, 1,"C");

                //decision varsiables to assign items (i) to regions (j)
                IIntVar[][] x = new IIntVar[items.Count][];

                //init the decision var above
                for(int i = 0; i < items.Count; i++)
                {
                    x[i] = new IIntVar[regions.Count];
                    for (int j = 0; j < regions.Count; j++) x[i][j] = cplex_bp.BoolVar($"x[{i}][{j}]");
                }

                //add objective
                ILinearNumExpr obj = cplex_bp.LinearNumExpr();
                obj.AddTerm(1,C);

                cplex_bp.AddMinimize(obj);

                /*Constraint #1: ensure that each item is assigned only once*/
                for(int i = 0; i < items.Count; i++)
                {
                    ILinearIntExpr constraint = cplex_bp.LinearIntExpr();
                    for (int j = 0; j < regions.Count; j++) constraint.AddTerm(1, x[i][j]);
                    cplex_bp.AddEq(constraint, 1);
                }

                /*constraint #2*/
                for(int j = 0; j < regions.Count; j++)
                {
                    ILinearNumExpr constraint = cplex_bp.LinearNumExpr();

                    for (int i = 0; i < items.Count; i++)
                    {
                        double cost = items[i].area / regions[j].area;
                        constraint.AddTerm(cost, x[i][j]);
                    }
                    constraint.AddTerm(-1, C);

                    cplex_bp.AddLe(constraint, 0);
                }

                /*constraint #3*/
                for (int j = 0; j < regions.Count; j++)
                {
                    List<DFFRow> dffRows = FeasibilityConstraint.Generate(items, regions[j], 0.01, 0.49, 0.05);
                    for(int f = 0; f < Math.Min(dffRows.Count, 10); f++)
                    {
                        ILinearNumExpr constraint = cplex_bp.LinearNumExpr();
                        for (int i = 0; i < items.Count; i++)
                        {
                            if (dffRows[f].row[i] == 0) continue;
                            constraint.AddTerm(dffRows[f].row[i], x[i][j]);
                        }
                        constraint.AddTerm(-1, C);

                        cplex_bp.AddLe(constraint, 0);
                    }
                }

                /*constraint #4*/
                cplex_bp.AddLe(C, 1);

                /*constraint #5: Ensure that each region has atleast one item packed into it*/
                for(int j = 0; j < regions.Count; j++)
                {
                    ILinearIntExpr constraint = cplex_bp.LinearIntExpr();
                    for (int i = 0; i < items.Count; i++) constraint.AddTerm(1, x[i][j]);
                    cplex_bp.AddGe(constraint, 1);
                }

                if (cplex_bp.Solve())
                {
                    if (cplex_bp.GetObjValue() < bestObj)
                    {
                        for (int i = 0; i < items.Count; i++)
                        {
                            for (int j = 0; j < regions.Count; j++)
                            {
                                if ((int)Math.Round(cplex_bp.GetValue(x[i][j])) != 1) continue;
                                regions[j].AddItem(items[i],0,0);
                            }    
                        }
                    }
                    return cplex_bp.GetObjValue();
                }
                else return 2;
            }
            catch (ILOG.Concert.Exception exc)
            {
                System.Console.WriteLine("Concert exception " + exc + " caught");
                return 2;
            }
        }
    }
}
