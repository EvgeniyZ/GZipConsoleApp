using System;
using System.Threading;

namespace GZipConsoleApp.Workers
{
    public abstract class GZipper
    {
        protected readonly string DestinationFilename;
        protected readonly int BlockSize;
        protected readonly string SourceFilename;
        protected readonly CancellationToken CancellationToken;
        protected readonly Action<string, Exception> OnException;

        protected GZipper(int blockSize, string sourceFilename, string destinationFilename, CancellationToken cancellationToken, 
            Action<string, Exception> onException)
        {
            BlockSize = blockSize;
            SourceFilename = sourceFilename;
            DestinationFilename = destinationFilename;
            CancellationToken = cancellationToken;
            OnException = onException;
        }
    }
}