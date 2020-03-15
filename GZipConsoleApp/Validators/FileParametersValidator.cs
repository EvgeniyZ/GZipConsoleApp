using System.IO;
using GZipConsoleApp.Entities;

namespace GZipConsoleApp.Validators
{
    internal class FileParametersValidator
    {
        private const string Source = "source";
        private const string Destination = "destination";

        public (bool isValid, string errorMessage) Validate(string sourceFilename, string destinationFilename)
        {
            if (string.IsNullOrEmpty(sourceFilename))
            {
                return (false, ConstructEmptyFilenameMessage(Source));
            }

            if (string.IsNullOrEmpty(destinationFilename))
            {
                return (false, ConstructEmptyFilenameMessage(Destination));
            }

            var sourceFileInfo = new FileInfo(sourceFilename);
            var destinationFileInfo = new FileInfo(destinationFilename + ZipSettings.ZipExtension);
            if (sourceFileInfo == destinationFileInfo)
            {
                return (false, $"{Source} and {Destination} should not be the same");
            }

            if (!sourceFileInfo.Exists)
            {
                return (false, $"{Source} file name does not exists, please check ${Source} file name");
            }

            if (destinationFileInfo.Exists)
            {
                return (false, $"{destinationFilename + ZipSettings.ZipExtension} already exists");
            }

            return (true, string.Empty);
        }

        private static string ConstructEmptyFilenameMessage(string fileParameter)
        {
            return $"Empty {fileParameter} filename, please specify a not empty {fileParameter} filename";
        }
    }
}