using System;
using CommonUtilties;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace AddSuppVoteCollection
{
    /// <summary>
    /// Processor class for Administrative Supplement Vote Collection.
    /// 
    /// PURPOSE:
    /// Monitors an Outlook folder for voting response emails and forwards them
    /// to designated OGA staff. Voting responses are generated when Program
    /// Directors use Outlook's voting buttons to respond to supplement notifications.
    /// 
    /// ORIGINAL SOURCE: Migrated from VBS supplement vote collection scripts
    /// 
    /// DETECTION LOGIC:
    /// Emails are considered voting responses if their subject contains:
    /// - "Accepted:" - PD approved the supplement
    /// - "Rejected:" - PD rejected the supplement
    /// 
    /// FORWARDING:
    /// Matching emails are forwarded to:
    /// - emily.driskell@nih.gov
    /// - jonesni@mail.nih.gov
    /// With subject: "DO NOT REPLY : Forwarding Response [{original subject}]"
    /// 
    /// FOLDER STRUCTURE:
    /// - Source: Configured Outlook folder (dirpathVoteCollection)
    /// - Archive: Source folder\AddSupp_Vote\
    /// 
    /// LOGGING:
    /// Uses Serilog via CommonUtilities.Logger for structured logging:
    /// - Information: Vote processing start/complete, individual vote processing
    /// - Debug: Folder navigation, item forwarding, archive operations
    /// - Verbose: Subfolder navigation details
    /// - Error: Processing failures with full exception details
    /// 
    /// Log entries include structured parameters for easy filtering:
    /// - {VoteType}: "Accepted" or "Rejected"
    /// - {Sender}: Email sender name
    /// - {Subject}: Email subject line
    /// - {ItemCount}: Number of items found/processed
    /// </summary>
    public class Processor
    {
        /// <summary>
        /// Main processing method that collects and forwards voting responses.
        /// 
        /// Processing steps (all logged):
        /// 1. Initialize Outlook connection
        /// 2. Navigate to source folder
        /// 3. Scan for voting response emails
        /// 4. Forward matches to OGA staff
        /// 5. Archive processed emails
        /// </summary>
        /// <param name="dirPath">Outlook folder path to monitor for voting responses</param>
        /// <param name="verbose">Verbose mode flag ("y" for diagnostic output)</param>
        /// <param name="logDir">Directory for log files</param>
        /// <returns>Number of voting responses processed</returns>
        public int Process(string dirPath, string verbose, string logDir)
        {
            int itemsProcessed = 0;

            CommonUtilities.Logger?.Information("Starting vote collection processing");
            CommonUtilities.Logger?.Debug("Folder path: {DirPath}", dirPath);

            // Initialize Outlook application
            Outlook.Application outlookApp = new Outlook.Application();
            Outlook.NameSpace outlookNs = outlookApp.GetNamespace("MAPI");
            CommonUtilities.Logger?.Debug("Outlook application initialized");

            // Navigate to source and archive folders
            Outlook.MAPIFolder currentFolder = GetCurrentFolder(outlookNs, dirPath);
            Outlook.MAPIFolder oldFolder = currentFolder.Folders["AddSupp_Vote"];

            int currentItem = currentFolder.Items.Count;
            CommonUtilities.Logger?.Information("Found {ItemCount} items in folder", currentItem);

            // Process items from last to first (to avoid index shifting when moving)
            while (currentItem > 0)
            {
                try
                {
                    Outlook.MailItem mailItem = currentFolder.Items[currentItem] as Outlook.MailItem;
                    if (mailItem != null)
                    {
                        string subject = mailItem.Subject;

                        // Check if this is a voting response
                        if (subject.Contains("Accepted:") || subject.Contains("Rejected:"))
                        {
                            string voteType = subject.Contains("Accepted:") ? "Accepted" : "Rejected";
                            CommonUtilities.Logger?.Information("Processing {VoteType} vote from {Sender}: {Subject}",
                           voteType, mailItem.SenderName, subject);

                            // Forward the voting response to OGA staff
                            var outMail = mailItem.Forward();
                            outMail.Recipients.Add("emily.driskell@nih.gov");
                            outMail.Recipients.Add("jonesni@mail.nih.gov");
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
        /// Logs navigation progress at Debug/Verbose levels.
        /// </summary>
        /// <param name="ns">The Outlook namespace</param>
        /// <param name="dirPath">The folder path (e.g., "Public Folders\All Public Folders\NCI\GAB")</param>
        /// <returns>The MAPIFolder at the specified path</returns>
        private Outlook.MAPIFolder GetCurrentFolder(Outlook.NameSpace ns, string dirPath)
        {
            CommonUtilities.Logger?.Debug("Navigating to folder: {Path}", dirPath);

            string[] dirs = dirPath.Split(new char[] { '\\' });
            Outlook.MAPIFolder folder = ns.Folders[dirs[0]];
            for (int i = 1; i < dirs.Length; i++)
            {
                if (!string.IsNullOrEmpty(dirs[i]))
                {
                    folder = folder.Folders[dirs[i]];
                    CommonUtilities.Logger?.Verbose("Entered subfolder: {Folder}", dirs[i]);
                }
            }
            return folder;
        }
    }
}
