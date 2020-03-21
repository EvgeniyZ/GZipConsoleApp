using System;
using System.Threading;

namespace GZipConsoleApp.Workers
{
    public abstract class GZipper
    {
        protected readonly string DestinationFilename;
        protected readonly string Command;
        protected readonly int BlockSize;
        protected readonly string SourceFilename;
        protected readonly CancellationToken CancellationToken;
        protected readonly Action<string, Exception> OnException;
        protected volatile bool ReadCompleted;

        protected GZipper(int blockSize, string sourceFilename, string destinationFilename, string command, CancellationToken cancellationToken,
            Action<string, Exception> onException)
        {
            BlockSize = blockSize;
            SourceFilename = sourceFilename;
            DestinationFilename = destinationFilename;
            Command = command;
            CancellationToken = cancellationToken;
            OnException = onException;
        }

        protected void StartRead()
        {
            var readerThread = new Thread(() =>
            {
                try
                {
                    Read();
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception e)
                {
                    OnException(Command, e);
                }
            });
            readerThread.Start();
        }

        protected abstract void Read();
    }
}