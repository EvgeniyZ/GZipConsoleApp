namespace GZipConsoleApp.Entities
{
    public static class ZipSettings
    {
        public const string ZipExtension = ".gz";

        public static string GetFilenameWithZipExtension(string filename)
        {
            return filename + ZipExtension;
        }
    }
}