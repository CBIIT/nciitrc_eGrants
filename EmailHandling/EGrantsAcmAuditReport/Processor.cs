using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;

namespace EGrantsAcmAuditReport
{
    /// <summary>
    /// Processor class for ACM Monthly Audit Report publishing.
    /// 
    /// PURPOSE:
    /// Scans a source directory for Excel audit report files, registers each file
    /// in the EIM database, and distributes copies to backup and image server locations
    /// for web access by eGrants users.
    /// 
    /// ORIGINAL SOURCE: Migrated from eGrants_ACM_Audit_report.vbs
    /// 
    /// WORKFLOW:
    /// 1. Opens a database connection and checks that the source directory exists
    /// 2. Iterates over all Excel files (*.xls, *.xlsx) in the source directory
    /// 3. Skips zero-length files (incomplete or failed report generations)
    /// 4. For each valid file:
    ///    a. Inserts a record into dbo.egrants_audit_report with the report metadata
    ///       (name, filename, last-modified date, and web URL)
    ///    b. Copies the file to the backup directory (local archive)
    ///    c. Copies the file to image server 1 (primary web-accessible UNC path)
    ///    d. Copies the file to image server 2 (secondary/redundant UNC path)
    ///    e. Deletes the original file from the source directory
    /// 5. File copy/delete failures are silently caught to allow processing to continue
    /// 
    /// DATABASE TABLE: dbo.egrants_audit_report
    /// - Report_name: Fixed value "Egrants ACM Monthly Audit Report"
    /// - File_name: The Excel filename (e.g., "ACM_Audit_Report_2024_01.xlsx")
    /// - Run_date: The file's last write time (when the report was generated)
    /// - url: Web-relative path used by the eGrants application to serve the file
    /// 
    /// DEPENDENCIES:
    /// - SQL Server EIM database with dbo.egrants_audit_report table
    /// - File system read access to the source directory
    /// - File system write access to the backup directory
    /// - Network access to image server UNC paths (may be unavailable in dev)
    /// </summary>
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

                    // Use parameterized query to prevent SQL injection
                    string sql = "INSERT INTO dbo.egrants_audit_report (Report_name, File_name, Run_date, url) VALUES(@ReportName, @FileName, @RunDate, @Url)";
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@ReportName", ReportName);
                        cmd.Parameters.AddWithValue("@FileName", fileName);
                        cmd.Parameters.AddWithValue("@RunDate", runDate);
                        cmd.Parameters.AddWithValue("@Url", fileUrl);
                        cmd.ExecuteNonQuery();
                    }

                    try { File.Copy(file.FullName, Path.Combine(bckDir, fileName), true); } catch { }
                    // Copy to primary web server share (serves files to eGrants users via web URL)
                    try { if (Directory.Exists(imgSvrPath)) File.Copy(file.FullName, Path.Combine(imgSvrPath, fileName), true); } catch { }
                    // Copy to secondary/archival network share (redundancy/failover)
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
