using System.Threading;

namespace GZipConsoleApp.Workers
{
    public abstract class GZipper
    {
        protected readonly string DestinationFilename;
        protected readonly int BlockSize;
        protected readonly string SourceFilename;
        protected readonly CancellationToken CancellationToken;

        protected GZipper(int blockSize, string sourceFilename, string destinationFilename, CancellationToken cancellationToken)
        {
            BlockSize = blockSize;
            SourceFilename = sourceFilename;
            DestinationFilename = destinationFilename;
            CancellationToken = cancellationToken;
        }
    }
}