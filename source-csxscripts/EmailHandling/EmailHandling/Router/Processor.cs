using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace Router
{
    internal class Processor
    {
        public static int Process(string dirPath, SqlConnection con, string verbose, string debug)
        {
            int itemsProcessedCount = 0;

            //Utilities.ShowDiagnosticIfVerbose($"_conStr: '{_conStr}'", _verbose);

            Utilities.ShowDiagnosticIfVerbose("Here we go ...", verbose);
            Outlook.Application oApp = new Outlook.Application();
            Utilities.ShowDiagnosticIfVerbose("Created the outlook object.", verbose);
            Outlook.NameSpace oNS = oApp.GetNamespace("MAPI");
            oNS.Logon("", "", false, true);
            Utilities.ShowDiagnosticIfVerbose($"Logged on to Outlook.", verbose);

            char sepchar = '\\';

            Utilities.ShowDiagnosticIfVerbose($"dirpath: {dirPath}", verbose);
            //Parse inputstr and Navigate to the folder
            if (!string.IsNullOrWhiteSpace(dirPath))
            {
                var dirs = dirPath.Split(sepchar);
                var i = 0;
                Utilities.ShowDiagnosticIfVerbose($"Setting objNS.Folders to {dirs[0]}", verbose);
                Outlook.MAPIFolder startingFolder = null;
                foreach (Outlook.MAPIFolder folder in oNS.Folders)
                {
                    Utilities.ShowDiagnosticIfVerbose($"Checking folder named ... {folder.Name} to see if it looks like '{dirs[i]}'", verbose);
                    if (folder.Name.Equals(dirs[i]))
                    {
                        Utilities.ShowDiagnosticIfVerbose($"Checking folder named ... {folder.Name}", verbose);
                        startingFolder = folder;
                    }
                }
                Utilities.ShowDiagnosticIfVerbose("Set", verbose);
                Outlook.MAPIFolder currentFolder = startingFolder;

                // loop through all the folders
                i++;
                while(i < dirs.Length - 1)
                {
                    bool foundThisDirectory = false;
                    Utilities.ShowDiagnosticIfVerbose($"dirs {i} : {dirs[i]}", verbose);

                    foreach (Outlook.MAPIFolder folder in currentFolder.Folders)
                    {
                        Utilities.ShowDiagnosticIfVerbose($"Checking folder named ... {folder.Name} to see if it looks like '{dirs[i]}'", verbose);
                        if (folder.Name.Equals(dirs[i]))
                        {
                            foundThisDirectory = true;
                            Utilities.ShowDiagnosticIfVerbose($"Checking folder named ... {folder.Name}", verbose);
                            currentFolder = folder;
                            break;
                        }
                    }
                    if (foundThisDirectory)
                    {
                        i++;
                    }
                }
            }


		//Do While i < UBound(xArray)

  //          call ShowDiagnosticIfVerbose("xArray i " & xArray(i), Verbose)
		//	Set CFolder = CFolder.Folders(xArray(i))

  //          i = i + 1

  //      Loop
  //  End If  'If dirpath <> "" Then

            //oNS.Folders()
            //Outlook.MAPIFolder oInbox = oNS.GetFolderFromID()
            //Outlook.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
            //Outlook.Items oItems = oInbox.Items;
            //Console.WriteLine(oItems.Count);
            //sb.Append($"Found {oItems.Count} many emails\r\n");
            //Outlook.MailItem oMsg = (Outlook.MailItem)oItems.GetFirst();
            //Console.WriteLine(oMsg.Subject);
            //sb.Append($"Here's the first subject : {oMsg.Subject}\r\n");
            //oNS.Logoff();
            //Console.ReadLine();

            return itemsProcessedCount;
        }
    }
}
