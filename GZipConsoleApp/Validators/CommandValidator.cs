using System;
using System.Collections;

namespace GZipConsoleApp.Validators
{
    public class CommandValidator
    {
        private const string Decompress = "decompress";
        private const string Compress = "compress";

        public (bool isValid, string errorMessage) Validate(string command)
        {
            if (command.Equals(Decompress, StringComparison.OrdinalIgnoreCase) || command.Equals(Compress, StringComparison.OrdinalIgnoreCase))
            {
                return (true, string.Empty);
            }

            return (false, $"Invalid 1st parameter, only {Compress}/{Decompress} are valid. Please, use one of them");
        }
    }
}