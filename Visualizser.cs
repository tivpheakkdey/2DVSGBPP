using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using _2DWVSBPP_with_Visualizer.Problem;

namespace _2DWVSBPP_with_Visualizer
{
    public partial class Visualizser : Form
    {
        List<List<Bin>> solutionList = new List<List<Bin>>();
        double binMaxHeight;
        double binMaxWidth;

        Font myFont = new Font("Arial", 10);
        Pen myPen = new Pen(Color.Black);
        Brush indexBrush = Brushes.Black;
        Brush itemBrush = new SolidBrush(Color.Green);
        Brush binBrush = new SolidBrush(Color.Gray);

        PointF origin = new PointF(0, 0);

        int multiplier = 1;
        int solutionIndexA = 0;
        int solutionIndexB = 0;

        public Visualizser(List<List<Bin>> solutionsList, double binMaxWidth ,double binMaxHeight)
        {
            this.solutionList = solutionsList;
            this.binMaxWidth = binMaxWidth;
            this.binMaxHeight = binMaxHeight;
            InitializeComponent();
        }

        private void DrawSolution(int multiplier, int solutionIndex, FlowLayoutPanel panel)
        {
            PointF binOrigin = origin;

            panel.Controls.Clear(); 

            foreach (Bin bin in solutionList[solutionIndex])
            {
                PictureBox pb = new PictureBox();
                binOrigin.Y = multiplier * ((float)binMaxHeight - (float)bin.type.height);
                SizeF binSizeF = new SizeF(multiplier * (float)bin.type.width, multiplier * (float)bin.type.height);
                RectangleF binRectF = new RectangleF(binOrigin, binSizeF);
                Rectangle binRect = Rectangle.Round(binRectF);

                Bitmap drawSurfacing = new Bitmap(Convert.ToInt32(multiplier * binMaxWidth), Convert.ToInt32(multiplier * (int)binMaxHeight));
                using (var g = Graphics.FromImage(drawSurfacing))
                {
                    g.FillRectangle(binBrush, binRect); //Draw solid bin
                    g.DrawRectangle(myPen, binRect); //Draw the item out line

                    for (int k = 0; k < bin.items.Count; k++)
                    {
                        PointF itemOrigin = new PointF(binOrigin.X + multiplier * (float)bin.xy[k].Item2, multiplier * ((float)binMaxHeight - (float)bin.items[k].height - (float)bin.xy[k].Item1));
                        SizeF itemSizeF = new SizeF(multiplier * (float)bin.items[k].width, multiplier * (float)bin.items[k].height);

                        RectangleF itemF = new RectangleF(itemOrigin, itemSizeF);
                        Rectangle item = Rectangle.Round(itemF);

                        g.FillRectangle(itemBrush, item);
                        g.DrawRectangle(myPen, item);
                        g.DrawString($"{bin.items[k].index} ({bin.items[k].width}x{bin.items[k].height})", myFont, indexBrush, item);
                    }
                    g.DrawString($"{bin.type.width} x {bin.type.height}", myFont, indexBrush, binOrigin.X, multiplier * ((float)binMaxHeight) - 20);
                }

                pb.SizeMode = PictureBoxSizeMode.AutoSize;
                pb.Image = drawSurfacing;
                panel.Controls.Add(pb);
            }
        }

        private void Close(object sender, EventArgs e)
        {
            this.Close();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            multiplier = (int)numericUpDown1.Value;

            DrawSolution(multiplier, solutionIndexA,flowLayoutPanel1);
            DrawSolution(multiplier, solutionIndexB, flowLayoutPanel2);
        }

        private void Visualizser_Load(object sender, EventArgs e)
        {
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.FlowDirection = FlowDirection.LeftToRight;
            flowLayoutPanel1.WrapContents =true;

            for(int i = 0; i < solutionList.Count; i++)
            {
                comboBox1.Items.Add(i);
                comboBox2.Items.Add(i);
            }

            DrawSolution(multiplier, solutionIndexA, flowLayoutPanel1);
            DrawSolution(multiplier, solutionIndexB, flowLayoutPanel2); 
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            solutionIndexA = Convert.ToInt32(comboBox1.Text);
            DrawSolution(multiplier, solutionIndexA, flowLayoutPanel1);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            solutionIndexB = Convert.ToInt32(comboBox2.Text);
            DrawSolution(multiplier, solutionIndexB, flowLayoutPanel2);
        }
    }
}
