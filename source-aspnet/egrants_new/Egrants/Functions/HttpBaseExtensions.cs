using System;
using System.IO;
using System.IO.Compression;
using System.Web;

namespace egrants_new.Egrants.Functions
{
    public static class HttpPostedFileBaseExtensions
    {
        public static Byte[] ToByteArray(this HttpPostedFileBase value)
        {
            if (value == null)
                return null;
            var array = new Byte[value.ContentLength];
            value.InputStream.Position = 0;
            value.InputStream.Read(array, 0, value.ContentLength);
            return array;
        }
    }
}