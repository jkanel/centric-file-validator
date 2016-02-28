using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public List<String> DomainValues = new List<string>();

        public RowDefinition RowDefinition;

        public ColumnDefinition(RowDefinition RowDefinition)
        {
            this.RowDefinition = RowDefinition;
        }


        public void Validate(string RowValue, int RowPosition, List<ValidationError> ValidationErrors)
        {

          // test for required but missing values
          if(this.Required && (RowValue == null || RowValue.Length == 0))
          {
              ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDisposition, this.Position, this.Label,
                  string.Format("Column is required but the row did not present a value.", 
                    RowPosition, this.Position, this.Label)
                  ));

              return;

          } 

          switch(DataType.ToUpper())
          {
              case "TEXT":

                  if(!this.Truncate && this.MaxLength > 0 && RowValue.Length > this.MaxLength)
                  {
                      // non-critical error
                    ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDisposition, this.Position, this.Label,
                          string.Format("Value of \"{0}\" exceeds maximum length of {1} and will not be truncated.",
                            RowValue.Substring(0,this.MaxLength), this.MaxLength)
                          ));
                  }
                  break;

              case "DECIMAL":

                  decimal d;

                  if(decimal.TryParse(RowValue, out d)==false)
                  {

                    ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDisposition, this.Position, this.Label,
                          string.Format("Value of \"{0}\" is does not meet the expected DECIMAL format.", RowValue)
                          ));

                  }

                  break;

              case "DATE":

                  DateTime dt;

                  if (DateTime.TryParse(RowValue, out dt) == false)
                  {

                    ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDisposition, this.Position, this.Label,
                          string.Format("Value of \"{0}\" does not meet the expected DATE format.", RowValue)
                          ));

                  } else
                    // test of value only a date without time component (check by adding a time component to see if it parses)
                    if (DateTime.TryParse(RowValue + " 23:23:59", out dt) == false)
                    {

                      ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDisposition, this.Position, this.Label,
                          string.Format("Value of \"{0}\" is a TIMESTAMP but not a DATE.", RowValue)
                          ));

                    }

                  break;

              case "TIMESTAMP":

                  DateTime ts;

                  // test of value is not a datetime
                  if (DateTime.TryParse(RowValue, out ts) == false)
                  {

                    ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDisposition, this.Position, this.Label,
                          string.Format("Value of \"{0}\" does not meet the expected TIMESTAMP format.", RowValue)
                          ));


                  } else
                      // test of value only a date without time component (check by adding a time component to see if it parses)
                      if (DateTime.TryParse(RowValue + " 23:23:59", out ts) == true)
                  {

                    ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDisposition, this.Position, this.Label,
                              string.Format("Value of \"{0}\" is a DATE but not a TIMESTAMP.", RowValue)
                              ));

                  }

                  break;

              case "INTEGER":

                  int i;

                  if (int.TryParse(RowValue, out i) == false)
                  {

                    ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDisposition, this.Position, this.Label,
                          string.Format("Value of \"{0}\" is does not meet the expected INTEGER format.", RowValue)
                          ));

                  }

                  break;

              }


              if(this.DomainValues.Count > 0 && !this.DomainValues.Contains(RowValue))
              {
                ValidationErrors.Add(new ValidationError(RowPosition, this.RowDefinition.RowDisposition, this.Position, this.Label,
                      string.Format("Value of \"{0}\" is not in the list of allowable values.", RowValue)
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
