using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;

namespace EGrantsAcmAuditReport
{
    public class Processor
    {
        private const string ReportName = "Egrants ACM Monthly Audit Report";

        public int Process(SqlConnection con, string srcDir, string bckDir, string imgSvrPath, string imgSvrPath2, string verbose, string logDir)
        {
            int filesProcessed = 0;
            con.Open();

            if (!Directory.Exists(srcDir)) return 0;

            foreach (var file in new DirectoryInfo(srcDir).GetFiles("*.xls*"))
            {
                try
                {
                    if (file.Length == 0) continue;

                    string fileName = file.Name;
                    DateTime runDate = file.LastWriteTime;
                    string fileUrl = $"/data/funded/egrantsadmin/auditreport/{fileName}";

                    string sql = $"INSERT INTO dbo.egrants_audit_report (Report_name, File_name, Run_date, url) VALUES('{ReportName}', '{fileName}', '{runDate:yyyy-MM-dd HH:mm:ss}', '{fileUrl}')";
                    using (var cmd = new SqlCommand(sql, con)) cmd.ExecuteNonQuery();

                    try { File.Copy(file.FullName, Path.Combine(bckDir, fileName), true); } catch { }
                    try { if (Directory.Exists(imgSvrPath)) File.Copy(file.FullName, Path.Combine(imgSvrPath, fileName), true); } catch { }
                    try { if (Directory.Exists(imgSvrPath2)) File.Copy(file.FullName, Path.Combine(imgSvrPath2, fileName), true); } catch { }
                    try { File.Delete(file.FullName); } catch { }

                    filesProcessed++;
                    Program.WriteLog($"Processed: {fileName}", null, DateTime.Now, logDir);
                }
                catch (Exception ex)
                {
                    Program.WriteLog($"Error: {file.Name}", ex.Message, DateTime.Now, logDir);
                }
            }
            con.Close();
            return filesProcessed;
        }
    }
}
