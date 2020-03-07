using System;
using System.IO;
using GZipConsoleApp.Entities;

namespace GZipConsoleApp.Validators
{
    public class CommandAndFilesValidator
    {
        private readonly CommandParameterValidator _commandParameterValidator;
        private readonly FileParametersValidator _fileParametersValidator;

        public CommandAndFilesValidator()
        {
            _commandParameterValidator = new CommandParameterValidator();
            _fileParametersValidator = new FileParametersValidator();
        }

        public (bool isValid, string errorMessage) Validate(string command, string sourceFilename, string destinationFilename)
        {
            var commandValidationResult = _commandParameterValidator.Validate(command);
            if (!commandValidationResult.isValid)
            {
                return commandValidationResult;
            }

            var fileValidationResult = _fileParametersValidator.Validate(sourceFilename, destinationFilename);
            if (!fileValidationResult.isValid)
            {
                return fileValidationResult;
            }

            FileInfo sourceFileInfo = new FileInfo(sourceFilename);
            if (sourceFileInfo.Extension == ZipSettings.ZipExtension && command.Equals(Command.Compress, StringComparison.OrdinalIgnoreCase))
            {
                return (false, $"{nameof(sourceFilename)} already compressed");
            }

            if (sourceFileInfo.Extension != ZipSettings.ZipExtension && command.Equals(Command.Decompress, StringComparison.OrdinalIgnoreCase))
            {
                return (false, $"{nameof(sourceFilename)} should have {ZipSettings.ZipExtension} extension, can't decompress unknown file");
            }

            return (true, string.Empty);
        }
    }
}