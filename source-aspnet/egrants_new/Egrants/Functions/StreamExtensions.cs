using System.IO;

namespace egrants_new.Egrants.Functions
{
    public static class StreamExtensions
    {
        public static byte[] ReadAllBytes(this Stream inStream)
        {
            if (inStream is MemoryStream)
                return ((MemoryStream)inStream).ToArray();

            using (var memoryStream = new MemoryStream())
            {
                inStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}