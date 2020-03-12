namespace GZipConsoleApp.Entities
{
    public class ByteBlock
    {
        public byte[] Buffer { get; set; }

        public ByteBlock(byte[] buffer)
        {
            Buffer = buffer;
        }
    }
}