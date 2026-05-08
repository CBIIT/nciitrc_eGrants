using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace DocManEmail
{
    public class Processor
 {
        public int Process(SqlConnection con, string dirPath, string outDir, string verbose, string logDir)
        {
            int itemsProcessed = 0;
     Outlook.Application outlookApp = new Outlook.Application();
            Outlook.NameSpace outlookNs = outlookApp.GetNamespace("MAPI");
          con.Open();

   Outlook.MAPIFolder currentFolder = GetCurrentFolder(outlookNs, dirPath);
         Outlook.MAPIFolder oldFolder = currentFolder.Folders["old"];
    int itemToProcess = currentFolder.Items.Count;

            while (itemToProcess > 0 && itemsProcessed < 50)
          {
       try
   {
       Outlook.MailItem item = currentFolder.Items[itemToProcess] as Outlook.MailItem;
                    if (item != null)
            {
         string senderId = GetSenderId(item);
               string cpiid = ExtractValue(ExtractElement(item.Subject, 1), "cpiid");
   string docid = ExtractValue(ExtractElement(item.Subject, 1), "docid");

         if (!string.IsNullOrWhiteSpace(cpiid) || !string.IsNullOrWhiteSpace(docid))
         {
             ProcessDocument(con, item, cpiid, docid, senderId, outDir, verbose);
      item.Move(oldFolder);
   itemsProcessed++;
    Program.WriteLog($"Processed: {senderId}; {item.Subject}", null, DateTime.Now, logDir);
      }
      }
      }
            catch (Exception ex)
    {
 Program.WriteLog($"Error item {itemToProcess}", ex.Message, DateTime.Now, logDir);
      }
        itemToProcess = currentFolder.Items.Count;
         }
         con.Close();
            return itemsProcessed;
  }

        private void ProcessDocument(SqlConnection con, Outlook.MailItem item, string cpiid, string docid, string senderId, string outDir, string verbose)
        {
   using (var cmd = new SqlCommand("SP_CREATE_DOCMAN_DOCUMENT_NEW", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
    cmd.Parameters.AddWithValue("@CP", string.IsNullOrEmpty(cpiid) ? (object)DBNull.Value : cpiid);
cmd.Parameters.AddWithValue("@CAT", ExtractValue(ExtractElement(item.Subject, 2), "catid") ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@SEQ", ExtractValue(ExtractElement(item.Subject, 3), "num") ?? (object)DBNull.Value);
   cmd.Parameters.AddWithValue("@DD", ExtractValue(ExtractElement(item.Subject, 4), "date") ?? "");
        cmd.Parameters.AddWithValue("@UID", senderId);
       cmd.Parameters.AddWithValue("@FT", "pdf");
         cmd.Parameters.AddWithValue("@ACTIONID", string.IsNullOrEmpty(cpiid) ? "2" : "1");
                cmd.Parameters.AddWithValue("@DOCID", string.IsNullOrEmpty(docid) ? (object)DBNull.Value : docid);
       cmd.Parameters.AddWithValue("@REASON", ExtractValue(ExtractElement(item.Subject, 3), "reason") ?? (object)DBNull.Value);

                using (var reader = cmd.ExecuteReader())
            {
      if (reader.Read() && item.Attachments.Count > 0)
      {
   string docId = reader[0].ToString();
       item.Attachments[1].SaveAsFile(Path.Combine(outDir, $"{docId}.pdf"));
        }
           }
  }
        }

        private Outlook.MAPIFolder GetCurrentFolder(Outlook.NameSpace ns, string dirPath)
        {
        string[] dirs = dirPath.Split('\\');
   Outlook.MAPIFolder folder = ns.Folders[dirs[0]];
        for (int i = 1; i < dirs.Length; i++)
     if (!string.IsNullOrEmpty(dirs[i])) folder = folder.Folders[dirs[i]];
    return folder;
        }

        private string GetSenderId(Outlook.MailItem item)
        {
            if (item.SenderEmailType == "EX" && item.Sender?.GetExchangeUser() != null)
                return item.Sender.GetExchangeUser().Alias;
   return item.SenderEmailType == "SMTP" ? item.SenderEmailAddress : "";
        }

        private string ExtractElement(string str, int n)
   {
        string[] parts = str.Split(',');
            return (n > 0 && n <= parts.Length) ? parts[n - 1].Trim() : "";
        }

  private string ExtractValue(string p, string name)
{
            string[] parts = p.Split('=');
            return (parts.Length == 2 && parts[0].Trim().ToLower().Contains(name)) ? parts[1].Trim() : null;
        }
 }
}
