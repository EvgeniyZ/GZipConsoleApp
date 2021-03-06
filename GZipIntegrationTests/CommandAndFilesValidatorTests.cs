﻿using GZipConsoleApp.Entities;
using GZipConsoleApp.Validators;
using Xunit;

namespace GZipIntegrationTests
{
    public class CommandAndFilesValidatorTests
    {
        [Theory(Skip = "Integration test heavy rely on a existing paths and folders")]
        [InlineData(Command.Compress, @"C:\gzip-tests\test.txt", @"C:\gzip-tests\result")]
        [InlineData(Command.Compress, "test.txt", "result")]
        [InlineData(Command.Decompress, @"C:\gzip-tests\test.gz", @"C:\gzip-tests\result")]
        public void Validate_ShouldBeValid(string command, string sourceFilename, string destinationFilename)
        {
            var commandAndFilesValidator = new CommandAndFilesValidator();

            var (isValid, errorMessage) = commandAndFilesValidator.Validate(command, sourceFilename, destinationFilename);

            Assert.True(isValid);
            Assert.Empty(errorMessage);
        }

        [Theory(Skip = "Integration test heavy rely on a existing paths and folders")]
        [InlineData(Command.Compress, @"C:\gzip-tests\test.gz", @"C:\gzip-tests\result")]
        public void Validate_AlreadyCompressedFiles_ShouldBeInvalid(string command, string sourceFilename, string destinationFilename)
        {
            var commandAndFilesValidator = new CommandAndFilesValidator();

            var (isValid, errorMessage) = commandAndFilesValidator.Validate(command, sourceFilename, destinationFilename);

            Assert.False(isValid);
            Assert.NotEmpty(errorMessage);
        }

        [Theory(Skip = "Integration test heavy rely on a existing paths and folders")]
        [InlineData(Command.Decompress, @"C:\gzip-tests\test.txt", @"C:\gzip-tests\result")]
        public void Validate_DecompressButNotGzipped_ShouldBeInvalid(string command, string sourceFilename, string destinationFilename)
        {
            var commandAndFilesValidator = new CommandAndFilesValidator();

            var (isValid, errorMessage) = commandAndFilesValidator.Validate(command, sourceFilename, destinationFilename);

            Assert.False(isValid);
            Assert.NotEmpty(errorMessage);
        }
    }
}