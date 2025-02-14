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

            string tempFilePath = Path.GetTempFileName();
            File.WriteAllBytes(tempFilePath, content.GetBytes());

        using (EventLog eventLog = new EventLog("Application"))
        {
            eventLog.Source = "Application";
            eventLog.WriteEntry($"Bytes written to {tempFilePath}", EventLogEntryType.Information, 101, 1);
        }

            string folderPathToLibre = "C:\\Program Files\\LibreOffice\\program";               // works
            Console.WriteLine($"Checking existence of directory at {folderPathToLibre}");
            var checkFolderExists = Directory.Exists(folderPathToLibre);
            var checkFolderExistsText = checkFolderExists ? "True" : "False";

            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry($"Does libre folder exist ? {checkFolderExistsText}", EventLogEntryType.Information, 101, 1);
            }

            if (!checkFolderExists)
                throw new Exception($"Didn't find libre office folder. Is this installed ?? Location : {folderPathToLibre}");

            string sofficePath = "C:\\Program Files\\LibreOffice\\program\\soffice.com";
            string args = $"--headless --convert-to docx \"{tempFilePath}\" --outdir \"{Path.GetDirectoryName(tempFilePath)}\"";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = sofficePath,
                //FileName = "whoami",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            //startInfo. += (DatagramSender, Exception) =>
            //{

            //}

            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry("Process INFO object created", EventLogEntryType.Information, 101, 1);
            }

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        using (EventLog eventLog = new EventLog("Application"))
                        {
                            eventLog.Source = "Application";
                            eventLog.WriteEntry($"Got some kinda output : {e.Data}", EventLogEntryType.Information, 101, 1);
                        }
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        using (EventLog eventLog = new EventLog("Application"))
                        {
                            eventLog.Source = "Application";
                            eventLog.WriteEntry($"Got some kinda error : {e.Data}", EventLogEntryType.Information, 101, 1);
                        }
                    }
                };

                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry("Process object created", EventLogEntryType.Information, 101, 1);
                }

                process.Start();

                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry("Process object created", EventLogEntryType.Information, 101, 1);
                }

                // make sure it doesn't have any output like this :
                // Entity: line 1: parser error : Document is empty

                string errorIndicatorString = "parser error :";

                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry("Reading stdout and stderr", EventLogEntryType.Information, 101, 1);        // made it to here on dev !!
                }

            //string output = process.StandardOutput.ReadToEnd();
             //       string error = process.StandardError.ReadToEnd();

                // doesn't make it this far if ReadToEnd() lines above are uncommmented !



                using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry("Waiting for exit ...", EventLogEntryType.Information, 101, 1);
            }

                process.WaitForExit();

            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry("Exit reached", EventLogEntryType.Information, 101, 1);
            }

                //using (EventLog eventLog = new EventLog("Application"))
                //{
                //    eventLog.Source = "Application";
                //    eventLog.WriteEntry($"Output : {output}", EventLogEntryType.Information, 101, 1);
                //}

                //if (output.Contains(errorIndicatorString) || error.Contains(errorIndicatorString))
                //{
                //    throw new ExternalException($"Creating the Word .docx file from the .doc failed after attempting to use this command : {sofficePath} {args}");
                //}
            }

            string convertedFilePath = Path.Combine(Path.GetDirectoryName(tempFilePath), Path.GetFileNameWithoutExtension(tempFilePath) + ".docx");

        using (EventLog eventLog = new EventLog("Application"))
        {
            eventLog.Source = "Application";
            eventLog.WriteEntry($"convertedFilePath : {convertedFilePath}", EventLogEntryType.Information, 101, 1);
        }

            DocxToPdfRenderer docXRenderer = new DocxToPdfRenderer();
            PdfDocument pdfDocument = docXRenderer.RenderDocxAsPdf(convertedFilePath);

            // Clean up
            File.Delete(tempFilePath);
            File.Delete(convertedFilePath);

            return new List<PdfDocument> { pdfDocument };
        }
    }
}
