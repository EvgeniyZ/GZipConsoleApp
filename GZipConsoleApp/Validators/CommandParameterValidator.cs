using System;
using GZipConsoleApp.Entities;

namespace GZipConsoleApp.Validators
{
    internal class CommandParameterValidator
    {
        public (bool isValid, string errorMessage) Validate(string command)
        {
            if (command.Equals(Command.Compress, StringComparison.OrdinalIgnoreCase) || command.Equals(Command.Decompress, StringComparison.OrdinalIgnoreCase))
            {
                return (true, string.Empty);
            }

            return (false,
                $"Invalid 1st parameter, only {Command.Compress}/{Command.Decompress} are valid. Please, {Command.Compress} to compress the file and {Command.Decompress} to decompress zipped file");
        }
    }
}