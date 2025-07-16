using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using CommonUtilties;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace AddSupp_VoteCollection
{
    public class Processor
    {
        public int Process(string dirPath, string verbose, string debug)
        {
            int itemsProcessed = 0;
            try
            {
                Outlook.Application outlookApp = new Outlook.Application();
                NameSpace objNS = outlookApp.GetNamespace("MAPI");
                Folder cFolder = null;

                // Parse _dirPath and navigate to the folder
                if (!string.IsNullOrEmpty(dirPath))
                {
                    string[] xArray = dirPath.Split('\\');
                    cFolder = objNS.Folders[xArray[0]] as Folder;

                    for (int i = 1; i < xArray.Length; i++)
                    {
                        cFolder = cFolder.Folders[xArray[i]] as Folder;
                    }
                }

                // Navigate to the target folder
                Folder oldFolder = cFolder.Folders["AddSupp_Vote"] as Folder;

                int currItem = cFolder.Items.Count;

                while (currItem > 0)
                {
                    MailItem cItem = cFolder.Items[currItem] as MailItem;

                    if (cItem != null)
                    {
                        string v_SubLine = cItem.Subject;

                        if (v_SubLine.Contains("Accepted:") || v_SubLine.Contains("Rejected:"))
                        {
                            MailItem outMail = cItem.Forward();
                            string fwdSubject = $"DO NOT REPLY :   Forwarding Response [{v_SubLine}]";

                            outMail.Recipients.Add("emily.driskell@nih.gov");
                            outMail.Recipients.Add("jonesni@mail.nih.gov");
                            outMail.Subject = fwdSubject;
                            outMail.Send();

                            itemsProcessed++;

                            // Log processing
                            DateTime processTimeStamp = DateTime.Now;
                            string logMssg = $"Processed! => EmailSender: {cItem.SenderName}; Subjectline: {v_SubLine}; Received Date: {cItem.ReceivedTime}";
                            string errorMssg = "";

                            CommonUtilities.WriteLog(8, logMssg, errorMssg, processTimeStamp);

                            // Move the item to the old folder
                            cItem.Move(oldFolder);
                        }
                    }

                    currItem--;
                }

                Marshal.ReleaseComObject(objNS);
                Marshal.ReleaseComObject(outlookApp);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return itemsProcessed;
        }

    }
}
