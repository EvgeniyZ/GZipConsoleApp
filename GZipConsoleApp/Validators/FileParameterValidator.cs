using System.IO;

namespace GZipConsoleApp.Validators
{
    public class FileParameterValidator
    {
        public (bool isValid, string errorMessage) Validate(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return (false, "Empty filename, please specify a not empty filename");
            }
            var fileInfo = new FileInfo(fileName);
            
        }
    }
}