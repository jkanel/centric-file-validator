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

        public string RowDisposition
        {
          get{
              return this.DispositionColumnValue;
          }
        }

        public string RowDispositionPhrase
        {
          get
          {
            return string.Format("[{0}]=\"{1}\"", this.DispositionColumnLabel, this.DispositionColumnValue);
          }
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
          catch
          {
            return false;
          }
        }

        public void Validate(string[] RowValues, int RowPosition, List<ValidationError> ValidationErrors)
        {

          int RowValueCount = RowValues.Length;

          ValidateRowValueCount(RowValueCount, RowPosition, ValidationErrors);

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

      protected void ValidateRowValueCount(int RowValueCount, int RowPosition, List<ValidationError> ValidationErrors)
      {

        // check values and column counts
        if (RowValueCount > this.ColumnDefinitions.Count)
        {
          ValidationErrors.Add(new ValidationError(RowPosition, this.RowDispositionPhrase,
                string.Format("Contains too many values: {0} value(s) were provided, {1} were expected.",
                  RowValueCount, this.ColumnDefinitions.Count)));

        }
        else if (RowValueCount < this.ColumnDefinitions.Count)
        {

          ValidationErrors.Add(new ValidationError(RowPosition, this.RowDispositionPhrase,
                string.Format("Contains too few values: {0} value(s) were provided, {1} were expected.",
                  RowValueCount, this.ColumnDefinitions.Count)));

          ValidationErrors.Add(new ValidationError(RowPosition, this.RowDispositionPhrase,
                string.Format("Missing values for the following columns: {0}.",
                 ColumnDefinition.ColumnLabelsPostPosition(this.ColumnDefinitions, RowValueCount))));

        }
      }
        
    }
}
