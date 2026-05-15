using System;
using System.Data.SqlClient;
using CommonUtilties;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace AddSuppProd
{
    /// <summary>
    /// Processor class for Administrative Supplement Production.
    /// 
    /// PURPOSE:
    /// Processes emails from the NCIOGASupplements public folder by moving
    /// them to an archive folder after they have been handled. This is part
    /// of the supplement workflow that tracks supplement-related correspondence.
    /// 
    /// ORIGINAL SOURCE: Migrated from add_supp_prod.vbs
    /// 
    /// FOLDER STRUCTURE:
    /// - Source: Public Folders\...\NCIOGASupplements\
    /// - Archive: Public Folders\...\NCIOGASupplements\old\
    /// 
    /// PROCESSING LOGIC:
    /// 1. Opens the configured Outlook public folder
    /// 2. Iterates through all mail items in the folder
    /// 3. Moves each processed item to the "old" subfolder
    /// 4. Logs any errors that occur during processing
    /// 
    /// LOGGING:
    /// Uses Serilog via CommonUtilities.Logger for structured logging:
    /// - Information: Processing start/complete, item counts
    /// - Debug: Folder navigation, database connection, individual item processing
    /// - Verbose: Subfolder navigation details
    /// - Error: Processing failures with full exception details
    /// 
    /// Log entries include structured parameters for easy filtering:
    /// - {DirPath}: Outlook folder path being processed
    /// - {ItemCount}: Number of items found in folder
    /// - {Subject}: Email subject line (at Debug level)
    /// - {Count}: Final count of processed items
    /// 
    /// NOTE: The actual supplement categorization and document extraction
    /// is handled by the add_supp_prod.vbs logic which has been partially
    /// migrated. This processor focuses on the archive/cleanup portion.
    /// </summary>
    public class Processor
    {
        /// <summary>
        /// Main processing method that moves supplement emails to archive.
        /// Logs all operations using Serilog structured logging.
        /// </summary>
        /// <param name="con">SQL Server database connection</param>
        /// <param name="dirPath">Outlook folder path (e.g., "Public Folders\...\NCIOGASupplements")</param>
        /// <param name="outDir">Output directory for extracted files (not currently used)</param>
        /// <param name="verbose">Verbose mode flag ("y" for diagnostic output)</param>
        /// <param name="logDir">Directory for log files</param>
        /// <returns>Number of items successfully processed</returns>
        public int Process(SqlConnection con, string dirPath, string outDir, string verbose, string logDir)
        {
            int itemsProcessed = 0;

            CommonUtilities.Logger?.Information("Starting supplement production processing");
            CommonUtilities.Logger?.Debug("Folder path: {DirPath}", dirPath);

            // Initialize Outlook application
            Outlook.Application outlookApp = new Outlook.Application();
            Outlook.NameSpace outlookNs = outlookApp.GetNamespace("MAPI");
            CommonUtilities.Logger?.Debug("Outlook application initialized");

            con.Open();
            CommonUtilities.Logger?.Debug("Database connection opened");

            // Navigate to the source folder and get the archive folder
            Outlook.MAPIFolder currentFolder = GetCurrentFolder(outlookNs, dirPath);
            Outlook.MAPIFolder oldFolder = currentFolder.Folders["old"];

            int itemToProcess = currentFolder.Items.Count;
            CommonUtilities.Logger?.Information("Found {ItemCount} items to process", itemToProcess);

            // Process items from last to first (to avoid index shifting when moving)
            while (itemToProcess > 0)
            {
                try
                {
                    Outlook.MailItem currentItem = currentFolder.Items[itemToProcess] as Outlook.MailItem;
                    if (currentItem != null)
                    {
                        CommonUtilities.Logger?.Debug("Processing item: {Subject}", currentItem.Subject);

                        // Move the item to archive
                        currentItem.Move(oldFolder);
                        itemsProcessed++;

                        CommonUtilities.Logger?.Debug("Item moved to archive");
                    }
                }
                catch (Exception ex)
                {
                    CommonUtilities.Logger?.Error(ex, "Error processing item {ItemNumber}", itemToProcess);
                    Program.WriteLog($"Error processing item {itemToProcess}", ex.Message, DateTime.Now, logDir);
                }
                itemToProcess--;
            }

            con.Close();
            CommonUtilities.Logger?.Information("Processing complete. {Count} items archived", itemsProcessed);

            return itemsProcessed;
        }

        /// <summary>
        /// Navigates to an Outlook MAPI folder using a backslash-separated path.
        /// Logs navigation progress at Debug/Verbose levels.
        /// </summary>
        /// <param name="ns">The Outlook namespace</param>
        /// <param name="dirPath">The folder path (e.g., "Public Folders\All Public Folders\NCI\GAB\NCIOGASupplements")</param>
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
