using System;
using System.Runtime.CompilerServices;
using System.Threading;
using GZipConsoleApp.Entities;
using GZipConsoleApp.Validators;
using GZipConsoleApp.Workers;

[assembly: InternalsVisibleTo("GZipUnitTests")]
[assembly: InternalsVisibleTo("GZipIntegrationTests")]

namespace GZipConsoleApp
{
    class Program
    {
        private const int BlockSize = 1000000;
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private const int SuccessCode = 0;
        private const int ErrorCode = 1;
        private static bool ExceptionOccured;

        static void Main(string[] args)
        {
            //args = new[] {"compress", @"C:\gzip-tests\test.pdf", @"C:\gzip-tests\result"};
            //args = new[] {"compress", @"C:\films\The.Irishman.2019.WEBRip.720p.mkv", @"C:\gzip-tests\result"};
            //args = new[] {"compress", @"C:\gzip-tests\test.txt", @"C:\gzip-tests\result"};
            args = new[] {"compress", @"C:\gzip-tests\large-test.zip", @"C:\gzip-tests\result"};
            //args = new[] {"decompress", @"C:\gzip-tests\result.gz", @"C:\gzip-tests\decompressed\test1.txt"};
            //args = new[] {"decompress", @"C:\gzip-tests\result.gz", @"C:\gzip-tests\decompressed\test1.pdf"};
            if (args.Length != 3)
            {
                Console.WriteLine(
                    $"GZip Console accepts only 3 arguments.\n1st is a {Command.Compress}\\{Command.Decompress}.\n2nd - source filename.\n3nd - destination filename.");
                return;
            }

            Console.CancelKeyPress += CancelKeyPress;
            var command = args[0];
            var sourceFilename = args[1];
            var destinationFilename = args[2];
            var (isValid, errorMessage) = new CommandAndFilesValidator().Validate(command, sourceFilename, destinationFilename);
            if (!isValid)
            {
                Console.WriteLine(ErrorCode);
                Console.WriteLine(errorMessage);
                return;
            }

            CancellationTokenSource.Token.Register(() => { DeleteFileOnAbortion(destinationFilename); });

            try
            {
                bool result = false;
                switch (command)
                {
                    case Command.Compress:
                        var compressor = new Compressor(BlockSize, sourceFilename, destinationFilename, CancellationTokenSource.Token, OnException);
                        result = compressor.Compress();
                        break;
                    case Command.Decompress:
                        var decompressor = new Decompressor(BlockSize, sourceFilename, destinationFilename, CancellationTokenSource.Token, OnException);
                        result = decompressor.Decompress();
                        break;
                }

                if (ExceptionOccured)
                {
                    Cancel();
                }
                else
                {
                    Console.WriteLine(result ? SuccessCode : ErrorCode);
                }
            }
            catch (Exception e)
            {
                ShowWarningMessageOnException(command, e);
                Cancel();
                throw;
            }
        }

        private static void ShowWarningMessageOnException(string command, Exception e)
        {
            if (e is OperationCanceledException)
            {
                return;
            }

            Console.WriteLine($"{command} is aborted due to exception, please contact a developer and send him an exception below.");
            Console.WriteLine(e);
        }

        private static void Cancel()
        {
            Console.WriteLine(ErrorCode);
            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();
        }

        private static void DeleteFileOnAbortion(string destinationFilename)
        {
            try
            {
                System.IO.File.Delete(destinationFilename + ZipSettings.ZipExtension);
            }
            catch (Exception exception)
            {
                Console.WriteLine(
                    $"Unsuccessfully deleting {destinationFilename} due to an exception below. Please contact a developer and send him the exception");
                Console.WriteLine(exception);
                throw;
            }
        }

        private static void OnException(string command, Exception ex)
        {
            ShowWarningMessageOnException(command, ex);
            ExceptionOccured = true;
        }

        private static void CancelKeyPress(object sender, ConsoleCancelEventArgs args)
        {
            if (args.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                Cancel();
                args.Cancel = true;
            }
        }
    }
}