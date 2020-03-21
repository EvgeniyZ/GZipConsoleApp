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

        public Compressor(int blockSize, string sourceFilename, string destinationFilename, CancellationToken cancellationToken,
            Action<string, Exception> onException) : base(blockSize, sourceFilename,
            destinationFilename, Entities.Command.Compress, cancellationToken, onException)
        {
            _compressingQueue = new ProducerConsumerQueue<ByteBlock>(Environment.ProcessorCount, Compress);
            _writingQueue = new ProducerConsumerQueue<ByteBlock>(1, Write);
        }

        public bool Compress()
        {
            StartRead();
            while (!ReadCompleted)
            {
            }

            _compressingQueue.Stop();
            _writingQueue.Stop();

            return true;
        }

        protected override void Read()
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
                        OnException(Command, e);
                    }

                    _compressingQueue.Enqueue(ByteBlock.FromData(currentBlockId++, data));
                }

                ReadCompleted = true;
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
                        OnException(Command, e);
                    }
                }

                ByteBlock compressedByteBlock = ByteBlock.FromData(byteBlock.Id, byteBlock.Data, memoryStream.ToArray());
                _writingQueue.Enqueue(compressedByteBlock);
            }
        }

        private void Write(ByteBlock byteBlock)
        {
            CancellationToken.ThrowIfCancellationRequested();
            using (var fileCompressed = new FileStream(ZipSettings.GetFilenameWithZipExtension(DestinationFilename), FileMode.Append))
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
                    OnException(Command, e);
                }
            }
        }
    }
}