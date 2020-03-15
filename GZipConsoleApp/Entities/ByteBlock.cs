namespace GZipConsoleApp.Entities
{
    public class ByteBlock
    {
        public int Id { get; private set; }
        public byte[] Data { get; private set; }
        public int OriginLength { get; private set; }
        public byte[] CompressedData { get; private set; }

        public static ByteBlock FromData(int id, byte[] data, byte[] compressedData = default)
        {
            return new ByteBlock
            {
                Id = id,
                Data = data,
                CompressedData = compressedData,
                OriginLength = data.Length
            };
        }

        public static ByteBlock FromCompressedData(int id, byte[] compressedData, int length)
        {
            return new ByteBlock
            {
                Id = id,
                CompressedData = compressedData,
                OriginLength = length
            };
        }
    }
}