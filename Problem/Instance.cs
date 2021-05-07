using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DWVSBPP_with_Visualizer.Problem
{
    public class Instance
    {
        public string problem_class { get; private set; }
        public string relative_index { get; private set; }
        public string absolute_index { get; private set; }
        public int n { get; private set; }
        public int m { get; private set; }
        public List<Item> items { get; private set; } = new List<Item>();
        public List<BinType> types { get; private set; } = new List<BinType>();
        public double max_height { get; private set; }
        public double max_width { get; private set; }
        public double max_area { get; private set; }

        public Instance(string path)
        {
            //read from instance file
            StreamReader inFile = new StreamReader(path);

            string line = inFile.ReadLine().Trim();
            string[] value = Regex.Split(line, @"\s+");
            problem_class = value[0];

            //read in number of items and bins
            line = inFile.ReadLine().Trim();
            value = Regex.Split(line, @"\s+");
            n = Convert.ToInt32(value[0]);
            m = Convert.ToInt32(value[1]);
            //m = 1;

            line = inFile.ReadLine().Trim();
            value = Regex.Split(line, @"\s+");
            relative_index = value[0];
            absolute_index = value[1];

            //read in dimension of bins
            line = inFile.ReadLine().Trim();
            value = Regex.Split(line, @"\s+");

            for (int i = 0; i < m; i++)
            {
                int tempHeight = Convert.ToInt32(value[i * 2]);
                int tempWidth = Convert.ToInt32(value[(i * 2) + 1]);
                types.Add(new BinType(tempHeight, tempWidth));
            }

            //set bin type cost to their area (tiv)
            for (int i = 0; i < m; i++) types[i].SetCost(types[i].width* types[i].height);

            //read in bin profit
            line = inFile.ReadLine().Trim();
            value = Regex.Split(line, @"\s+");

            // NOTE: we use the area as a measure of the cost. This is different to the cost parameter stated in the test instances.
            //for (int i = 0; i < m; i++) types[i].SetCost(Convert.ToDouble(value[i]));            
            for (int i = 0; i < m; i++) types[i].SetCost(Convert.ToDouble(types[i].height * types[i].width));

            for (int i = 0; i < n; i++)
            {
                line = inFile.ReadLine().Trim();
                value = Regex.Split(line, @"\s+");
                items.Add(new Item(i, Convert.ToInt32(value[0]), Convert.ToInt32(value[1])));
            }

            items.Sort();
            for (int i = 0; i < n; i++) items[i].index = i;

            max_height = items.Max(x => x.height);
            max_width = items.Max(x => x.width);
            max_area = items.Max(x => x.area);
            inFile.Close();
        }

        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Instance: [{problem_class}][{relative_index}][{absolute_index}]");
            sb.AppendLine($"n: \t{n}");
            sb.AppendLine($"n: \t{m}");
            sb.AppendLine($"List of items:");
            for (int i = 0; i < n; i++) sb.AppendLine("\t" + items[i].ToString());
            sb.AppendLine($"List of bin types:");
            for (int i = 0; i < m; i++) sb.AppendLine("\t" + types[i].ToString());
            return sb.ToString();
        }

    }
}
