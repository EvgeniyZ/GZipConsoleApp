using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GZipConsoleApp.Entities;

namespace GZipConsoleApp.Workers
{
    public class Decompressor : GZipper
    {
        private readonly ProducerConsumerQueue<ByteBlock> _decompressingQueue;
        private readonly ProducerConsumerQueue<ByteBlock> _writingQueue;
        private volatile bool _readCompleted;

        public Decompressor(int blockSize, string sourceFilename, string destinationFilename, CancellationToken cancellationToken,
            Action<string, Exception> onException) : base(blockSize, sourceFilename,
            destinationFilename, cancellationToken, onException)
        {
            _decompressingQueue = new ProducerConsumerQueue<ByteBlock>(Environment.ProcessorCount, Decompress);
            _writingQueue = new ProducerConsumerQueue<ByteBlock>(1, Write);
        }

        public bool Decompress()
        {
            var readerThread = new Thread(() =>
            {
                try
                {
                    Read();
                }
                catch (Exception e)
                {
                    OnException(Command.Decompress, e);
                    throw;
                }
            });
            readerThread.Start();
            while (!_readCompleted)
            {
            }

            _decompressingQueue.Stop();
            _writingQueue.Stop();

            return true;
        }

        private void Read()
        {
            using (var fileToBeDecompressed = new FileStream(SourceFilename, FileMode.Open))
            {
                while (fileToBeDecompressed.Position < fileToBeDecompressed.Length)
                {
                    CancellationToken.ThrowIfCancellationRequested();
                    byte[] idBuffer = new byte[4];
                    fileToBeDecompressed.Read(idBuffer, 0, idBuffer.Length);
                    int id = BitConverter.ToInt32(idBuffer);

                    byte[] originLengthBuffer = new byte[4];
                    fileToBeDecompressed.Read(originLengthBuffer, 0, originLengthBuffer.Length);
                    int originLength = BitConverter.ToInt32(originLengthBuffer);
                    byte[] compressedLengthBuffer = new byte[4];
                    fileToBeDecompressed.Read(compressedLengthBuffer, 0, compressedLengthBuffer.Length);
                    int compressedLength = BitConverter.ToInt32(compressedLengthBuffer);
                    byte[] compressedData = new byte[compressedLength];
                    fileToBeDecompressed.Read(compressedData, 0, compressedLength);

                    ByteBlock block = ByteBlock.FromCompressedData(id, compressedData, originLength);
                    _decompressingQueue.Enqueue(block);
                }

                _readCompleted = true;
            }
        }

        private void Decompress(ByteBlock byteBlock)
        {
            CancellationToken.ThrowIfCancellationRequested();
            byte[] decompressedData = new byte[byteBlock.OriginLength];
            using (MemoryStream memoryStream = new MemoryStream(byteBlock.CompressedData))
            {
                using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    try
                    {
                        gZipStream.Read(decompressedData, 0, decompressedData.Length);
                    }
                    catch (Exception e)
                    {
                        OnException(Command.Decompress, e);
                        throw;
                    }

                    ByteBlock block = ByteBlock.FromData(byteBlock.Id, decompressedData);
                    _writingQueue.Enqueue(block);
                }
            }
        }

        private void Write(ByteBlock byteBlock)
        {
            CancellationToken.ThrowIfCancellationRequested();
            using (var destination = new FileStream(DestinationFilename, FileMode.OpenOrCreate, FileAccess.Write))
            {
                try
                {
                    destination.Position = byteBlock.Id * (long) BlockSize;
                    destination.Write(byteBlock.Data, 0, byteBlock.Data.Length);
                    destination.Position = 0;
                }
                catch (Exception e)
                {
                    OnException(Command.Decompress, e);
                    throw;
                }
            }
        }
    }
}