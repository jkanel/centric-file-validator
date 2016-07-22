using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Centric.DNA.File.Definition;

namespace Centric.DNA.File
{
    public class FileDefinition
    {

        public string Label;

        public char ColumnDelimiter = Char.Parse("\t");
        public int SkipRows = 0;
        public bool IgnoreEmptyRows = true;
        public bool TrimSpaces = true;
        public bool StripQuotes = true;
        public bool CompressWhitespace = true;
        public System.Text.Encoding Encoding = System.Text.Encoding.GetEncoding("ISO-8859-1");
       
        public List<RowDefinition> RowDefinitions = new List<RowDefinition>();
        
        public string[] ConvertRowToValues(string RowText)
        {

            string[] Values = RowText.Split(this.ColumnDelimiter);
            string value = null;
            bool changed = false;

            for (int i = 0; i < Values.Length; i++)
            {
                // get the value from the array
                value = Values[i];
                changed = false;

                // compress whitespace, replace with a single space
                if (this.CompressWhitespace == true)
                {
                    value = Regex.Replace(value, @"\s+"," ");
                    changed = true;
                }

                // trim leading or trailing spaces
                if (this.TrimSpaces == true)
                {
                    value = value.Trim();
                    changed = true;
                }

                // strip surrounding quotes
                if (this.StripQuotes == true && value.Length > 1 && value.Substring(0, 1) == "\"" && value.Substring(value.Length-1,1) =="\"")
                {
                  value = value.Substring(1, value.Length - 2);
                  changed = true;
                }

                // if the value was changed replace it in the array
                if (changed == true)
                {
                    Values[i] = value;
                }
            }

            return Values;

        }


        public RowDefinition FindRowDefinition(string[] Values)
        {
            foreach(RowDefinition rd in this.RowDefinitions)
            {
                if (rd.MatchesRowDisposition(Values) == true)
                {
                    return rd;
                }
            }

            return (RowDefinition)null;
        }

        public string FindDispositionValue(string[] Values)
        {
          foreach (RowDefinition rd in this.RowDefinitions)
          {
            if (rd.MatchesRowDisposition(Values) == true)
            {
              return rd.DispositionColumnValue;
            }
          }

          return (string)null;
        }

    }
}
