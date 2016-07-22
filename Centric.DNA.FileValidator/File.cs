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
        private string _FileGuid = null;
        
        public delegate void RowValidationFunction(int RowPosition, string RowText, bool ContainsData, int RowErrorCount, string RowDisposition);

        public File() : this(null, new FileDefinition())
        {
        }

        /// <summary>
        /// File class constructor.
        /// </summary>
        /// <param name="FilePath">Full path of the file.</param>
        public File(String FilePath) : this(FilePath, new FileDefinition())
        {
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

            this._FileGuid = Guid.NewGuid().ToString().Replace("-", String.Empty).ToUpper();

            
        }

        /// <summary>
        /// Validate the File against the internal FileDefinition.
        /// </summary>
        /// <param name="AbortLimit">Limit of critical errors allowed before aborting validation.</param>
        public void Validate(int AbortLimit)
        {
          Validate(AbortLimit, null);
        }


        /// <summary>
        /// Validate the File against the internal FileDefinition.
        /// </summary>
        /// <param name="AbortLimit">Limit of critical errors allowed before aborting validation.</param>
        public void Validate(int AbortLimit, RowValidationFunction rvf)
        {

          StreamReader sr = new StreamReader(this.FilePath, this.FileDefinition.Encoding);

          this.ValidationErrors.Add(new ValidationError(string.Format("File Name: {0}", this.FileName), ValidationErrorSeverity.Information));
          this.ValidationErrors.Add(new ValidationError(string.Format("File Hash: {0}", this.FileHash), ValidationErrorSeverity.Information));

          int RowPosition = 0;
          string[] RowValues = null;
          string RowText;
          int DataRowCount = 0;

          while((RowText = sr.ReadLine()) != null)
          {
              RowPosition++;

              // got the next row if directed to skip row
              if (RowPosition <= this.FileDefinition.SkipRows)
              {
                continue;
              }
          
                    
              // consider if the row has now values
              if(RowText == null || RowText.Trim().Length == 0)
              {
                if(this.FileDefinition.IgnoreEmptyRows == true)
                { 
                  this.ValidationErrors.Add(new ValidationError(RowPosition, null,
                    string.Format("Row {0} contains no data.", RowPosition), ValidationErrorSeverity.Warning));
                }
                else
                {
                  this.ValidationErrors.Add(new ValidationError(RowPosition, null,
                    string.Format("Row {0} contains no data.", RowPosition), ValidationErrorSeverity.Critical));
                }

                // raise the event
                if (rvf != null)
                {
                  rvf(RowPosition, RowText, false, this.FileDefinition.IgnoreEmptyRows?0:1, null);
                }


                // go to the next row
                continue;
              }
              
              // convert row text to string array
              RowValues = this.FileDefinition.ConvertRowToValues(RowText);

              // determine the applicable row definition
              RowDefinition rd = this.FileDefinition.FindRowDefinition(RowValues);

              if(rd == null)
              {
                  this.ValidationErrors.Add(
                      new ValidationError(RowPosition, "Undetermined",
                          string.Format("The row disposition could not be determined")));

                  // raise the event
                  if (rvf != null)
                  {
                    rvf(RowPosition, RowText, true, 1, null);
                  }


              } else 
              {
                
                // this row has data
                DataRowCount++;
                rd.Validate(RowValues, RowPosition, this.ValidationErrors);

                // raise the event
                if (rvf != null)
                {
                  rvf(RowPosition, RowText, true, ValidationError.CriticalErrorCount(this.ValidationErrors, RowPosition), rd.DispositionColumnValue);
                }

              }

              // break out of validation if abort limit is hit
              if (AbortLimit > 0 && ValidationError.CriticalErrorCount(this.ValidationErrors) >= AbortLimit)
              {
                break;
              }
          }
          
          // close the stream reader
          sr.Close();

          // report the count of data rows (with disposition)
          // ADDITIONAL VALIDATION
          if(DataRowCount == 0)
          {         
              this.ValidationErrors.Add(new ValidationError(
                string.Format("No data rows were presented in this file.")
                ));

          } else
          {

            this.ValidationErrors.Add(new ValidationError(
              string.Format("{0} data row(s) have disposition in this file.", DataRowCount), ValidationErrorSeverity.Information
              ));
          
          }

          // report the number of validation errors
          // ADDITIONAL VALIDATION
          int CriticalErrorCount = ValidationError.CriticalErrorCount(this.ValidationErrors);

          this.ValidationErrors.Add(new ValidationError(
            string.Format("{0} critical errors were identified in this file.", CriticalErrorCount), ValidationErrorSeverity.Information
            ));


          // report if the abort limit was reached
          // ADDITIONAL VALIDATION
            if (CriticalErrorCount > 0 && CriticalErrorCount >= AbortLimit)
          {
            
            this.ValidationErrors.Add(new ValidationError(
              string.Format("Validation was halted after {0} critical errors.", 
                (CriticalErrorCount>AbortLimit)?CriticalErrorCount:AbortLimit), ValidationErrorSeverity.Information
              ));

          }
          
        }


        /// <summary>
        /// File name component of the file specified in the FilePath.
        /// </summary>
        /// <returns>Returns the file name with a path.</returns>
        public string FileName
        {
          get {
            return System.IO.Path.GetFileName(this.FilePath);
          }
        }

        /// <summary>
        /// File name component of the file specified in the FilePath.
        /// </summary>
        /// <returns>Returns the file name with a path.</returns>
        public string FileGuid
        {
          get
          {
            return this._FileGuid;
          }
        } 
   

        /// <summary>
        /// Directory component of the file specified in the FilePath.
        /// </summary>
        /// <returns></returns>
        public string DirectoryPath
        {
          get
          {
            return System.IO.Path.GetDirectoryName(this.FilePath);
          }
        }

        /// <summary>
        /// File extension of the file specified in the FilePath.
        /// </summary>
        /// <returns></returns>
        public string FileExtension
        {
          get
          {
            return System.IO.Path.GetExtension(this.FilePath);
          }
        }


        /// <summary>
        /// File name of the file specified in the FilePath excluding the file extension suffix.
        /// </summary>
        /// <returns></returns>
        public string FileNameWithoutExtension
        {
          get
          {
            return System.IO.Path.GetFileNameWithoutExtension(this.FilePath);
          }
        }

        /// <summary>
        /// Date and time at which the file was created.
        /// </summary>
        /// <returns></returns>
        public DateTime CreateTimestamp
        {
          get
          {
            return System.IO.File.GetCreationTime(this.FilePath);
          }
        }

        /// <summary>
        /// Date and time at which the file was last modified.
        /// </summary>
        /// <returns></returns>
        public DateTime LastModifiedTimestamp
        {
          get
          {
            return System.IO.File.GetLastWriteTime(this.FilePath);
          }
        }

        /// <summary>
        /// 256-bit (64 character) hash of the file specified in the FilePath.
        /// </summary>
        /// <returns></returns>
        public string FileHash
        {
          get {

            FileStream fs = System.IO.File.OpenRead(this.FilePath);

            MD5 md5hasher = MD5.Create();
            byte[] hash = md5hasher.ComputeHash(fs);

            md5hasher.Clear();
            fs.Close();

            return BitConverter.ToString(hash).Replace("-",String.Empty);
          }
        }

        /// <summary>
        /// Unique file name comprised the original file name and the file hash. 
        /// </summary>
        /// <returns></returns>
        public string ArchiveFileName
        {
          get
          {
            return String.Concat(
                new string[] { this.FileNameWithoutExtension, "_", this._FileGuid, this.FileExtension });
          }
        }

        /// <summary>
        /// Returns the fully qualified archive file path give an archive folder path.
        /// </summary>
        /// <param name="ArchiveFolderPath">Folder path in which the archive file is located</param>
        /// <returns>Fully qualified archive file path.</returns>
        public string ArchiveFilePath(string ArchiveFolderPath)
        {
           return File.GetFilePath(ArchiveFolderPath, this.ArchiveFileName);
        }


        /// <summary>
        /// Determines the branch (section of the folder path) within the trunk folder path and excluding the file name.
        /// </summary>
        /// <param name="TrunkFolderPath">The folder path below which the the folders comprise the branch.</param>
        /// <returns>Branch folder path below the trunk folder path and excluding the file name.</returns>
        public string FolderBranch(string TrunkFolderPath)
        {
          return File.GetFolderBranch(this.FilePath, TrunkFolderPath);
        }

        public static string GetFilePath(string FolderPath, string FileName)
        {
          return System.IO.Path.Combine(FolderPath, FileName);
        }

        /// <summary>
        /// Determines the branch (section of the folder path) within the trunk folder path and excluding the file name.
        /// </summary>
        /// <param name="FilePath">The file who branch in the trunk folder path is being determined.</param>  
        /// <param name="TrunkFolderPath">The folder path below which the the folders comprise the branch.</param>
        /// <returns>Branch folder path below the trunk folder path and excluding the file name.</returns>/// 
        public static string GetFolderBranch(string FilePath, string TrunkFolderPath)
        {
          // determine the file folder path
          string QualifiedFileFolderPath = System.IO.Path.GetDirectoryName(FilePath); 
                    
          if(TrunkFolderPath == null || TrunkFolderPath.Trim().Length == 0)
          {
            return QualifiedFileFolderPath;
          }

          // determine if the last character of the trunk folder is not a path delimiter
          if(TrunkFolderPath.LastIndexOf("\\")!=(TrunkFolderPath.Length - 1))
          {
            // append the path delimiter
            // necessary for correct processing of GetDirectoryName function
            TrunkFolderPath += "\\";
          }

          // determine the trunk folder path
          string QualifiedTrunkFolderPath = System.IO.Path.GetDirectoryName(TrunkFolderPath);

          if(QualifiedFileFolderPath.Equals(QualifiedTrunkFolderPath))
          {
            return String.Empty;
          }
          if(QualifiedTrunkFolderPath.Length > QualifiedFileFolderPath.Length)
          {
            throw new InvalidDataException("The trunk folder path exceeds the length of the file folder path.");

          }
          else if (!QualifiedFileFolderPath.Substring(0, QualifiedTrunkFolderPath.Length).Equals(QualifiedTrunkFolderPath))
          {
            throw new InvalidDataException("The trunk folder path does not exist in the file folder path.");

          } else
          {
            
            string DerivedFolderBranch = QualifiedFileFolderPath.Substring(QualifiedTrunkFolderPath.Length);

            // determine if the first character is a folder delimiter
            if (DerivedFolderBranch.IndexOf("\\") == 0)
            {
              // strip the first character
              DerivedFolderBranch = DerivedFolderBranch.Substring(1);
            }

            return DerivedFolderBranch;

          }
        }
    }
}
