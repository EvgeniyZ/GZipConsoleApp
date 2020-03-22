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
            if (args.Length != 3)
            {
                Console.WriteLine(
                    $"GZip Console accepts only 3 arguments.\n1st is a {Command.Compress}\\{Command.Decompress}.\n2nd - source filename.\n3nd - destination filename.");
                return;
            }

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

            Console.CancelKeyPress += CancelKeyPress;
            CancellationTokenSource.Token.Register(() => { DeleteFileOnAbortion(destinationFilename); });

            try
            {
                bool result = false;
                Console.WriteLine($"{command} started.");
                Console.WriteLine("Please, use CTRL+C if you want to cancel an operation.");
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
            }
        }

        private static void ShowWarningMessageOnException(string command, Exception e)
        {
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
            var filenameWithZipExtension = ZipSettings.GetFilenameWithZipExtension(destinationFilename);
            Console.WriteLine($"Operation was canceled - deleting {filenameWithZipExtension}.");
            try
            {
                System.IO.File.Delete(filenameWithZipExtension);
                Console.WriteLine($"{filenameWithZipExtension} is deleted.");
            }
            catch (Exception exception)
            {
                Console.WriteLine(
                    $"Failed deleting {filenameWithZipExtension} due to an exception below. Please contact a developer and send him the exception.");
                Console.WriteLine(exception);
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