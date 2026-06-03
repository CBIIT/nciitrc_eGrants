using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace ExchangeLatest
{
    public class Processor
    {
     public int Process(string dirPath, SqlConnection con, string verbose, string outDir)
      {
     int itemsProcessed = 0;
      Outlook.Application outlookApp = new Outlook.Application();
     Outlook.NameSpace outlookNs = outlookApp.GetNamespace("MAPI");
con.Open();

  Outlook.MAPIFolder folder = GetCurrentFolder(outlookNs, dirPath, verbose);
    int itemToProcess = folder.Items.Count;

  while (itemToProcess > 0)
      {
       try
  {
 Outlook.MailItem item = folder.Items[itemToProcess] as Outlook.MailItem;
   if (item != null)
   {
   var p = ParseSubjectLine(item.Subject);
   if (!string.IsNullOrEmpty(p.GrantNumber) || !string.IsNullOrEmpty(p.ApplId) || !string.IsNullOrEmpty(p.Category))
   {
      ProcessEmail(con, item, p, GetSenderId(item), outDir, verbose);
     itemsProcessed++;
          }
  }
   }
            catch (Exception ex) { CommonUtilities.WriteLog(8, $"Error item {itemToProcess}", ex.Message, DateTime.Now); }
       itemToProcess--;
 }
        con.Close();
 return itemsProcessed;
    }

     private SubjectParams ParseSubjectLine(string subject)
        {
         var p = new SubjectParams();
    foreach (var part in subject.Split(','))
   {
  string lp = part.Trim().ToLower();
  if (lp.Contains("grantnumber")) p.GrantNumber = ExtractValue(part, "grantnumber");
  else if (lp.Contains("category")) p.Category = ExtractValue(part, "category");
   else if (lp.Contains("applid")) p.ApplId = ExtractValue(part, "applid");
    else if (lp.Contains("sub=")) p.SubCategory = ExtractValue(part, "sub");
  else if (lp.Contains("extract")) p.Extract = ExtractValue(part, "extract");
    }
            return p;
   }

        private void ProcessEmail(SqlConnection con, Outlook.MailItem item, SubjectParams p, string senderId, string outDir, string verbose)
   {
    string applId = p.ApplId;
  if (string.IsNullOrEmpty(applId)) applId = GetApplId(RemoveSpecialChars(p.GrantNumber ?? ""), con);
    if (string.IsNullOrEmpty(applId)) applId = GetApplId(RemoveSpecialChars(item.Subject), con);
     if (string.IsNullOrEmpty(applId)) applId = GetApplId(RemoveSpecialChars(item.Body), con);

  using (var cmd = new SqlCommand("getPlaceHolder_new", con))
   {
   cmd.CommandType = CommandType.StoredProcedure;
   cmd.Parameters.AddWithValue("@param1", applId ?? "");
  cmd.Parameters.AddWithValue("@param2", "");
  cmd.Parameters.AddWithValue("@param3", item.ReceivedTime);
 cmd.Parameters.AddWithValue("@param4", p.Category ?? "Correspondence");
     cmd.Parameters.AddWithValue("@param5", "txt");
   cmd.Parameters.AddWithValue("@param6", item.Subject);
 cmd.Parameters.AddWithValue("@param7", item.Body);
   cmd.Parameters.AddWithValue("@param8", p.SubCategory ?? "");

    using (var reader = cmd.ExecuteReader())
     {
    if (reader.Read())
     {
       string fileNum = reader[0].ToString();
      string extract = p.Extract ?? "1";
 if (extract == "1" || extract == "3") item.SaveAs(Path.Combine(outDir, $"{fileNum}.txt"), Outlook.OlSaveAsType.olTXT);
   if ((extract == "2" || extract == "3") && item.Attachments.Count > 0)
    item.Attachments[1].SaveAsFile(Path.Combine(outDir, $"{fileNum}.{GetFileType(item.Attachments[1].FileName)}"));
   }
  }
   }
     }

        private Outlook.MAPIFolder GetCurrentFolder(Outlook.NameSpace ns, string dirPath, string verbose)
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

  private string GetApplId(string text, SqlConnection con)
  {
   try { using (var cmd = new SqlCommand($"SELECT dbo.Imm_fn_applid_match('{text}')", con)) return cmd.ExecuteScalar()?.ToString() ?? ""; }
   catch { return ""; }
  }

   private string ExtractValue(string p, string name)
 {
   string[] parts = p.Split('=');
    return (parts.Length == 2 && parts[0].Trim().ToLower().Contains(name)) ? parts[1].Trim() : null;
     }

        private string GetFileType(string fileName) => fileName.Contains(".") ? fileName.Substring(fileName.LastIndexOf('.') + 1) : "txt";
     private string RemoveSpecialChars(string text) => text.Replace(":", " ").Replace("/", " ").Replace("\\", " ").Replace(" ", "").Trim();

   private class SubjectParams { public string GrantNumber, Category, ApplId, SubCategory, Extract; }
    }
}
