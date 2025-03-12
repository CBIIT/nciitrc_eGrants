using EmailConcatenation.Interfaces;
using IronPdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spire.Doc;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Org.BouncyCastle.Tls;
using System.Net.Http;
using Org.BouncyCastle.Utilities;

namespace EmailConcatenation.Converters
{
    public class WordDocConverter : IWordDocConverter, IConvertToPdf
    {
        public bool SupportsThisFileType(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName) &&
                fileName.ToLower().EndsWith(".doc") )
                return true;
            return false;
        }

        public List<PdfDocument> ToPdfDocument(ContentForPdf content)
        {
            Console.WriteLine("Handling Word .doc file type case ...");

            var action = "http://localhost:8081/convert";
            byte[] fileBytes = null;

            using (var stream = new MemoryStream(content.GetBytes()))
            {
                Console.WriteLine("Acquired stream object.");
                var result = Upload(action, string.Empty, stream, null);

                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry($"Upload complete", EventLogEntryType.Information, 101, 1);
                }


                // Read the response content as a byte array
                fileBytes = result.ReadAsByteArrayAsync().Result;

                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry($"Bytes read", EventLogEntryType.Information, 101, 1);
                }
            }

            var pdfDocument = new PdfDocument(fileBytes);

            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry($"Created PdfDocument", EventLogEntryType.Information, 101, 1);
            }

            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                var fileBytesNullOrEmpty = fileBytes == null || fileBytes.Length == 0;
                eventLog.WriteEntry($"Is fileBytes null or empty ? {fileBytesNullOrEmpty}", EventLogEntryType.Information, 101, 1);
            }

            return new List<PdfDocument> { pdfDocument };
        }

        private static HttpContent Upload(string actionUrl, string paramString, Stream paramFileStream, byte[] paramFileBytes)
        {
            using (var client = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            {
                paramFileStream.Position = 0;
                HttpContent fileStreamContent = new StreamContent(paramFileStream);
                Console.WriteLine("Acquired client and formData.");

                fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry($"About to POST async to the PDF conversion task server", EventLogEntryType.Information, 101, 1);
                }

                var response = client.PostAsync(actionUrl, fileStreamContent).Result;

                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry($"PDF conversion HTTP response status code : {response.StatusCode}", EventLogEntryType.Information, 101, 1);
                }

                response.EnsureSuccessStatusCode();

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                return response.Content;
            }
        }
    }
}
