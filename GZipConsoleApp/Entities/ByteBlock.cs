namespace GZipConsoleApp.Entities
{
    public class ByteBlock
    {
        public int Id { get; set; }
        public byte[] Data { get; set; }

        public int Length { get; set; }
        public byte[] CompressedData { get; set; }

        public static ByteBlock FromData(int id, byte[] data, byte[] compressedData = default)
        {
            return new ByteBlock
            {
                Id = id,
                Data = data,
                Length = data.Length,
                CompressedData = compressedData
            };
        }

        public static ByteBlock FromCompressedData(int id, byte[] compressedData, int length, byte[] data = default)
        {
            return new ByteBlock
            {
                Id = id,
                Data = data,
                CompressedData = compressedData,
                Length = length
            };
        }
    }
}