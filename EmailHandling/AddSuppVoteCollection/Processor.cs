using System;
using System.Linq;
using CommonUtilties;
using Microsoft.Extensions.Configuration;

namespace AddSuppVoteCollection
{
    /// <summary>
    /// Processor class for Administrative Supplement Vote Collection.
    /// 
    /// PURPOSE:
    /// Monitors an Outlook folder for voting response emails and forwards them
    /// to designated OGA staff.
    /// 
    /// OUTLOOK INTEGRATION:
    /// Uses late-bound COM automation (dynamic/Activator) to control Outlook.
    /// No Primary Interop Assembly (PIA) or NuGet interop package is required at compile time.
    /// Outlook must be installed and configured on the machine where this runs.
    /// </summary>
    public class Processor
    {
        public int Process(string dirPath, string verbose, string logDir, IConfiguration configuration)
        {
            int itemsProcessed = 0;

            // Load forward recipients from configuration
            var forwardRecipients = configuration.GetSection("VoteCollection:ForwardRecipients").GetChildren().Select(c => c.Value!).ToArray();

            CommonUtilities.Logger?.Information("Starting vote collection processing");
            CommonUtilities.Logger?.Debug("Folder path: {DirPath}", dirPath);
            CommonUtilities.Logger?.Debug("Forward recipients: {Recipients}", string.Join(", ", forwardRecipients));

            // Create Outlook application via late binding (no PIA needed)
            Type outlookType = Type.GetTypeFromProgID("Outlook.Application");
            if (outlookType == null)
            {
                CommonUtilities.Logger?.Error("Outlook is not installed or not registered on this machine");
                throw new InvalidOperationException("Outlook.Application COM class not found. Is Outlook installed?");
            }
            dynamic outlookApp = Activator.CreateInstance(outlookType);
            dynamic outlookNs = outlookApp.GetNamespace("MAPI");
            CommonUtilities.Logger?.Debug("Outlook application initialized via late binding");

            // Navigate to source and archive folders
            dynamic currentFolder = GetCurrentFolder(outlookNs, dirPath);

            // Debug: List subfolders under currentFolder
            CommonUtilities.Logger?.Information("Listing subfolders under: {FolderName}", (string)currentFolder.Name);
            foreach (dynamic subfolder in currentFolder.Folders)
            {
                CommonUtilities.Logger?.Information("  Folder: {FolderName}", (string)subfolder.Name);
            }

            dynamic oldFolder = currentFolder.Folders["AddSupp_Vote"];

            int currentItem = currentFolder.Items.Count;
            CommonUtilities.Logger?.Information("Found {ItemCount} items in folder", currentItem);

            // Process items from last to first (to avoid index shifting when moving)
            while (currentItem > 0)
            {
                try
                {
                    dynamic mailItem = currentFolder.Items[currentItem];
                    if (mailItem != null)
                    {
                        string subject = mailItem.Subject;

                        // Check if this is a voting response
                        if (subject.Contains("Accepted:") || subject.Contains("Rejected:"))
                        {
                            string voteType = subject.Contains("Accepted:") ? "Accepted" : "Rejected";
                            CommonUtilities.Logger?.Information("Processing {VoteType} vote from {Sender}: {Subject}",
                                voteType, (string)mailItem.SenderName, subject);

                            // Forward the voting response to OGA staff
                            dynamic outMail = mailItem.Forward();
                            foreach (var recipient in forwardRecipients)
                            {
                                outMail.Recipients.Add(recipient);
                            }
                            outMail.Subject = $"DO NOT REPLY : Forwarding Response [{subject}]";
                            outMail.Send();

                            CommonUtilities.Logger?.Debug("Forwarded vote to OGA staff");

                            itemsProcessed++;
                            Program.WriteLog($"Processed: {mailItem.SenderName}; {subject}", null, DateTime.Now, logDir);

                            // Archive the original email
                            mailItem.Move(oldFolder);
                            CommonUtilities.Logger?.Debug("Moved item to archive folder");
                        }
                    }
                }
                catch (Exception ex)
                {
                    CommonUtilities.Logger?.Error(ex, "Error processing item {ItemNumber}", currentItem);
                    Program.WriteLog($"Error processing item {currentItem}", ex.Message, DateTime.Now, logDir);
                }
                currentItem--;
            }

            CommonUtilities.Logger?.Information("Vote collection complete. Processed {Count} items", itemsProcessed);
            return itemsProcessed;
        }

        /// <summary>
        /// Navigates to an Outlook MAPI folder using a backslash-separated path.
        /// </summary>
        private dynamic GetCurrentFolder(dynamic ns, string dirPath)
        {
            CommonUtilities.Logger?.Debug("Navigating to folder: {Path}", dirPath);

            string[] dirs = dirPath.Split(new char[] { '\\' });
            dynamic folder = ns.Folders[dirs[0]];
            for (int i = 1; i < dirs.Length; i++)
            {
                if (!string.IsNullOrEmpty(dirs[i]))
                {
                    folder = folder.Folders[dirs[i]];
                }
            }
            return folder;
        }
    }
}
