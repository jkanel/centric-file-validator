using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;

namespace Centric.DNA.File
{
    public enum ValidationErrorType
    {
      None = 0,
      File = 1,
      Row = 2,
      Column = 3
    }

    public enum ValidationErrorScope
    {
      None = 0,
      Information = 1,
      Warning = 2,
      Critical = 3
    }

    public class ValidationError
    {

        public int RowPosition;
        public string ColumnLabel;
        public int ColumnPosition;

        public ValidationErrorType ValidationErrorType = ValidationErrorType.None;
        public Exception Exception;
        public string Message;

        public ValidationErrorScope ValidationErrorScope = ValidationErrorScope.None;

        /// <summary>
        /// ValidationError constructor.
        /// </summary>
        public ValidationError(){ }
        
        /// <summary>
        /// ValidationError constructor that assumes the error has a type of 'File'.
        /// </summary>
        /// <param name="Message">Message describing the error.</param>
        /// <param name="ValidationErrorScope">Indicates whether the error is critical.  Defaults to 'true'.</param>
        public ValidationError(string Message, ValidationErrorScope ValidationErrorScope = ValidationErrorScope.Critical)
        {
            this.ValidationErrorType = ValidationErrorType.File;
            this.Message = Message;
            this.ValidationErrorScope = ValidationErrorScope;

        }
        
        /// <summary>
        /// ValidationError constructor that assumes the error has a type of 'Row'.
        /// </summary>
        /// <param name="RowPosition">Position of the file row that generated the error.</param>
        /// <param name="Message">Message describing the error.</param>
        /// <param name="ValidationErrorScope">Indicates whether the error is critical.  Defaults to 'true'.</param>
        public ValidationError(int RowPosition, string Message, ValidationErrorScope ValidationErrorScope = ValidationErrorScope.Critical)
        {
            this.ValidationErrorType = ValidationErrorType.Row;
            this.RowPosition = RowPosition;
            this.Message = Message;
            this.ValidationErrorScope = ValidationErrorScope;
        }

        /// <summary>
        /// ValidationError constructor that assumes the error has a type of 'Column'.
        /// </summary>
        /// <param name="ColumnPosition">Position of the column that generated the error.</param>
        /// <param name="ColumnLabel">Label of the column that generated the error.</param>
        /// <param name="RowPosition">Position of the file row that generated the error.</param>
        /// <param name="Message">Message describing the error.</param>
        /// <param name="ValidationErrorScope">Indicates whether the error is critical.  Defaults to 'true'.</param>
        public ValidationError(int RowPosition, int ColumnPosition, string ColumnLabel, string Message, ValidationErrorScope ValidationErrorScope = ValidationErrorScope.Critical)
        {

            this.ValidationErrorType = ValidationErrorType.Column;
            this.RowPosition = RowPosition;
            this.ColumnLabel = ColumnLabel;
            this.ColumnPosition = ColumnPosition;
            this.Message = Message;
            this.ValidationErrorScope = ValidationErrorScope;

        }

        public string ValidationErrorScopeName()
        {
          return Enum.GetName(this.ValidationErrorScope.GetType(), this.ValidationErrorScope).ToUpper();
        }

        public override string ToString()
        {
          return this.ValidationErrorScopeName() + " - " + this.Message;
        }

        public static int CriticalErrorCount(List<ValidationError> ValidationErrors)
        {
            return ValidationErrors.Count(item => item.ValidationErrorScope == ValidationErrorScope.Critical);
        }

        public static bool ContainsCriticalErrors(List<ValidationError> ValidationErrors)
        {
          return ValidationErrors.Exists(item => item.ValidationErrorScope == ValidationErrorScope.Critical);
        }


        public static void ExportToFile(List<ValidationError> ValidationErrors, string ExportFilePath, ValidationErrorScope ThresholdValidationErrorScope = ValidationErrorScope.Critical)
        {

          StreamWriter sw = new StreamWriter(ExportFilePath);

          // always show the file Validation Errors regardless of scope
          List<ValidationError> FileVEList = ValidationErrors.Where(i => i.ValidationErrorType == ValidationErrorType.File).ToList<ValidationError>();

          foreach (ValidationError ve in FileVEList)
          {
            sw.WriteLine(ve.ToString());
          }

          // get the list of row positions having a minimum scope in column or row
          List<int> CriticalRowPositionList = ValidationErrors
            .Where(i => (i.ValidationErrorType == ValidationErrorType.Row || i.ValidationErrorType == ValidationErrorType.Column) && (int)i.ValidationErrorScope >= (int)ThresholdValidationErrorScope)
            .OrderBy(i => i.RowPosition)
            .Select(i => i.RowPosition).ToList<int>();

          // show all rows whose position is in the critical row position list
          List<ValidationError> RowVEList = ValidationErrors
            .Where(i => CriticalRowPositionList.Contains(i.RowPosition))
            .OrderBy(i => i.RowPosition).ThenBy(i => i.ColumnPosition)
            .ToList<ValidationError>();

          int CurrentRowPosition = -999999;

          foreach (ValidationError ve in RowVEList)
          {
            if (ve.RowPosition != CurrentRowPosition)
            {
              sw.WriteLine(String.Empty);
              CurrentRowPosition = ve.RowPosition;
            }

            sw.WriteLine(ve.ToString());
          }

          sw.Close();
        }

        public static string ExportToString(List<ValidationError> ValidationErrors, ValidationErrorScope ThresholdValidationErrorScope = ValidationErrorScope.Critical)
        {
          StringBuilder sb = new StringBuilder();

          // always show the file Validation Errors regardless of scope
          List<ValidationError> FileVEList = ValidationErrors.Where(i => i.ValidationErrorType == ValidationErrorType.File).ToList<ValidationError>();
          
          foreach (ValidationError ve in FileVEList)
          {
            sb.AppendLine(ve.ToString());
          }

          // get the list of row positions having a minimum scope in column or row
          List<int> CriticalRowPositionList = ValidationErrors
            .Where(i => (i.ValidationErrorType == ValidationErrorType.Row || i.ValidationErrorType == ValidationErrorType.Column) && (int)i.ValidationErrorScope >= (int)ThresholdValidationErrorScope)
            .OrderBy(i => i.RowPosition)
            .Select(i => i.RowPosition).ToList<int>();

          // show all rows whose position is in the critical row position list
          List<ValidationError> RowVEList = ValidationErrors
            .Where(i => CriticalRowPositionList.Contains(i.RowPosition))
            .OrderBy(i => i.RowPosition).ThenBy(i => i.ColumnPosition)
            .ToList<ValidationError>();

          int CurrentRowPosition=-999999;

          foreach (ValidationError ve in RowVEList)
          {
            if (ve.RowPosition != CurrentRowPosition)
            {
              sb.AppendLine(String.Empty);
              CurrentRowPosition = ve.RowPosition;
            }

            sb.AppendLine(ve.ToString());
          }

          return sb.ToString();
        }
    }
}
