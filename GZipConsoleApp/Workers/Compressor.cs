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
        private readonly ProducerConsumerQueue<ByteBlock> _writingQueue;
        private volatile bool _readCompleted;

        public Compressor(int blockSize, string sourceFilename, string destinationFilename, CancellationToken cancellationToken,
            Action<string, Exception> onException) : base(blockSize, sourceFilename,
            destinationFilename, cancellationToken, onException)
        {
            _compressingQueue = new ProducerConsumerQueue<ByteBlock>(Environment.ProcessorCount, Compress);
            _writingQueue = new ProducerConsumerQueue<ByteBlock>(1, Write);
        }

        public bool Compress()
        {
            var readerThread = new Thread(() =>
            {
                try
                {
                    Read();
                }
                catch (Exception e)
                {
                    OnException(Command.Compress, e);
                    throw;
                }
            });
            readerThread.Start();
            while (!_readCompleted)
            {
            }

            _compressingQueue.Stop();
            _writingQueue.Stop();

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

                    var data = new byte[bytesRead];
                    try
                    {
                        fileToBeCompressed.Read(data, 0, bytesRead);
                    }
                    catch (Exception e)
                    {
                        OnException(Command.Compress, e);
                        throw;
                    }
                    _compressingQueue.Enqueue(ByteBlock.FromData(currentBlockId++, data));
                }
                _readCompleted = true;
            }
        }

        private void Compress(ByteBlock byteBlock)
        {
            CancellationToken.ThrowIfCancellationRequested();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    try
                    {
                        gZipStream.Write(byteBlock.Data, 0, byteBlock.Data.Length);
                    }
                    catch (Exception e)
                    {
                        OnException(Command.Compress, e);
                        throw;
                    }
                }

                ByteBlock compressedByteBlock = ByteBlock.FromData(byteBlock.Id, byteBlock.Data, memoryStream.ToArray());
                _writingQueue.Enqueue(compressedByteBlock);
            }
        }

        private void Write(ByteBlock byteBlock)
        {
            CancellationToken.ThrowIfCancellationRequested();
            using (var fileCompressed = new FileStream(DestinationFilename + ZipSettings.ZipExtension, FileMode.Append))
            {
                try
                {
                    var idAsByteArray = BitConverter.GetBytes(byteBlock.Id);
                    fileCompressed.Write(idAsByteArray, 0, idAsByteArray.Length);

                    var dataLengthAsByteArray = BitConverter.GetBytes(byteBlock.Data.Length);
                    fileCompressed.Write(dataLengthAsByteArray, 0, dataLengthAsByteArray.Length);

                    var compressedLengthAsByteArray = BitConverter.GetBytes(byteBlock.CompressedData.Length);
                    fileCompressed.Write(compressedLengthAsByteArray, 0, compressedLengthAsByteArray.Length);
                    fileCompressed.Write(byteBlock.CompressedData, 0, byteBlock.CompressedData.Length);
                }
                catch (Exception e)
                {
                    OnException(Command.Compress, e);
                    throw;
                }
            }
        }
    }
}