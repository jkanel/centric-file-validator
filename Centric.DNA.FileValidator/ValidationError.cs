using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;

namespace Centric.DNA.File
{
    public enum ValidationErrorScope
    {
      None = 0,
      File = 1,
      Row = 2,
      Column = 3
    }

    public enum ValidationErrorSeverity
    {
      None = 0,
      Information = 1,
      Warning = 2,
      Critical = 3
    }

    public class ValidationError
    {
      
        /// <summary>
        /// Ordinal position of the row line in the file.
        /// </summary>
        public int RowPosition = 0;

        /// <summary>
        /// Disposition of the row.
        /// </summary>
        public string RowDisposition = null;

        /// <summary>
        /// Ordinal position of the column in the row.
        /// </summary>
        public int ColumnPosition = 0;

        /// <summary>
        /// Label attributed to the column.
        /// </summary>
        public string ColumnLabel = null;

        /// <summary>
        /// Severity of the error (Critical, Warning, Information).
        /// </summary>
        public ValidationErrorSeverity Severity = ValidationErrorSeverity.None;

        /// <summary>
        /// Identifies the source of the filedation (File, Row, Column).
        /// </summary>
        public ValidationErrorScope Scope = ValidationErrorScope.None;

        /// <summary>
        /// Text message describe the error.
        /// </summary>
        public string Message;

        /// <summary>
        /// ValidationError constructor.
        /// </summary>
        public ValidationError(){ }
        
        /// <summary>
        /// ValidationError constructor that assumes the error has a type of 'File'.
        /// </summary>
        /// <param name="Message">Message describing the error.</param>
        /// <param name="ValidationErrorSeverity">Indicates whether the error is critical.  Defaults to 'true'.</param>
        public ValidationError(string Message, ValidationErrorSeverity Severity = ValidationErrorSeverity.Critical)
        {
          this.Scope = ValidationErrorScope.File;
          this.Message = Message;
          this.Severity = Severity;
        }
        
        /// <summary>
        /// ValidationError constructor that assumes the error has a type of 'Row'.
        /// </summary>
        /// <param name="RowPosition">Position of the file row that generated the error.</param>
        /// <param name="Message">Message describing the error.</param>
        /// <param name="ValidationErrorSeverity">Indicates whether the error is critical.  Defaults to 'true'.</param>
        public ValidationError(int RowPosition, string RowDisposition, string Message, ValidationErrorSeverity Severity = ValidationErrorSeverity.Critical)
        {
          this.Scope = ValidationErrorScope.Row;
            this.RowPosition = RowPosition;
            this.RowDisposition = RowDisposition;
            this.Message = Message;
            this.Severity = Severity;
        }

        /// <summary>
        /// ValidationError constructor that assumes the error has a type of 'Column'.
        /// </summary>
        /// <param name="ColumnPosition">Position of the column that generated the error.</param>
        /// <param name="ColumnLabel">Label of the column that generated the error.</param>
        /// <param name="RowPosition">Position of the file row that generated the error.</param>
        /// <param name="Message">Message describing the error.</param>
        /// <param name="ValidationErrorSeverity">Indicates whether the error is critical.  Defaults to Critical.</param>
        public ValidationError(int RowPosition, string RowDisposition, int ColumnPosition, string ColumnLabel, string Message, ValidationErrorSeverity Severity = ValidationErrorSeverity.Critical)
        {

          this.Scope = ValidationErrorScope.Column;
            this.RowPosition = RowPosition;
            this.RowDisposition = RowDisposition;
            this.ColumnLabel = ColumnLabel;
            this.ColumnPosition = ColumnPosition;
            this.Message = Message;
            this.Severity = Severity;

        }

        /// <summary>
        /// Returns the description of the error severity enumaration value.
        /// </summary>
        /// <returns></returns>
        public string SeverityName()
        {
          return Enum.GetName(this.Severity.GetType(), this.Severity).ToUpper();
        }

        /// <summary>
        /// String representation of the error.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {

          StringBuilder sb = new StringBuilder();
          switch(this.Scope)
          {
            case ValidationErrorScope.File:
              
              sb.AppendLine(string.Format("FILE - {0}", this.Message));

              break;

            case ValidationErrorScope.Row:

              sb.AppendLine(String.Empty);
              sb.AppendLine(string.Format("ROW #{0} - DISPOSITION: {1}", this.RowPosition, this.RowDisposition));
              sb.AppendLine(string.Format(">> {0}", this.Message));

              break;

            case ValidationErrorScope.Column:

              sb.AppendLine(String.Empty);
              sb.AppendLine(string.Format("ROW #{0} - DISPOSITION: {1}", this.RowPosition, this.RowDisposition));
              sb.AppendLine(string.Format(">> COLUMN #{0} [{1}] - {2}", this.ColumnPosition, this.ColumnLabel, this.Message));
              
              break;
          }

          return sb.ToString();

        }

        /// <summary>
        /// Determines the number of errors having a critical severity.
        /// </summary>
        /// <param name="ValidationErrors">List of ValidationErrors.</param>
        /// <returns>Number of errors having a critical severity.</returns>
        public static int CriticalErrorCount(List<ValidationError> ValidationErrors)
        {
          return ValidationErrors.Count(item => item.Severity == ValidationErrorSeverity.Critical);
        }

        /// <summary>
        /// Indicates whether the errors contain at least one critical error.
        /// </summary>
        /// <param name="ValidationErrors">List of ValidationErrors.</param>
        /// <returns>Returns true if there are any critical severity errors in the list of ValidationErrors.</returns>
        public static bool ContainsCriticalErrors(List<ValidationError> ValidationErrors)
        {
          return ValidationErrors.Exists(item => item.Severity == ValidationErrorSeverity.Critical);
        }


        /// <summary>
        /// Generates a file summary of the validation errors.
        /// </summary>
        /// <param name="ValidationErrors">List of ValidationErrors.</param>
        /// <param name="ExportFilePath">Fully qualified path of the export file.</param>
        public static void ExportToFile(List<ValidationError> ValidationErrors, string ExportFilePath)
        {
        
          StreamWriter sw = new StreamWriter(ExportFilePath);

          // always show the file Validation Errors regardless of scope
          List<ValidationError> VEList = ValidationErrors
            .OrderBy(e => e.RowPosition)
            .ThenBy(e => e.ColumnPosition)
            .ToList<ValidationError>();

          foreach (ValidationError ve in VEList)
          {
            sw.Write(ve.ToString());
          }

          sw.Close();

        }


        /// <summary>
        /// Generates a string summary of the validation errors.
        /// </summary>
        /// <param name="ValidationErrors">List of ValidationErrors.</param>
        /// <returns>Minimum severity used to filter reported errors.  Only applies to row and column errors.</returns>
        public static string ExportToString(List<ValidationError> ValidationErrors)
        {
          StringBuilder sb = new StringBuilder();

          List<ValidationError> VEList = ValidationErrors
            .OrderBy(e => e.RowPosition)
            .ThenBy(e => e.ColumnPosition)
            .ToList<ValidationError>();

          foreach (ValidationError ve in VEList)
          {

            sb.Append(ve.ToString());
          }

          return sb.ToString();
        }
    }
}
