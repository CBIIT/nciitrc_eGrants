using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using CommonUtilties;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace AddSuppProd
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
            while (itemToProcess > 0)
        {
           try
   {
   Outlook.MailItem currentItem = currentFolder.Items[itemToProcess] as Outlook.MailItem;
     if (currentItem != null)
       {
      currentItem.Move(oldFolder);
        itemsProcessed++;
          }
      }
     catch (Exception ex)
      {
 Program.WriteLog("Error processing item " + itemToProcess, ex.Message, DateTime.Now, logDir);
  }
                itemToProcess--;
            }
 con.Close();
          return itemsProcessed;
        }

     private Outlook.MAPIFolder GetCurrentFolder(Outlook.NameSpace ns, string dirPath)
        {
       string[] dirs = dirPath.Split(new char[] { '\\' });
Outlook.MAPIFolder folder = ns.Folders[dirs[0]];
       for (int i = 1; i < dirs.Length; i++)
      if (!string.IsNullOrEmpty(dirs[i])) folder = folder.Folders[dirs[i]];
  return folder;
        }
  }
}
