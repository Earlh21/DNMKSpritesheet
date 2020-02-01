using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace DNMKSpritesheet
{
    public class DataString
    {
        public string Name { get; set; }
        public List<DataString> Data { get; set; }

        public void AddData(DataString data)
        {
            Data.Add(data);
        }
        
        public DataString(string name)
        {
            Name = name;
            Data = new List<DataString>();
        }

        private List<string> GetLines()
        {
            List<string> lines = new List<string>();
            
            if (Data.Count == 0)
            {
                lines.Add(Name);
                return lines;
            }
            
            lines.Add(Name + " {");

            foreach (DataString data_string in Data)
            {
                List<string> new_lines = data_string.GetLines();

                for (int i = 0; i < new_lines.Count; i++)
                {
                    new_lines[i] = '\t' + new_lines[i];
                }

                lines.AddRange(new_lines);
            }
            
            lines.Add("}");

            return lines;
        }
        
        public override string ToString()
        {
            List<string> lines = GetLines();

            string to_return = lines[0];

            for (int i = 1; i < lines.Count; i++)
            {
                string line = lines[i];

                to_return += Environment.NewLine + line;
            }

            return to_return;
        }
    }
}