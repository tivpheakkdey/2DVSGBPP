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
            assignment.Sort(Item.CompareItemByHeight);
            List<Item> availableItems = assignment;
            List<Item> packedItems = new List<Item>();

            //Check if any item is too big for the bin
            for (int i = 0; i < assignment.Count; i++)
            {
                if (!assignment[i].CanFit(bin.type)) return false;
            }

            Rectangle binSize = new Rectangle(bin.type.height, bin.type.width);

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
            for(int i = 0; i < times; i++)
            {
                a.Add(b[b.Count - 1]);
                b.RemoveAt(b.Count - 1);
            }
            a.Sort(Item.CompareItemByHeight);
        }
    }
}
