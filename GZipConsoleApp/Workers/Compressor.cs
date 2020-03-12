using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GZipConsoleApp.Entities;

namespace GZipConsoleApp.Workers
{
    public class Compressor
    {
        private readonly int _blockSize;
        private readonly string _sourceFilename;
        private readonly string _destinationFilename;
        private readonly CancellationToken _cancellationToken;
        private readonly ProducerConsumerQueue<ByteBlock> _compressingQueue;
        private readonly ProducerConsumerQueue<ByteBlock> _writingQueue;
        private readonly ManualResetEvent[] _completedOperations = new ManualResetEvent[6];

        public Compressor(int blockSize, string sourceFilename, string destinationFilename, CancellationToken cancellationToken)
        {
            _blockSize = blockSize;
            _sourceFilename = sourceFilename;
            _destinationFilename = destinationFilename;
            _cancellationToken = cancellationToken;
            _compressingQueue = new ProducerConsumerQueue<ByteBlock>(4, Compress);
            _writingQueue = new ProducerConsumerQueue<ByteBlock>(2, Write);
        }

        public bool Compress()
        {
            for (var i = 0; i < _completedOperations.Length; i++)
            {
                _completedOperations[i] = new ManualResetEvent(false);
            }
            var readerThread = new Thread(Read);
            readerThread.Start();

            WaitHandle.WaitAll(_completedOperations);
            return true;
        }

        private void Read()
        {
            using (var fileToBeCompressed = new FileStream(_sourceFilename, FileMode.Open))
            {
                while (fileToBeCompressed.Position < fileToBeCompressed.Length)
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    int bytesRead;
                    if (fileToBeCompressed.Length - fileToBeCompressed.Position <= _blockSize)
                    {
                        bytesRead = (int) (fileToBeCompressed.Length - fileToBeCompressed.Position);
                    }
                    else
                    {
                        bytesRead = _blockSize;
                    }

                    var readBuffer = new byte[bytesRead];
                    fileToBeCompressed.Read(readBuffer, 0, bytesRead);
                    _compressingQueue.Enqueue(new ByteBlock(readBuffer));
                }
            }
        }

        private void Compress(ByteBlock byteBlock, int workerId)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    gZipStream.Write(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
                }

                ByteBlock compressedByteBlock = new ByteBlock(memoryStream.ToArray());
                _writingQueue.Enqueue(compressedByteBlock);
            }

            ManualResetEvent operation = _completedOperations[workerId];
            operation.Set();
        }

        private void Write(ByteBlock byteBlock, int workerId)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            using (var fileCompressed = new FileStream(_destinationFilename + ".gz", FileMode.Append))
            {
                BitConverter.GetBytes(byteBlock.Buffer.Length).CopyTo(byteBlock.Buffer, 4);
                fileCompressed.Write(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
            }

            ManualResetEvent operation = _completedOperations[workerId];
            operation.Set();
        }
    }
}