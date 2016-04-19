using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Centric.DNA.File;
using Centric.DNA.File.Definition;


namespace Centric.DNA.File.Test
{
    class Program
    {
        static void Main(string[] args)
        {

          string BasePath = @"C:\Working\GitHub\centric-file-validator\Centric.DNA.FileValidator\Sample";

          Centric.DNA.File.File f = new File(BasePath + @"\" + "rpm_test.txt");
          f.FileDefinition = FileDefinitionLoader.LoadFromXmlFile(BasePath + @"\" + "rpm.xml");

          File.RowValidationFunction rvf = RowValidation;
       
          f.Validate(100, rvf);

          //File.EmitRowFunction emr = EmitRow;
          //f.IterateEmitRow(EmitRow);

          Console.WriteLine(f.ArchiveFileName);
          Console.WriteLine(f.ArchiveFilePath(@"C:\Temp"));
          Console.WriteLine(f.FolderBranch(@"C:\Working\GitHub\centric-file-validator\Centric.DNA.FileValidator"));


          bool ContainsCriticalErrors = ValidationError.ContainsCriticalErrors(f.ValidationErrors);

          ValidationError.ExportToFile(f.ValidationErrors, BasePath + @"\" + "rpm.log");

          /*
           * 
          if (ContainsCriticalErrors)
          {
            ValidationError.ExportToFile(f.ValidationErrors, BasePath + @"\" + "rpm.log");
            ValidationError.ExportToTabFile(f.ValidationErrors, BasePath + @"\" + "rpm_tab.txt");
          }
           *
           */

        }

        public static void RowValidation(int RowPosition, string RowText, bool ContainsData, int RowErrorCount, string RowDisposition)
        {
          Console.WriteLine(RowDisposition);
          Console.WriteLine(RowText);
        }
    }
}
