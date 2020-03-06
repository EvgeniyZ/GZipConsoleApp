using System.IO;

namespace GZipConsoleApp.Validators
{
    public class FileParameterValidator
    {
        public (bool isValid, string errorMessage) Validate(string sourceFilename, string destinationFilename)
        {
            if (string.IsNullOrEmpty(sourceFilename))
            {
                return (false, ConstructEmptyFilenameMessage(nameof(sourceFilename)));
            }

            if (string.IsNullOrEmpty(destinationFilename))
            {
                return (false, ConstructEmptyFilenameMessage(nameof(destinationFilename)));
            }

            var sourceFileInfo = new FileInfo(sourceFilename);
            var destinationFileInfo = new FileInfo(destinationFilename);
            if (sourceFileInfo == destinationFileInfo)
            {
                return (false, $"{nameof(sourceFilename)} and {nameof(destinationFilename)} should not be equal");
            }

            if (!sourceFileInfo.Exists)
            {
                return (false, $"{nameof(sourceFilename)} does not exists, please check ${nameof(sourceFilename)}");
            }

            if (destinationFileInfo.Directory is null)
            {
                return (false, $"{destinationFilename} directory path does not exists");
            }

            return (true, string.Empty);
        }

        private static string ConstructEmptyFilenameMessage(string fileParameter)
        {
            return $"Empty {fileParameter} filename, please specify a not empty {fileParameter} filename";
        }
    }
}