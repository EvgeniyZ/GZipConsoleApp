using System;
using System.IO;
using System.Threading;
using GZipConsoleApp.Entities;

namespace GZipConsoleApp.Workers
{
    public abstract class GZipper
    {
        private readonly string _destinationFilename;
        protected readonly int BlockSize;
        protected readonly string SourceFilename;
        protected readonly CancellationToken CancellationToken;
        protected readonly ProducerConsumerQueue<ByteBlock> WritingQueue;

        protected GZipper(int blockSize, string sourceFilename, string destinationFilename, CancellationToken cancellationToken)
        {
            BlockSize = blockSize;
            SourceFilename = sourceFilename;
            _destinationFilename = destinationFilename;
            CancellationToken = cancellationToken;
            WritingQueue = new ProducerConsumerQueue<ByteBlock>(1, Write);
        }

        private void Write(ByteBlock byteBlock, int workerId)
        {
            CancellationToken.ThrowIfCancellationRequested();
            using (var fileCompressed = new FileStream(_destinationFilename + ".gz", FileMode.Append))
            {
                //BitConverter.GetBytes(byteBlock.Buffer.Length).CopyTo(byteBlock.Buffer, 4);
                var idAsByteArray = BitConverter.GetBytes(byteBlock.Id);
                var bufferLengthAsByteArray = BitConverter.GetBytes(byteBlock.Buffer.Length);
                fileCompressed.Write(idAsByteArray, 0, idAsByteArray.Length);
                fileCompressed.Write(bufferLengthAsByteArray, 0, bufferLengthAsByteArray.Length);
                fileCompressed.Write(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
            }
        }
    }
}