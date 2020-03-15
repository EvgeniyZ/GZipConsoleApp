using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GZipConsoleApp.Entities;

namespace GZipConsoleApp.Workers
{
    public class Compressor : GZipper
    {
        private readonly ProducerConsumerQueue<ByteBlock> _compressingQueue;

        public Compressor(int blockSize, string sourceFilename, string destinationFilename, CancellationToken cancellationToken) : base(blockSize, sourceFilename,
            destinationFilename, cancellationToken)
        {
            _compressingQueue = new ProducerConsumerQueue<ByteBlock>(Environment.ProcessorCount, Compress);
        }

        public bool Compress(Action<Exception> onException)
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
                _compressingQueue.Dispose();
                WritingQueue.Dispose();
            }

            return true;
        }

        private void Read()
        {
            var currentBlockId = 0;
            using (var fileToBeCompressed = new FileStream(SourceFilename, FileMode.Open))
            {
                while (fileToBeCompressed.Position < fileToBeCompressed.Length)
                {
                    CancellationToken.ThrowIfCancellationRequested();
                    int bytesRead;
                    if (fileToBeCompressed.Length - fileToBeCompressed.Position <= BlockSize)
                    {
                        bytesRead = (int) (fileToBeCompressed.Length - fileToBeCompressed.Position);
                    }
                    else
                    {
                        bytesRead = BlockSize;
                    }

                    var readBuffer = new byte[bytesRead];
                    fileToBeCompressed.Read(readBuffer, 0, bytesRead);
                    _compressingQueue.Enqueue(new ByteBlock(currentBlockId++, readBuffer));
                }
            }
        }

        private void Compress(ByteBlock byteBlock, int workerId)
        {
            CancellationToken.ThrowIfCancellationRequested();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    gZipStream.Write(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
                }

                ByteBlock compressedByteBlock = new ByteBlock(byteBlock.Id, memoryStream.ToArray());
                WritingQueue.Enqueue(compressedByteBlock);
            }
        }
    }
}