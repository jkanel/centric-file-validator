using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace Centric.DNA.File
{
    public class File
    {
        public String FilePath;
        public FileDefinition FileDefinition;
        public List<ValidationError> ValidationErrors = new List<ValidationError>();
        public const string DEFAULT_FILE_EXTENSION = "txt";

        /// <summary>
        /// File class constructor.
        /// </summary>
        /// <param name="FilePath">Full path of the file.</param>
        public File(String FilePath)
        {
            this.FilePath = FilePath;
            this.FileDefinition = new FileDefinition();
        }

        /// <summary>
        /// File class constructor.
        /// </summary>
        /// <param name="FilePath">Full path of the file.</param>
        /// <param name="FileDefinition">FileDefinition used to validate the File.</param>
        public File(String FilePath, FileDefinition FileDefinition)
        {
            this.FilePath = FilePath;
            this.FileDefinition = FileDefinition;
        }

        /// <summary>
        /// Validate the File against the internal FileDefinition.
        /// </summary>
        public void Validate(int CriticalErrorCountAbortLimit)
        {

            // informational file validation error
          this.ValidationErrors.Add(
            new ValidationError(string.Format("File \"{0}\"", this.FileName()), ValidationErrorScope.Information)
          );

          //loop through file rows
          StreamReader sr = new StreamReader(this.FilePath);

          int RowPosition = 0;
          string[] RowValues = null;
          string RowText;
          int DataRowCount = 0;
            
          while(!sr.EndOfStream)
          {
              RowPosition++;

              if (RowPosition > this.FileDefinition.SkipRows)
              {
                    
                  RowText = sr.ReadLine();
                    
                  if(RowText == null || RowText.Trim().Length == 0)
                  {
                        this.ValidationErrors.Add(
                          new ValidationError(RowPosition,
                              string.Format("Row {0} - Contains no data.", RowPosition), ValidationErrorScope.Warning));

                        // go to the next row
                        continue;
                  }

                  RowValues = this.FileDefinition.ConvertRowToValues(RowText);

                  // determine the applicable row definition
                  RowDefinition rd = this.FileDefinition.FindRowDefinition(RowValues);

                  if(rd == null)
                  {
                      this.ValidationErrors.Add(
                          new ValidationError(RowPosition,
                              string.Format("Row {0} - Disposition could not be determined", RowPosition)));

                  } else 
                  {
                      DataRowCount++;
                      rd.Validate(RowValues, RowPosition, this.ValidationErrors);
                  }

                  if(ValidationError.CriticalErrorCount(this.ValidationErrors) >= CriticalErrorCountAbortLimit)
                  {
                    break;
                  }
              }

          }

          if(DataRowCount == 0)
          {         
              this.ValidationErrors.Add(new ValidationError(
                string.Format("File \"{0}\" - No data rows were presented in this file.", this.FileName())
                ));
          } else
          {

            this.ValidationErrors.Add(new ValidationError(
              string.Format("File \"{0}\" - {1} data rows were disposed in this file.", this.FileName(), DataRowCount), ValidationErrorScope.Information
              ));
          
          }

          // report the number of validation errors
          int CriticalErrorCount = ValidationError.CriticalErrorCount(this.ValidationErrors);

            this.ValidationErrors.Add(new ValidationError(
              string.Format("File \"{0}\" - {1} critical errors were identified in this file.", this.FileName(), CriticalErrorCount), ValidationErrorScope.Information
              ));


          // report if the abort limit was reached
          if(CriticalErrorCount >= CriticalErrorCountAbortLimit)
          {
            
            this.ValidationErrors.Add(new ValidationError(
              string.Format("File \"{0}\" - Validation was aborted after {1} critical errors.", 
                this.FileName(), (CriticalErrorCount>CriticalErrorCountAbortLimit)?CriticalErrorCount:CriticalErrorCountAbortLimit), ValidationErrorScope.Information
              ));

          }
          
        }
        
        /// <summary>
        /// File name component of the file specified in the FilePath.
        /// </summary>
        /// <returns></returns>
        public string FileName()
        {
            return System.IO.Path.GetFileName(this.FilePath);
        }

        /// <summary>
        /// Directory component of the file specified in the FilePath.
        /// </summary>
        /// <returns></returns>
        public string DirectoryPath()
        {
            return System.IO.Path.GetDirectoryName(this.FilePath);
        }

        /// <summary>
        /// File extension of the file specified in the FilePath.
        /// </summary>
        /// <returns></returns>
        public string FileExtension()
        {
            return System.IO.Path.GetExtension(this.FilePath);
        }


        /// <summary>
        /// File name of the file specified in the FilePath excluding the file extension suffix.
        /// </summary>
        /// <returns></returns>
        public string FileNameWithoutExtension()
        {
            return System.IO.Path.GetFileNameWithoutExtension(this.FilePath);
        }

        /// <summary>
        /// Date and time at which the file was created.
        /// </summary>
        /// <returns></returns>
        public DateTime CreateTimestamp()
        {
            return System.IO.File.GetCreationTime(this.FilePath);
        }


        /// <summary>
        /// 256-bit (64 character) hash of the file specified in the FilePath.
        /// </summary>
        /// <returns></returns>
        public string FileHash()
        {
            FileStream fs = System.IO.File.OpenRead(this.FilePath);

            SHA256Managed sha = new SHA256Managed();
            byte[] hash = sha.ComputeHash(fs);

            return BitConverter.ToString(hash).ToUpper();
        }

        /// <summary>
        /// Archive file name of the file specified in the FilePath made unqiue through incorporation of the file hash. 
        /// </summary>
        /// <returns></returns>
        public string GenerateArchiveFileName()
        {
            return String.Concat(
                new string[]{this.FileNameWithoutExtension(), "_", this.FileHash(), ".", File.DEFAULT_FILE_EXTENSION});
        }
    }
}
