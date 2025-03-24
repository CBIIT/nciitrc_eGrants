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
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry("Preparing to send .doc file ...", EventLogEntryType.Information, 101, 1);
            }

            var action = "http://localhost:8081/convert";
            byte[] fileBytes = null;

            using (var stream = new MemoryStream(content.GetBytes()))
            {
                Console.WriteLine("Acquired stream object.");
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry("Uploading ...", EventLogEntryType.Information, 101, 1);
                }
                var result = Upload(action, string.Empty, stream, null);
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry("Uploaded.", EventLogEntryType.Information, 101, 1);
                }

                // Read the response content as a byte array
                fileBytes = result.ReadAsByteArrayAsync().Result;
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry("Got result.", EventLogEntryType.Information, 101, 1);
                }
            }

            var pdfDocument = new PdfDocument(fileBytes);

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

                var response = client.PostAsync(actionUrl, fileStreamContent).Result;

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
