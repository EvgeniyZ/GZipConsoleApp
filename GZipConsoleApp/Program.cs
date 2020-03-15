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
            args = new[] {"compress", @"C:\films\The.Irishman.2019.WEBRip.720p.mkv", @"C:\gzip-tests\result"};
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
                Console.WriteLine(errorMessage);
                return;
            }

            try
            {
                bool result = false;
                switch (command)
                {
                    case Command.Compress:
                        var compressor = new Compressor(BlockSize, sourceFilename, destinationFilename, CancellationTokenSource.Token);
                        result = compressor.Compress(OnException);

                        break;
                }

                if (ExceptionOccured)
                {
                    Console.WriteLine(ErrorCode);
                }
                else
                {
                    Console.WriteLine(result ? SuccessCode : ErrorCode);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{command} is aborted due to exception, please contact a developer and send him an exception below.");
                Console.WriteLine(e);
                try
                {
                    System.IO.File.Delete(destinationFilename);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(
                        $"Unsuccessfully deleting {destinationFilename} due to an exception below. Please contact a developer and send him the exception");
                    Console.WriteLine(exception);
                    throw;
                }

                throw;
            }
        }

        static void OnException(Exception ex)
        {
            Console.WriteLine(ex);
            ExceptionOccured = true;
        }

        static void CancelKeyPress(object sender, ConsoleCancelEventArgs args)
        {
            if (args.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                Console.WriteLine("Cancelling zipping");
                CancellationTokenSource.Cancel();
                CancellationTokenSource.Dispose();
                args.Cancel = true;
            }
        }
    }
}