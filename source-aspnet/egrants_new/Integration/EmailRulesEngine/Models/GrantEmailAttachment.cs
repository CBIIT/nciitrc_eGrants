using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Web;
using System.Windows.Forms.VisualStyles;
using FileStyleUriParser = System.FileStyleUriParser;

namespace egrants_new.Integration.EmailRulesEngine.Models
{
    public class GrantEmailAttachment
    {
        public string Id { get; set; }
        public string ContentId { get; set; }
        public string ContentType { get; set; }
        public string ContentBytes { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }

        public void SaveToDisk(string path, string name, string extension)
        {
            try
            {
                string filename = String.Join(".", name, extension);
                string filepath = Path.Combine(path, name);
                File.WriteAllBytes(filepath, Convert.FromBase64String(ContentBytes));

            }
            catch (Exception ex)
            {
                // TODO: Handle the exception, but for now just throw it 
                throw ex;
            }
        }

    }
}