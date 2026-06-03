using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
using CommonUtilties;

namespace LoadSuppPfr
{
    /// <summary>
    /// Processor for Load Supplement PFR
    /// Loads Supplement Progress/Final Reports from XML metadata files
    /// </summary>
    public class Processor
    {
        public int Process(SqlConnection con, string docSrcPath, string bakDstPath, string finalDstPath, string verbose, string logDir)
        {
            int filesProcessed = 0;
            con.Open();
            CommonUtilities.ShowDiagnosticIfVerbose("Database connection opened", verbose);

            if (!Directory.Exists(docSrcPath))
            {
                CommonUtilities.ShowDiagnosticIfVerbose($"Source directory does not exist: {docSrcPath}", verbose);
                return 0;
            }

            foreach (var xmlFile in new DirectoryInfo(docSrcPath).GetFiles("*.xml"))
            {
                try
                {
                    Program.WriteLog($"Processing: {xmlFile.FullName}", null, DateTime.Now, logDir);
                    ProcessXmlFile(con, xmlFile, docSrcPath, bakDstPath, finalDstPath, verbose, logDir);
                    filesProcessed++;
                }
                catch (Exception ex)
                {
                    Program.WriteLog($"Error processing: {xmlFile.Name}", ex.Message, DateTime.Now, logDir);
                }
            }

            con.Close();
            return filesProcessed;
        }

        private void ProcessXmlFile(SqlConnection con, FileInfo xmlFile, string docSrcPath, string bakDstPath, string finalDstPath, string verbose, string logDir)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFile.FullName);

            var documentElement = xmlDoc.DocumentElement;
            if (documentElement == null) return;

            foreach (XmlNode listNode in documentElement.ChildNodes)
            {
                string applId = "", catName = "", fileName = "", docDt = "", fileType = "";

                foreach (XmlNode fieldNode in listNode.ChildNodes)
                {
                    switch (fieldNode.Name.ToLower())
                    {
                        case "applid": applId = fieldNode.InnerText; break;
                        case "folderid": if (fieldNode.InnerText == "19") catName = "PFR"; break;
                        case "filename": fileName = fieldNode.InnerText; break;
                        case "date": docDt = fieldNode.InnerText; break;
                        case "file_type": fileType = fieldNode.InnerText; break;
                    }
                }

                using (var cmd = new SqlCommand("getPlaceHolder_new", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@param1", applId);
                    cmd.Parameters.AddWithValue("@param2", " ");
                    cmd.Parameters.AddWithValue("@param3", DateTime.Parse(docDt));
                    cmd.Parameters.AddWithValue("@param4", catName);
                    cmd.Parameters.AddWithValue("@param5", fileType);
                    cmd.Parameters.AddWithValue("@param6", $"Supplement PFR - {applId}");
                    cmd.Parameters.AddWithValue("@param7", "");
                    cmd.Parameters.AddWithValue("@param8", "PFR");

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string fileNumberName = reader[0]?.ToString();
                            if (!string.IsNullOrEmpty(fileNumberName))
                            {
                                string pdfSrc = Path.Combine(docSrcPath, fileName);
                                string alias = $"{fileNumberName}.{fileType}";
                                if (File.Exists(pdfSrc))
                                {
                                    File.Copy(pdfSrc, Path.Combine(finalDstPath, alias), true);
                                    Program.WriteLog($"Copied to: {alias}", null, DateTime.Now, logDir);
                                    try { File.Move(pdfSrc, Path.Combine(bakDstPath, fileName)); } catch { }
                                }
                            }
                        }
                    }
                }

                // Move XML to backup
                try { File.Move(xmlFile.FullName, Path.Combine(bakDstPath, xmlFile.Name)); } catch { }
            }
        }
    }
}
