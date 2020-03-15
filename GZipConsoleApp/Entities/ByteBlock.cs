namespace GZipConsoleApp.Entities
{
    public class ByteBlock
    {
        public int Id { get; set; }
        public byte[] Buffer { get; set; }

        public ByteBlock(int id, byte[] buffer)
        {
            Id = id;
            Buffer = buffer;
        }
    }
}