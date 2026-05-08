using System;
using CommonUtilties;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace AddSuppVoteCollection
{
    public class Processor
    {
        public int Process(string dirPath, string verbose, string logDir)
        {
            int itemsProcessed = 0;
            Outlook.Application outlookApp = new Outlook.Application();
            Outlook.NameSpace outlookNs = outlookApp.GetNamespace("MAPI");
            Outlook.MAPIFolder currentFolder = GetCurrentFolder(outlookNs, dirPath);
            Outlook.MAPIFolder oldFolder = currentFolder.Folders["AddSupp_Vote"];
            int currentItem = currentFolder.Items.Count;
            while (currentItem > 0)
            {
                try
                {
                    Outlook.MailItem mailItem = currentFolder.Items[currentItem] as Outlook.MailItem;
                    if (mailItem != null)
                    {
                        string subject = mailItem.Subject;
                        if (subject.Contains("Accepted:") || subject.Contains("Rejected:"))
                        {
                            var outMail = mailItem.Forward();
                            outMail.Recipients.Add("emily.driskell@nih.gov");
                            outMail.Recipients.Add("jonesni@mail.nih.gov");
                            outMail.Subject = "DO NOT REPLY : Forwarding Response [" + subject + "]";
                            outMail.Send();
                            itemsProcessed++;
                            Program.WriteLog("Processed: " + mailItem.SenderName + "; " + subject, null, DateTime.Now, logDir);
                            mailItem.Move(oldFolder);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Program.WriteLog("Error processing item " + currentItem, ex.Message, DateTime.Now, logDir);
                }
                currentItem--;
            }
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
