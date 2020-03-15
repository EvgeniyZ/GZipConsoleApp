using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using GZipConsoleApp.Entities;

namespace GZipConsoleApp.Workers
{
    public class Decompressor : GZipper
    {
        private readonly ProducerConsumerQueue<ByteBlock> _decompressingQueue;

        public Decompressor(int blockSize, string sourceFilename, string destinationFilename, CancellationToken cancellationToken) : base(blockSize, sourceFilename,
            destinationFilename, cancellationToken)
        {
            _decompressingQueue = new ProducerConsumerQueue<ByteBlock>(Environment.ProcessorCount, Decompress);
        }

        private void Decompress(ByteBlock byteBlock, int workerId)
        {
            // CancellationToken.ThrowIfCancellationRequested();
            // using (MemoryStream memoryStream = new MemoryStream(byteBlock.Buffer))
            // {
            //     using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            //     {
            //         //byte[] decompressedData = new byte[];
            //         gZipStream.Read(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
            //         
            //         ByteBlock block = new ByteBlock(decompressedData);
            //         _decompressingQueue.Enqueue(block);
            //     }
            // }
        }
    }
}