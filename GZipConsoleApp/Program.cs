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
        private static readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        static void Main(string[] args)
        {
            args = new[] {"compress", @"C:\gzip-tests\Rasaad.bif", @"C:\gzip-tests\result"};
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

            switch (command)
            {
                case Command.Compress:
                    var compressor = new Compressor(BlockSize, sourceFilename, destinationFilename, _cancellationTokenSource.Token);
                    try
                    {
                        bool result = compressor.Compress();
                        Console.WriteLine(result ? "Compression is successfully finished" : "Compression failed with an unknown error");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                    break;
            }
        }

        static void CancelKeyPress(object sender, ConsoleCancelEventArgs args)
        {
            if (args.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                Console.WriteLine("Cancelling zipping");
                _cancellationTokenSource.Cancel();
                args.Cancel = true;
            }
        }
    }
}