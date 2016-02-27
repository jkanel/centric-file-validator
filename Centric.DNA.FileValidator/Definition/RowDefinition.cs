using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Centric.DNA.File
{
    public class RowDefinition
    {
        public string Label;
        public List<ColumnDefinition> ColumnDefinitions = new List<ColumnDefinition>();

        private int _DispositionColumnPosition;

        public string DispositionColumnLabel;
        public string DispositionColumnValue;
        public FileDefinition FileDefinition;

        public RowDefinition(FileDefinition FileDefinition)
        {
            this.FileDefinition = FileDefinition;
        }

        public string RowDisposition()
        {
            return string.Format("[{0}][{1}]=\"{2}\"", this.Label, this.DispositionColumnLabel, this.DispositionColumnValue);
        }

        public int DispositionColumnPosition
        {
          get
          {

            if (this._DispositionColumnPosition != 0)
            {
              return _DispositionColumnPosition;
            }
            else
            {

              int ColumnPosition = 0;

              foreach (ColumnDefinition cd in this.ColumnDefinitions)
              {
                ColumnPosition++;

                if (cd.Label.Equals(this.DispositionColumnLabel))
                {
                  _DispositionColumnPosition = ColumnPosition;
                }
              }

              return _DispositionColumnPosition;

            }
          }
        }

        public bool MatchesRowDisposition(string[] RowValues)
        {
          try
          {
            return RowValues[this.DispositionColumnPosition - 1].Equals(this.DispositionColumnValue);
          }
          catch (Exception e)
          {
            return false;
          }
        }

        public void Validate(string[] RowValues, int RowPosition, List<ValidationError> ValidationErrors)
        {

          // create informational validation error
          ValidationError rve = new ValidationError(RowPosition,
                   string.Format("Row {0} - Disposition {1}.",
                   RowPosition, this.RowDisposition()), ValidationErrorScope.Information);
          
          rve.ColumnPosition = -1;
          ValidationErrors.Add(rve);

          // check values and column counts
           if(RowValues.Length > this.ColumnDefinitions.Count)
           {
               ValidationErrors.Add(new ValidationError(RowPosition,
                   string.Format("Row {0} - Contains too many values: {1} value(s) were provided, {2} were expected.",
                   RowPosition, RowValues.Length, this.ColumnDefinitions.Count)));

           } else if (RowValues.Length < this.ColumnDefinitions.Count)
           {

               ValidationErrors.Add(new ValidationError(RowPosition,
                   string.Format("Row {0} - Contains too few values: {1} value(s) were provided, {2} were expected.",
                   RowPosition, RowValues.Length, this.ColumnDefinitions.Count)));

               ValidationErrors.Add(new ValidationError(RowPosition,
                   string.Format("Row {0} - Missing values for the following columns: {1}.",
                   RowPosition, ColumnDefinition.ColumnLabelsPostPosition(this.ColumnDefinitions, RowValues.Length))));

           }

           ColumnDefinition cd = null;

           for (int ColumnIndex = 0; ColumnIndex < RowValues.Length; ColumnIndex++)
           {
               
               if (ColumnIndex + 1 <= this.ColumnDefinitions.Count)
               {
                   cd = this.ColumnDefinitions[ColumnIndex];
                   cd.Validate(RowValues[ColumnIndex], RowPosition, ValidationErrors);
               }

           }



        }
        
    }
}
