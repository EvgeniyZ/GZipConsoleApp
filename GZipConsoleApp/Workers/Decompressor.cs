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

        public bool Decompress(Action<Exception> onException)
        {
            try
            {
                var readerThread = new Thread(() =>
                {
                    try
                    {
                        Read();
                    }
                    catch (Exception e)
                    {
                        onException(e);
                    }
                });
                readerThread.Start();
                Thread.Sleep(1000); // wait for a reader thread to write something to _compressingQueue
            }
            finally
            {
                _decompressingQueue.Dispose();
                WritingQueue.Dispose();
            }

            return true;
        }

        private void Read()
        {
            using (var fileToBeDecompressed = new FileStream(SourceFilename, FileMode.Open))
            {
                while (fileToBeDecompressed.Position < fileToBeDecompressed.Length)
                {
                    CancellationToken.ThrowIfCancellationRequested();
                    byte[] lengthBuffer = new byte[8];
                    fileToBeDecompressed.Read(lengthBuffer, 0, lengthBuffer.Length);
                    int blockLength = BitConverter.ToInt32(lengthBuffer, 4);
                    byte[] compressedData = new byte[blockLength];
                    lengthBuffer.CopyTo(compressedData, 0);

                    fileToBeDecompressed.Read(compressedData, 8, blockLength - 8);
                    int dataSize = BitConverter.ToInt32(compressedData, blockLength - 4);
                    byte [] lastBuffer = new byte[dataSize];

                    ByteBlock block = new ByteBlock(lastBuffer);
                    _decompressingQueue.Enqueue(block);
                }
            }
        }

        private void Decompress(ByteBlock byteBlock, int workerId)
        {
            CancellationToken.ThrowIfCancellationRequested();
            using (MemoryStream memoryStream = new MemoryStream(byteBlock.Buffer))
            {
                using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
                    byte[] decompressedData = byteBlock.Buffer.ToArray();
                    ByteBlock block = new ByteBlock(decompressedData);
                    WritingQueue.Enqueue(block);
                }
            }
        }
    }
}