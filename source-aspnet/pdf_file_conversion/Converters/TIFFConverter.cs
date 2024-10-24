using EmailConcatenation.Interfaces;
using IronPdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailConcatenation.Converters
{
    internal class TIFFConverter : ITIFFConverter
    {
        public bool SupportsThisFileType(string fileName)
        {
            if (fileName.ToLower().Contains(".") &&
                fileName.ToLower().Contains(".tif"))  
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

            Console.WriteLine("Handling special case .tif file ...");
            using (var tiffStream = new MemoryStream(content.Attachment.Data))
            {
                Bitmap tiffBitmap = new Bitmap(tiffStream);
                using (var pngStream = new MemoryStream())
                {
                    tiffBitmap.Save(pngStream, ImageFormat.Png);
                    pngStream.Position = 0;

                    var chromeRenderer = new ChromePdfRenderer();
                    string base64Image = Convert.ToBase64String(pngStream.ToArray());
                    // should end up looking like this : <img src='data:image/png;base64,... [long number of characters] />' />";
                    string htmlString = $"<img src='data:image/{fileNameExtension};base64,{base64Image}' />";
                    var pdfDoc = chromeRenderer.RenderHtmlAsPdf(htmlString);
                    return new List<PdfDocument> { pdfDoc };
                }
            }
        }
    }
}
