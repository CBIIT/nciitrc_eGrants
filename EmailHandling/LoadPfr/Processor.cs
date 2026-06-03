using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
using CommonUtilties;

namespace LoadPfr
{
    /// <summary>
    /// Processor for Load PFR
    /// Loads Progress/Final Reports from XML metadata files
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
                string applId = "", fileName = "", docDt = "", fileType = "", createdBy = "";

                foreach (XmlNode fieldNode in listNode.ChildNodes)
                {
                    switch (fieldNode.Name.ToLower())
                    {
                        case "applid": applId = fieldNode.InnerText; break;
                        case "filename": fileName = fieldNode.InnerText; break;
                        case "date": docDt = fieldNode.InnerText; break;
                        case "file_type": fileType = fieldNode.InnerText; break;
                        case "uid": createdBy = fieldNode.InnerText; break;
                    }
                }

                string pdfSrc = Path.Combine(docSrcPath, fileName);
                if (!File.Exists(pdfSrc)) continue;

                using (var cmd = new SqlCommand("Create_PFR", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@APPLID", applId);
                    cmd.Parameters.AddWithValue("@Rcvd_dt", DateTime.Parse(docDt));
                    cmd.Parameters.AddWithValue("@user_id", createdBy);
                    cmd.Parameters.AddWithValue("@filename", fileName);
                    cmd.Parameters.AddWithValue("@file_type", fileType);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string fileNumberName = reader["filenumbername"]?.ToString();
                            if (!string.IsNullOrEmpty(fileNumberName))
                            {
                                string alias = $"{fileNumberName}.{fileType}";
                                File.Copy(pdfSrc, Path.Combine(finalDstPath, alias), true);
                                Program.WriteLog($"Copied to: {alias}", null, DateTime.Now, logDir);
                            }
                        }
                    }
                }

                // Move to backup
                try
                {
                    File.Move(xmlFile.FullName, Path.Combine(bakDstPath, xmlFile.Name));
                    File.Move(pdfSrc, Path.Combine(bakDstPath, fileName));
                }
                catch { }
            }
        }
    }
}
