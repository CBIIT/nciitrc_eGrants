using EmailConcatenation.Interfaces;
using IronPdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailConcatenation.Converters
{
    internal class GeneralImageConverter : IGeneralImageConverter, IConvertToPdf
    {
        public bool SupportsThisFileType(string fileName)
        {
            if (fileName.ToLower().Contains(".") &&
                Constants.SupportedImageTypes.Any(sit => fileName.ToLower().Contains(sit)) &&
                !fileName.ToLower().Contains(".tif"))   // separate converter for .tif files
                return true;
            return false;
        }

        public List<PdfDocument> ToPdfDocument(ContentForPdf content)
        {
            Console.WriteLine("Handling general image case ...");

            // Found an image with a supported type.
            // Convert an image to a PDF
            var fileNameTokens = content.Attachment.FileName.ToLower().Split('.');
            var fileNameExtension = fileNameTokens[fileNameTokens.Length - 1].ToLower();  // the file extension is the last one
            Console.WriteLine($"Filename extension : '{fileNameExtension}'");

            if (content == null)
                return null;

            using (var imageStream = new MemoryStream(content.GetBytes()))
            {
                if (imageStream.Length == 0)
                    return null;

                Bitmap bitmap = new Bitmap(imageStream);
                var chromeRenderer = new ChromePdfRenderer();
                string base64Image = Convert.ToBase64String((byte[])new ImageConverter().ConvertTo(bitmap, typeof(byte[])));
                // should end up looking like this : <img src='data:image/png;base64,... [long number of characters] />' />";
                string htmlString = $"<img src='data:image/{fileNameExtension};base64,{base64Image}' />";
                Console.WriteLine($"fileNameExtension : {fileNameExtension}");
                var pdfDoc = chromeRenderer.RenderHtmlAsPdf(htmlString);
                return new List<PdfDocument> { pdfDoc };
            }
        }
    }
}
