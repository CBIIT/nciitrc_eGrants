using System;
using System.Data.SqlClient;
using CommonUtilties;

namespace AddSuppProd
{
    /// <summary>
    /// Processor class for Administrative Supplement Production.
    /// 
    /// PURPOSE:
    /// Processes emails from the NCIOGASupplements public folder by moving
    /// them to an archive folder after they have been handled.
    /// 
    /// OUTLOOK INTEGRATION:
    /// Uses late-bound COM automation (dynamic/Activator) to control Outlook.
    /// No Primary Interop Assembly (PIA) or NuGet interop package is required at compile time.
    /// Outlook must be installed and configured on the machine where this runs.
    /// </summary>
    public class Processor
    {
        public int Process(SqlConnection con, string dirPath, string outDir, string verbose, string logDir)
        {
            int itemsProcessed = 0;

            CommonUtilities.Logger?.Information("Starting supplement production processing");
            CommonUtilities.Logger?.Information("Folder path: {DirPath}", dirPath);

            // Create Outlook application via late binding (no PIA needed)
            Type outlookType = Type.GetTypeFromProgID("Outlook.Application");
            if (outlookType == null)
            {
                CommonUtilities.Logger?.Error("Outlook is not installed or not registered on this machine");
                throw new InvalidOperationException("Outlook.Application COM class not found. Is Outlook installed?");
            }
            dynamic outlookApp = Activator.CreateInstance(outlookType);
            dynamic outlookNs = outlookApp.GetNamespace("MAPI");
            CommonUtilities.Logger?.Information("Outlook application initialized via late binding");

            con.Open();
            CommonUtilities.Logger?.Information("Database connection opened");

            // Navigate to the source folder and get the archive folder
            dynamic currentFolder = GetCurrentFolder(outlookNs, dirPath);
            dynamic oldFolder = currentFolder.Folders["old"];

            int itemToProcess = currentFolder.Items.Count;
            CommonUtilities.Logger?.Information("Found {ItemCount} items to process", itemToProcess);

            // Process items from last to first (to avoid index shifting when moving)
            while (itemToProcess > 0)
            {
                try
                {
                    dynamic currentItem = currentFolder.Items[itemToProcess];
                    if (currentItem != null)
                    {
                        CommonUtilities.Logger?.Information("Processing item: {Subject}", (string)currentItem.Subject);

                        try
                        {
                            currentItem.Move(oldFolder);
                            CommonUtilities.Logger?.Information("Item moved to archive");
                        }
                        catch (System.Runtime.InteropServices.COMException comEx) when (comEx.HResult == unchecked((int)0x80040119))
                        {
                            // Move failed - item was already copied to archive by Outlook but original remains
                            CommonUtilities.Logger?.Warning("Item copied to archive but original could not be deleted (insufficient permissions). Subject: {Subject}", (string)currentItem.Subject);
                        }

                        itemsProcessed++;
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
        /// </summary>
        private dynamic GetCurrentFolder(dynamic ns, string dirPath)
        {
            CommonUtilities.Logger?.Information("Navigating to folder: {Path}", dirPath);

            string[] dirs = dirPath.TrimEnd('\\').Split(new char[] { '\\' });
            dynamic folder = ns.Folders[dirs[0]];
            CommonUtilities.Logger?.Information("Entered root folder: {Folder}", dirs[0]);

            for (int i = 1; i < dirs.Length; i++)
            {
                if (!string.IsNullOrEmpty(dirs[i]))
                {
                    try
                    {
                        folder = folder.Folders[dirs[i]];
                        CommonUtilities.Logger?.Information("Entered subfolder: {Folder}", dirs[i]);
                    }
                    catch (Exception ex)
                    {
                        CommonUtilities.Logger?.Error(ex, "Failed to navigate to subfolder: {Folder}. Available folders listed below.", dirs[i]);
                        // Log available folders to help diagnose
                        try
                        {
                            foreach (dynamic f in folder.Folders)
                            {
                                CommonUtilities.Logger?.Warning("  Available: {FolderName}", (string)f.Name);
                            }
                        }
                        catch { }
                        throw;
                    }
                }
            }
            return folder;
        }
    }
}
