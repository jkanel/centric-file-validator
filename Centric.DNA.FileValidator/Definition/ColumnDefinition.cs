using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Centric.DNA.File
{
    public class ColumnDefinition
    {
        public string Label;
        public string DataType;
        public bool Required;
        public int MaxLength;
        public bool Truncate;
        public int Position;
        public string RegexPattern;
        public string DomainList;

        public List<String> DomainValues = new List<string>();

        public RowDefinition RowDefinition;

        public ColumnDefinition(RowDefinition RowDefinition)
        {
            this.RowDefinition = RowDefinition;
        }


        public void Validate(string RowValue, int RowPosition, List<ValidationError> ValidationErrors)
        {

          // test for missing values
          if (RowValue == null || RowValue.Length == 0)
          {

            // if the value is required add a validation error
            if (this.Required)
            {
              ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDispositionPhrase, this.Position, this.Label,
                  string.Format("Column is required but the row did not present a value.",
                    RowPosition, this.Position, this.Label)
                  ));

            }

            // do not proceed with validation of missing values.
            return;

          }

          // test data types
          switch(DataType.ToUpper())
          {
              case "TEXT":
                  ValidateText(RowValue, RowPosition, ValidationErrors);
                  break;

              case "DECIMAL":
                  ValidateDecimal(RowValue, RowPosition, ValidationErrors);
                  break;

              case "DATE":
                  ValidateDate(RowValue, RowPosition, ValidationErrors);
                  break;

              case "TIMESTAMP":
                  ValidateTimestamp(RowValue, RowPosition, ValidationErrors);
                  break;

              case "INTEGER":
                  ValidateInteger(RowValue, RowPosition, ValidationErrors);
                  break;

            }

            // test domain values
            ValidateDomain(RowValue, RowPosition, ValidationErrors);

            // test regex pattern
            ValidateRegexPattern(RowValue, RowPosition, ValidationErrors);
        }

        protected void ValidateRegexPattern(string RowValue, int RowPosition, List<ValidationError> ValidationErrors)
        {
          if (this.RegexPattern != null && !new Regex(RegexPattern).IsMatch(RowValue))
          {
            ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDispositionPhrase, this.Position, this.Label,
              string.Format("Value of \"{0}\" does not match the Regex pattern of \"{1}\".", RowValue, this.RegexPattern)
              ));
          }
        }

        protected void ValidateDomain(string RowValue, int RowPosition, List<ValidationError> ValidationErrors)
        {
          if (this.DomainList != null && !this.DomainValues.Contains(RowValue))
          {
            ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDispositionPhrase, this.Position, this.Label,
              string.Format("Value of \"{0}\" is not in the list of allowable values.", RowValue)
              ));
          }
        }

        protected void ValidateText(string RowValue, int RowPosition, List<ValidationError> ValidationErrors)
        {
          if (!this.Truncate && this.MaxLength > 0 && RowValue.Length > this.MaxLength)
          {
            // non-critical error
            ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDispositionPhrase, this.Position, this.Label,
              string.Format("Value of \"{0}\" exceeds maximum length of {1} and will not be truncated.",
                RowValue.Substring(0, this.MaxLength), this.MaxLength)
              ));
          }
        }


        protected void ValidateInteger(string RowValue, int RowPosition, List<ValidationError> ValidationErrors)
        {
          int i;

          if (int.TryParse(RowValue, out i) == false)
          {

            ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDispositionPhrase, this.Position, this.Label,
              string.Format("Value of \"{0}\" is does not meet the expected INTEGER format.", RowValue)
              ));

          }
        }

        protected void ValidateDecimal(string RowValue, int RowPosition, List<ValidationError> ValidationErrors)
        {
          decimal d;

          if (decimal.TryParse(RowValue, out d) == false)
          {

            ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDispositionPhrase, this.Position, this.Label,
              string.Format("Value of \"{0}\" is does not meet the expected DECIMAL format.", RowValue)
              ));

          }
        }


        protected void ValidateDate(string RowValue, int RowPosition, List<ValidationError> ValidationErrors)
        {

          DateTime dt;

          if (DateTime.TryParse(RowValue, out dt) == false)
          {

            ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDispositionPhrase, this.Position, this.Label,
              string.Format("Value of \"{0}\" does not meet the expected DATE format.", RowValue)
              ));

          }
          else
            // test of value only a date without time component (check by adding a time component to see if it parses)
            if (DateTime.TryParse(RowValue + " 23:23:59", out dt) == false)
            {

              ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDispositionPhrase, this.Position, this.Label,
                string.Format("Value of \"{0}\" is a TIMESTAMP but not a DATE.", RowValue)
                ));
            }
        }


        protected void ValidateTimestamp(string RowValue, int RowPosition, List<ValidationError> ValidationErrors)
        {

          DateTime ts;

          // test of value is not a datetime
          if (DateTime.TryParse(RowValue, out ts) == false)
          {

            ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDispositionPhrase, this.Position, this.Label,
              string.Format("Value of \"{0}\" does not meet the expected TIMESTAMP format.", RowValue)
              ));

          }
          // test of value only a date without time component (check by adding a time component to see if it parses)
          else if (DateTime.TryParse(RowValue + " 23:23:59", out ts) == true)          
          {

            ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDispositionPhrase, this.Position, this.Label,
              string.Format("Value of \"{0}\" is a DATE but not a TIMESTAMP.", RowValue)
              ));

          }
        }

        /// <summary>
        /// Returns the a comma-delimited column label list for all column positions after the specified position.
        /// </summary>
        /// <param name="ColumnDefinitions"></param>
        /// <param name="ColumnPosition"></param>
        /// <returns></returns>
        public static string ColumnLabelsPostPosition(List<ColumnDefinition> ColumnDefinitions, int ColumnPosition)
        {
            string ColumnLabels = null;

            if (ColumnPosition >= ColumnDefinitions.Count)
            {
                return null;
            }
            else
            {

                for (int p = ColumnPosition; p < ColumnDefinitions.Count; p++)
                {
                    if (ColumnLabels == null)
                    {
                        ColumnLabels = ColumnDefinitions[p].Label;
                    }
                    else
                    {
                        ColumnLabels += "," + ColumnDefinitions[p].Label;
                    }
                }
            }

            return ColumnLabels;
        }
    }
}
