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

          string BasePath = @"C:\Working\Visual Studio 2013\Projects\Centric File Validator\Centric.DNA.FileValidator\Sample";

          Centric.DNA.File.File f = new File(BasePath + @"\" + "SampleData1.txt");
          f.FileDefinition = FileDefinitionLoader.LoadFromXmlFile(BasePath + @"\" + "SampleFileDefinition.xml");

          f.Validate(20);

          Console.WriteLine(f.GenerateArchiveFileName());

          bool ContainsCriticalErrors = ValidationError.ContainsCriticalErrors(f.ValidationErrors);

          if (ContainsCriticalErrors)
          {
            ValidationError.ExportToFile(f.ValidationErrors, BasePath + @"\" + "SampleData1.log");
          }

        }
    }
}
