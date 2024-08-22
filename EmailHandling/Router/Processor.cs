using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Exception = System.Exception;
using Outlook = Microsoft.Office.Interop.Outlook;

using System.Management;
using System.Security.Cryptography;
using System.Data;
using System.Threading;
using CommonUtilties;

namespace Router
{
    public class Processor
    {
        public static string v_SenderID { get; private set; }

        // Used by tests
        public Dictionary<string,string> emailsSentThisSession { get; private set; }

        public Processor()
        {
            emailsSentThisSession = new Dictionary<string, string>();
        }

        public int Process(string dirPath, SqlConnection con, string verbose, string debug, int routingBreakDuration)
        {
            int itemsProcessedCount = 0;
            emailsSentThisSession.Clear();

            CommonUtilities.ShowDiagnosticIfVerbose("Here we go ...", verbose);
            Outlook.Application oApp = new Outlook.Application();
            CommonUtilities.ShowDiagnosticIfVerbose("Created the outlook object.", verbose);
            Outlook.NameSpace oNS = oApp.GetNamespace("MAPI");
            oNS.Logon("", "", false, true);
            CommonUtilities.ShowDiagnosticIfVerbose($"Logged on to Outlook.", verbose);

            CommonUtilities.ShowDiagnosticIfVerbose($"Opening SQL connection ...", verbose);
            con.Open();
            CommonUtilities.ShowDiagnosticIfVerbose($"SQL connection opened.", verbose);

            char sepchar = '\\';

            CommonUtilities.ShowDiagnosticIfVerbose($"dirpath: {dirPath}", verbose);
            //Parse inputstr and Navigate to the folder
            if (!string.IsNullOrWhiteSpace(dirPath))
            {
                var dirs = dirPath.Split(sepchar);
                var i = 0;
                CommonUtilities.ShowDiagnosticIfVerbose($"Setting objNS.Folders to {dirs[0]}", verbose);
                Outlook.MAPIFolder startingFolder = null;

                Outlook.MAPIFolder currentFolder = oNS.Folders[dirs[0]];
                bool first = true;
                foreach (var dir in dirs)
                {
                    if (first || string.IsNullOrWhiteSpace(dir))
                    {
                        first = false;
                        continue;
                    }
                    currentFolder = currentFolder.Folders[dir];       // All Public Folders could not be found
                }
                CommonUtilities.ShowDiagnosticIfVerbose("Finished stepping through CFolder xarray", verbose);

                Outlook.MAPIFolder oldFolder = currentFolder.Folders["Old emails"];

                CommonUtilities.ShowDiagnosticIfVerbose("went to Old emails", verbose);
                CommonUtilities.ShowDiagnosticIfVerbose($"Mail count={currentFolder.Items.Count}", verbose);

                var itemCount = currentFolder.Items.Count;      // itmscncnt
                List<MailItem> eachEmailToProcess = new List<MailItem>();
                foreach(object item in currentFolder.Items)     // fails here
                {
                    Outlook.MailItem mailItem = item as Outlook.MailItem;
                    if (mailItem != null)
                        eachEmailToProcess.Add(mailItem);
                    else
                        CommonUtilities.ShowDiagnosticIfVerbose($"skipping a non mail item ...", verbose);
                }
                CommonUtilities.ShowDiagnosticIfVerbose($"staging email list count={eachEmailToProcess.Count}", verbose);

                int itemsToProcess = 0;
                int itemsProcessed = 0;

                CommonUtilities.ShowDiagnosticIfVerbose($"****************** starting ********************", verbose);

                foreach (var item in eachEmailToProcess)
                {
                    CommonUtilities.ShowDiagnosticIfVerbose($" ", verbose);
                    Outlook.MailItem currentItem = item as Outlook.MailItem;
                    CommonUtilities.ShowDiagnosticIfVerbose($"Item : {currentItem.ToString()}", verbose);

                    // TODO refactor these variable names
                    var v_SubLine = currentItem.Subject;
                    var v_Body = currentItem.Body;

                    CommonUtilities.ShowDiagnosticIfVerbose($"Subject : {v_SubLine}", verbose);
                    //CommonUtilities.ShowDiagnosticIfVerbose($"Body : {v_Body}", verbose);

                    bool failedToProcess = false;
                    string exceptionType = string.Empty;
                    try
                    {
                        HandleSingleEmail(currentItem, v_SubLine, v_Body, verbose, con, debug);
                    } catch(Exception ex)
                    {
                        failedToProcess = true;
                        var _logMessage = $"Error Occured! => EmailSender:{v_SenderID}; Subjectline : {v_SubLine}; Recieved Date: {currentItem.ReceivedTime}";
                        var _errorMessage = $"Error Type : {ex.GetType().FullName}, Error Message: {ex.Message} , Error Stack: {ex.StackTrace}";
                        var _endTimeStamp = DateTime.Now;
                        int _forAppending = 8;
                        CommonUtilities.WriteLog(_forAppending, _logMessage, _errorMessage, _endTimeStamp);

                        RaiseErrorToAdmin(currentItem, "Error Occured! PROD eMailRouter vbs", _errorMessage);
                    }
                    if (failedToProcess && exceptionType.ToLower().Contains("comexception"))
                    {
                        StartService("ClickToRunSvc");
                    }

                    itemCount++;
                    CommonUtilities.ShowDiagnosticIfVerbose("Incrementing count", verbose);
                    CommonUtilities.ShowDiagnosticIfVerbose($"Old folder: {oldFolder}", verbose);
                    var result = currentItem.Move(oldFolder);
                    Thread.Sleep(routingBreakDuration);

                    CommonUtilities.ShowDiagnosticIfVerbose("current Item moved", verbose);
                    CommonUtilities.ShowDiagnosticIfVerbose("**************************************************************************", verbose);

                    itemsProcessedCount++;

                    if (itemsProcessedCount >= 50)
                    {
                        var errorMessage1 = "Warning! PROD eMailRouter vbs has processed 50 mail items in one instance!";
                        var errorMessage2 = "Hello Admin, 50 items have been processed in one instance and the application is now exiting. Please check whether there is duplicate items processing.";
                        EmailMe(errorMessage1, errorMessage2);
                    }
                }
            }

            return itemsProcessedCount;
        }

        /// <summary>
        /// Override this method for testing.
        /// </summary>
        /// <param name="mailItem"></param>
        /// <returns></returns>
        protected virtual Dictionary<string,string> Send(MailItem mailItem)
        {
            mailItem.Send();

            return null;
        }

        private static bool EmailMe(string subject, string bodyMessage)
        {
            //Outlook.MailItem mailItem = (Outlook.MailItem)
            //    this.Application.CreateItem(Outlook.OlItemType.olMailItem);
            var mailItem = new Outlook.MailItem();
            mailItem.Subject = subject;
            mailItem.To = "leul.ayana@nih.gov";
            mailItem.HTMLBody = bodyMessage;
            mailItem.BodyFormat = OlBodyFormat.olFormatHTML;
            mailItem.Send();

            return true;
        }

        public static string StartService(string svcName)
        {
            string objPath = string.Format("Win32_Service.Name='{0}'", svcName);
            using (ManagementObject service = new ManagementObject(new ManagementPath(objPath)))
            {
                try
                {
                    ManagementBaseObject outParams = service.InvokeMethod("StartService",
                        null, null);
                    return (string)Enum.Parse(typeof(string),
                        outParams["ReturnValue"].ToString());
                }
                catch (Exception ex)
                {
                    if (ex.Message.ToLower().Trim() == "not found" ||
                        ex.GetHashCode() == 41149443)
                        return "Service not found.";
                    else
                        throw ex;
                }
            }
        }


        private string RaiseErrorToAdmin(MailItem currentItem, string errorMessage1, string errorMessage2)
        {
            var outmail = currentItem.Forward();
            outmail.Recipients.Add("leul.ayana@nih.gov");
            outmail.Recipients.Add("leul.ayana@nih.gov");   // NB : original system had this duplicated [sic]
            outmail.Subject = $"{errorMessage1}  >>(Subj: {currentItem.Subject} )" ;
            Send(outmail);
            return "done";
        }

        /// <summary>
        /// Overload for test objects that cannot create or pass COM objects due to registry conflicts
        /// </summary>
        /// <param name="v_SubLine"></param>
        /// <param name="v_Body"></param>
        /// <param name="verbose"></param>
        /// <param name="con"></param>
        /// <param name="debug"></param>
        public void HandleSingleEmail(string from, string v_SubLine, string v_Body, string verbose, SqlConnection con, string debug)
        {
            var newMail = new MailItem();
            //newMail.SenderEmailAddress = from;    // won't allow setting, try moving this around later

            newMail.Subject = v_SubLine;
            newMail.Body = v_Body;
            HandleSingleEmail(newMail, v_SubLine, v_Body, verbose, con, debug);
        }

        public void HandleSingleEmail(MailItem currentItem, string v_SubLine, string v_Body, string verbose, SqlConnection con, string debug)
        {
            var _dBugEmail = "leul.ayana@nih.gov";
            var _eGrantsDevEmail = "eGrantsDev@mail.nih.gov";
            var _eGrantsTestEmail = "eGrantsTest1@mail.nih.gov";
            var _eGrantsStageEmail = "eGrantsStage@mail.nih.gov";
            var _eFileEmail = "efile@mail.nih.gov";
            var _nciGrantsPostAwardEmail = "NCIGrantsPostAward@nih.gov";

            if (!v_SubLine.ToLower().Contains("undeliverable: "))
            {
                v_SenderID = GetSenderId(currentItem);
                CommonUtilities.ShowDiagnosticIfVerbose($"Sender : {v_SenderID}", verbose);

                if (v_SubLine.Contains("eSNAP Received at NIH") || v_SubLine.Contains("eRA Commons: RPPR for Grant"))
                {
                    if (v_SubLine.Contains("submitted to NIH with a Non-Compliance"))
                    {
                        //(1) load into eGrants
                        //---- IMP: STRIP SPACES FROM CATEGORY NAME "ERA NOTIFICATION"
                        var replysubj = $"category=eRANotification, sub=RPPR Non-Compliance, extract=1,{v_SubLine}";
                        CommonUtilities.ShowDiagnosticIfVerbose($"Found : {v_SubLine}", verbose);
                        CommonUtilities.ShowDiagnosticIfVerbose($"replysubj : {replysubj}", verbose);
                        var outmail = currentItem.Forward();
                        if (debug == "n")
                        {
                            outmail.Recipients.Add(_eFileEmail);
                            outmail.Recipients.Add(_eGrantsDevEmail);
                            outmail.Recipients.Add(_eGrantsTestEmail);
                            outmail.Recipients.Add(_eGrantsStageEmail);
                            outmail.Subject = replysubj;
                            Send(outmail);
                        }
                        else
                        {
                            outmail.Recipients.Add(_dBugEmail);
                            outmail.Recipients.Add(_eGrantsDevEmail);
                            outmail.Subject = replysubj;
                            Send(outmail);
                        }

                    } // end submitted to NIH with a Non-Compliance

                    //(2) forward to Bryan and Nicole
                    var outmail2 = currentItem.Forward();
                    if (debug == "n")
                    {
                        //outmail.Recipients.Add("leul.ayana@nih.gov");
                        outmail2.Recipients.Add("jonesni@mail.nih.gov");
                        outmail2.Recipients.Add("bakerb@mail.nih.gov");
                        outmail2.Recipients.Add("edward.mikulich@nih.gov");
                        Send(outmail2);
                    }
                    else
                    {
                        outmail2.Recipients.Add(_dBugEmail);
                        outmail2.Recipients.Add(_eGrantsDevEmail);
                        Send(outmail2);
                    }
                }
                else if (v_SubLine.Contains("IC ACTION REQUIRED - Relinquishing Statement"))
                {
                    var outmail2 = currentItem.Forward();
                    if (debug == "n")
                    {
                        //outmail.Recipients.Add("leul.ayana@nih.gov");
                        outmail2.Recipients.Add("emily.driskell@nih.gov");
                        outmail2.Recipients.Add("dvellaj@mail.nih.gov");
                        outmail2.Recipients.Add("edward.mikulich@nih.gov");
                        Send(outmail2);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"Found subject : {v_SubLine}", verbose);
                        outmail2.Recipients.Add(_dBugEmail);
                        outmail2.Recipients.Add(_eGrantsDevEmail);
                        Send(outmail2);
                    }
                }
                else if (v_SubLine.Contains(" Supplement Requested through "))
                {
                    CommonUtilities.ShowDiagnosticIfVerbose($"Found subject : {v_SubLine}", verbose);
                    var outmail2 = currentItem.Forward();
                    if (debug == "n")
                    {
                        outmail2.Recipients.Add("NCIOGASupplements@mail.nih.gov");
                        Send(outmail2);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"Found subject : {v_SubLine}", verbose);
                        outmail2.Recipients.Add(_dBugEmail);
                        outmail2.Recipients.Add(_eGrantsDevEmail);
                        Send(outmail2);
                    }
                }
                else if (v_SubLine.Contains(" FCOI ") && !v_SubLine.StartsWith("Automatic reply:"))
                {
                    string applId = string.Empty;
                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        applId = GetApplId(CommonUtilities.RemoveSpaceCharacters(v_SubLine), con);
                        CommonUtilities.ShowDiagnosticIfVerbose($"FCOI => applid : {applId}", verbose);
                    }
                    string p_SpecEmail = String.Empty;
                    string b_SpecEmail = String.Empty;
                    if (!string.IsNullOrWhiteSpace(applId))
                    {
                        var queryText = $"sp_getOfficersEmailForGrantNum";
                        using (SqlCommand command = new SqlCommand(queryText, con))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@APPLID", applId));
                            command.Parameters.Add(new SqlParameter("@OffCode", "SPEC"));
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                            while (reader.Read())
                            {
                                // MLH : was an earlier vbscript reference to {reader["ABC"]} but I don't see any field with that name returnin from this sproc or in the code
                                p_SpecEmail = $"{reader["Email_address_p"]}";
                                b_SpecEmail = $"{reader["Email_address_b"]}";
                                CommonUtilities.ShowDiagnosticIfVerbose($"Return from poroc (SPEC EMAIL)=>{p_SpecEmail}", verbose);
                                CommonUtilities.ShowDiagnosticIfVerbose($"Return from poroc (BACKUP_SPEC EMAIL)=>{b_SpecEmail}", verbose);
                            }
                        }
                    }
                    }
                    var outmail2 = currentItem.Forward();

                    if (debug == "n")
                    {
                        outmail2.Recipients.Add("jonesni@mail.nih.gov");
                        outmail2.Recipients.Add("bakerb@mail.nih.gov");
                        outmail2.Recipients.Add("dvellaj@mail.nih.gov");
                        outmail2.Recipients.Add("agyemann@mail.nih.gov");
                        outmail2.Recipients.Add("eugenia.chester@nih.gov");
                        outmail2.Recipients.Add("emily.driskell@nih.gov");
                        // if they're not equal, send to both
                        if (!string.IsNullOrWhiteSpace(p_SpecEmail) && !string.IsNullOrWhiteSpace(b_SpecEmail)
                            && !p_SpecEmail.Equals(b_SpecEmail, StringComparison.CurrentCultureIgnoreCase))
                        {
                            outmail2.Recipients.Add(p_SpecEmail);
                            outmail2.Recipients.Add(b_SpecEmail);
                        }
                        // they do equal, just send to p specEmail
                        else if (!string.IsNullOrWhiteSpace(p_SpecEmail) && !string.IsNullOrWhiteSpace(b_SpecEmail)
                            && p_SpecEmail.Equals(b_SpecEmail, StringComparison.CurrentCultureIgnoreCase))
                        {
                            outmail2.Recipients.Add(p_SpecEmail);
                        }
                        else if (!string.IsNullOrWhiteSpace(p_SpecEmail) && string.IsNullOrWhiteSpace(b_SpecEmail))
                        {
                            outmail2.Recipients.Add(p_SpecEmail);
                        }
                        else if (!string.IsNullOrWhiteSpace(b_SpecEmail) && string.IsNullOrWhiteSpace(p_SpecEmail))
                        {
                            outmail2.Recipients.Add(b_SpecEmail);
                        }
                        Send(outmail2);
                    }
                    else
                    {
                        outmail2.Recipients.Add(_dBugEmail);
                        outmail2.Recipients.Add(_eGrantsDevEmail);
                        var replysubj = string.Empty;
                        // if they're not equal, send to both
                        if (!string.IsNullOrWhiteSpace(p_SpecEmail) && !string.IsNullOrWhiteSpace(b_SpecEmail)
                            && !p_SpecEmail.Equals(b_SpecEmail, StringComparison.CurrentCultureIgnoreCase))
                        {
                            CommonUtilities.ShowDiagnosticIfVerbose($"P={p_SpecEmail}B={b_SpecEmail}", verbose);
                        }
                        // they do equal, just send to p specEmail
                        else if (!string.IsNullOrWhiteSpace(p_SpecEmail) && !string.IsNullOrWhiteSpace(b_SpecEmail)
                            && p_SpecEmail.Equals(b_SpecEmail, StringComparison.CurrentCultureIgnoreCase))
                        {
                            CommonUtilities.ShowDiagnosticIfVerbose($"P={p_SpecEmail}", verbose);
                        }
                        else if (!string.IsNullOrWhiteSpace(p_SpecEmail) && string.IsNullOrWhiteSpace(b_SpecEmail))
                        {
                            CommonUtilities.ShowDiagnosticIfVerbose($"P={p_SpecEmail}", verbose);
                        }
                        else if (!string.IsNullOrWhiteSpace(b_SpecEmail) && string.IsNullOrWhiteSpace(p_SpecEmail))
                        {
                            CommonUtilities.ShowDiagnosticIfVerbose($"B={b_SpecEmail}", verbose);
                        }
                        Send(outmail2);
                    }
                }
                //---- IMP: STRIP SPACES FROM CATEGORY NAME "ERA NOTIFICATION"	
                else if (v_SubLine.Contains("No Cost Extension Submitted"))
                {
                    var replysubj = $"category=eRANotification, sub=No Cost Extension, extract=1,{currentItem.Subject}";
                    CommonUtilities.ShowDiagnosticIfVerbose($"FOUND->{v_SubLine}", verbose);
                    CommonUtilities.ShowDiagnosticIfVerbose($"reply : {replysubj}", verbose);
                    var outmail = currentItem.Forward();
                    if (debug == "n")
                    {
                        outmail.Recipients.Add(_eFileEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Recipients.Add(_eGrantsTestEmail);
                        outmail.Recipients.Add(_eGrantsStageEmail);
                        //-----------ADD THE FOLLOWING FOR DEVELOPMENT TIER	AS NEEDED BASIS					
                        //outmail.Recipients.Add("leul.ayana@nih.gov")
                        outmail.Subject = replysubj;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"reply : {replysubj}", verbose);
                        //                              outmail.Recipients.Add(_dBugEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Subject = replysubj;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("Change of Institution request for Grant"))
                {
                    var replysubj = currentItem.Subject;
                    var outmail = currentItem.Forward();
                    if (debug == "n")
                    {
                        outmail.Recipients.Add("dvellaj@mail.nih.gov");
                        outmail.Recipients.Add("emily.driskell@nih.gov");
                        outmail.Recipients.Add("edward.mikulich@nih.gov");
                        outmail.Subject = replysubj;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"FOUND->{v_SubLine}", verbose);
                        outmail.Recipients.Add(_dBugEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Subject = replysubj;
                        Send(outmail);
                    }
                }
                else if (v_SenderID.ToLower().Contains("public"))
                {
                    CommonUtilities.ShowDiagnosticIfVerbose($"Found a public access email", verbose);
                    // get the appl id from the grant number in the subject line
                    // example: PASC: 5U24CA213274 - 08 - RUDIN, CHARLES M
                    // example2: Compliant PASC: 5R01CA258784 - 04 - SEN, TRIPARNA
                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"FOUND subject ->{v_SubLine}", verbose);
                        string[] tokens = v_SubLine.Split(new[] { ": " }, StringSplitOptions.None);
                        var secondPart = tokens[1].Trim();
                        CommonUtilities.ShowDiagnosticIfVerbose($"Second part : {secondPart}", verbose);

                        var subCat = string.Empty;
                        if (v_SubLine.ToLower().Contains("compliant"))
                        {
                            CommonUtilities.ShowDiagnosticIfVerbose($"Found compliant", verbose);
                            subCat = "Compliant";
                        }
                        else
                        {
                            CommonUtilities.ShowDiagnosticIfVerbose($"Found non compliant", verbose);
                        }

                        string[] tokens2 = secondPart.Split(new[] { " - " }, StringSplitOptions.None);
                        var middle = tokens2[0];
                        CommonUtilities.ShowDiagnosticIfVerbose($"Isolated middle part :{middle}", verbose);

                        var applId = GetApplId(RemoveSpCharacters(middle), con);
                        CommonUtilities.ShowDiagnosticIfVerbose($"appl id :{applId}", verbose);

                        var replySubj = $"category=PublicAccess, sub={subCat}, applid={applId}, extract=1, {currentItem.Subject}";

                        CommonUtilities.ShowDiagnosticIfVerbose($"dBugEmail :{_dBugEmail}", verbose);
                        CommonUtilities.ShowDiagnosticIfVerbose($"eGrantsDevEmail :{_eGrantsDevEmail}", verbose);
                        CommonUtilities.ShowDiagnosticIfVerbose($"replySubj :{replySubj}", verbose);

                        var outmail = currentItem.Forward();
                        if (debug == "n")
                        {
                            outmail.Recipients.Add(_eFileEmail);
                            outmail.Recipients.Add(_eGrantsDevEmail);
                            outmail.Recipients.Add(_eGrantsTestEmail);
                            outmail.Recipients.Add(_eGrantsStageEmail);
                            //===>STOPPING THIS UNTILL FIX IT/5/30 started
                            outmail.Subject = replySubj;
                            Send(outmail);
                        }
                        else
                        {
                            CommonUtilities.ShowDiagnosticIfVerbose($"reply : {replySubj}", verbose);
                            outmail.Recipients.Add(_dBugEmail);
                            outmail.Recipients.Add(_eGrantsDevEmail);
                            outmail.Subject = replySubj;
                            Send(outmail);
                        }
                        CommonUtilities.ShowDiagnosticIfVerbose($"done w / handling a public email", verbose);
                    }
                }
                else if (v_SubLine.Contains("JIT Request for Grant"))
                {
                    var replySubj = $"category=JIT Info, sub=Reminder, extract=1, {currentItem.Subject}";
                    var outmail = currentItem.Forward();
                    if (debug == "n")
                    {
                        outmail.Recipients.Add(_eFileEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Recipients.Add(_eGrantsTestEmail);
                        outmail.Recipients.Add(_eGrantsStageEmail);
                        outmail.Subject = replySubj;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                        outmail.Recipients.Add(_dBugEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Subject = replySubj;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("JIT Documents Have Been Submitted for Grant"))
                {
                    var replySubj = $"category=eRA Notification, sub=JIT Submitted, extract=1, {currentItem.Subject}";
                    var outmail = currentItem.Forward();
                    if (debug == "n")
                    {
                        outmail.Recipients.Add(_eFileEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Recipients.Add(_eGrantsTestEmail);
                        outmail.Recipients.Add(_eGrantsStageEmail);
                        //outmail.Recipients.Add("leul.ayana@nih.gov")
                        outmail.Subject = replySubj;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                        outmail.Recipients.Add(_dBugEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Subject = replySubj;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("NIH Automated Email: ACTION REQUIRED - Overdue Progress Report for Grant"))
                {
                    // July 2024: Per OGA (Lisa Vytlacil) no changes are needed. Leave upload criteria as is.
                    CommonUtilities.ShowDiagnosticIfVerbose("Very old email detected and tagged as NIH Automated Email: ACTION REQUIRED - Overdue Progress Report for Grant", verbose);

                    var replySubj = string.Empty;
                    if (v_SubLine.Contains(" R15 "))
                        replySubj = $"category=eRANotification, sub=Late Progress Report, extract=1, {currentItem.Subject}";
                    CommonUtilities.ShowDiagnosticIfVerbose($"Current subject : {currentItem.Subject}", verbose);
                    CommonUtilities.ShowDiagnosticIfVerbose($"Reply subject :  {replySubj}", verbose);
                    var outmail = currentItem.Forward();
                    if (debug == "n")
                    {
                        outmail.Recipients.Add(_eFileEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Recipients.Add(_eGrantsTestEmail);
                        outmail.Recipients.Add(_eGrantsStageEmail);
                        //outmail.Recipients.Add("leul.ayana@nih.gov")
                        outmail.Subject = replySubj;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                        outmail.Recipients.Add(_dBugEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Subject = replySubj;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("Expiring Funds") || v_SubLine.Contains("EXPIRING FUNDS-"))
                {
                    //Only attached document has to be extracted so many make Body=""
                    var replySubj = $"category=Closeout, extract=2, {currentItem.Subject}";
                    CommonUtilities.ShowDiagnosticIfVerbose($"Reply subject : {replySubj}", verbose);
                    var outmail = currentItem.Forward();
                    if (debug == "n")
                    {
                        outmail.Recipients.Add(_eFileEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Recipients.Add(_eGrantsTestEmail);
                        outmail.Recipients.Add(_eGrantsStageEmail);
                        outmail.Subject = replySubj;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                        outmail.Recipients.Add(_dBugEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Subject = replySubj;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("Prior Approval: "))
                {
                    var outmail = currentItem.Forward();
                    if (debug == "n")
                    {
                        outmail.Recipients.Add(_nciGrantsPostAwardEmail);
                        Send(outmail);
                    }
                    else
                    {
                        outmail.Recipients.Add(_dBugEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("FFR NOTIFICATION : REJECTED"))
                {
                    if (v_SubLine.ToLower().Contains("re: ffr notification") || v_SubLine.ToLower().Contains("fw: ffr notification"))
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                    }
                    else
                    {
                        var replySubj = $"category=Notification, sub=FFR Rejection, extract=1, {currentItem.Subject}";
                        var outmail = currentItem.Forward();
                        if (debug == "n")
                        {
                            outmail.Recipients.Add(_eFileEmail);
                            outmail.Recipients.Add(_eGrantsDevEmail);
                            outmail.Recipients.Add(_eGrantsTestEmail);
                            outmail.Recipients.Add(_eGrantsStageEmail);
                            outmail.Subject = replySubj;
                            Send(outmail);
                        }
                        else
                        {
                            outmail.Recipients.Add(_dBugEmail);
                            outmail.Recipients.Add(_eGrantsDevEmail);
                            outmail.Subject = replySubj;
                            Send(outmail);
                        }
                    }
                }
                else if (v_SubLine.Contains("eRA Commons: The Final RPPR - Additional Materials for Award"))
                {
                    if (v_SubLine.Contains("re: eRA Commons: The Final RPPR ") || v_SubLine.Contains("fw: eRA Commons: The Final RPPR "))
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                    }
                    else
                    {
                        var replySubject = $"category=FRAM: Request, sub=The Final RPPR, extract=1, {currentItem.Subject}";
                        var outmail = currentItem.Forward();
                        if (debug == "n")
                        {
                            outmail.Recipients.Add(_eFileEmail);
                            outmail.Recipients.Add(_eGrantsDevEmail);
                            outmail.Recipients.Add(_eGrantsTestEmail);
                            outmail.Recipients.Add(_eGrantsStageEmail);
                            outmail.Subject = replySubject;
                            Send(outmail);
                        }
                        else
                        {
                            CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                            outmail.Recipients.Add(_dBugEmail);
                            outmail.Recipients.Add(_eGrantsDevEmail);
                            outmail.Subject = replySubject;
                            Send(outmail);
                        }
                    }
                }
                else if (v_SubLine.Contains("RPPR Unobligated Balance: Additional Information Needed"))
                {
                    CommonUtilities.ShowDiagnosticIfVerbose($"Handlin RPPR for unobligated balance w/ this subject :  {v_SubLine}", verbose);
                    var applid = string.Empty;
                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        var grantid = v_SubLine.Trim().Split(' ')[7];
                        CommonUtilities.ShowDiagnosticIfVerbose($"Grant Id step 1 result : {grantid}", verbose);
                        if (!string.IsNullOrWhiteSpace(grantid))
                        {
                            applid = GetApplId(RemoveSpCharacters(grantid), con);
                            CommonUtilities.ShowDiagnosticIfVerbose($"Appl Id step 2 result : {applid}", verbose);
                        }
                    }

                    var replySubject = $"applid={applid}, category=Correspondence, sub=RPPR Unobligated Balance, extract=1, {currentItem.Subject}";
                    var outmail = currentItem.Forward();
                    if (debug == "n")
                    {
                        outmail.Recipients.Add(_eFileEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Recipients.Add(_eGrantsTestEmail);
                        outmail.Recipients.Add(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                        outmail.Recipients.Add(_dBugEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("eRA Commons: PRAM for Grant"))
                {
                    if (v_SubLine.Contains("re: eRA Commons: PRAM for Grant") || v_SubLine.Contains("fw: eRA Commons: PRAM for Grant"))
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                    }
                    else
                    {
                        var replySubject = $"category=PRAM: Requested, sub=PRAM for Grant, extract=1, {currentItem.Subject}";
                        var outmail = currentItem.Forward();
                        if (debug == "n")
                        {
                            //outmail.Recipients.Add(_eFileEmail);
                            outmail.Recipients.Add(_eGrantsDevEmail);
                            outmail.Recipients.Add(_eGrantsTestEmail);
                            outmail.Recipients.Add(_eGrantsStageEmail);
                            outmail.Subject = replySubject;
                            Send(outmail);
                        }
                        else
                        {
                            CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                            outmail.Recipients.Add(_dBugEmail);
                            outmail.Recipients.Add(_eGrantsDevEmail);
                            outmail.Subject = replySubject;
                            Send(outmail);
                        }
                    }
                }
                else if (v_SubLine.Contains("FRAM Requested") || v_SubLine.Contains("PRAM Requested"))
                {
                    var replySubject = string.Empty;
                    var applId = string.Empty;

                    if (v_SubLine.Contains("FRAM Requested"))
                    {
                        if (!string.IsNullOrWhiteSpace(v_SubLine))
                        {
                            applId = GetApplId(RemoveSpCharacters(v_SubLine), con);
                        }
                        replySubject = $"applid={applId}, category=FRAM, sub=Request, extract=1, {currentItem.Subject}";
                    }
                    else if (v_SubLine.Contains("PRAM Requested"))
                    {
                        if (!string.IsNullOrWhiteSpace(v_SubLine))
                        {
                            applId = GetApplId(RemoveSpCharacters(v_SubLine), con);
                        }
                        replySubject = $"applid={applId}, category=PRAM, sub=Request, extract=1, {currentItem.Subject}";
                    }

                    var outmail = currentItem.Forward();
                    if (debug == "n")
                    {
                        outmail.Recipients.Add(_eFileEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Recipients.Add(_eGrantsTestEmail);
                        outmail.Recipients.Add(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                        outmail.Recipients.Add(_dBugEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }

                }
                else if (v_SubLine.Contains("CHANGE_NOTICE_FOR") && v_SubLine.Contains("Application is withdrawn request"))
                {
                    var replySubject = string.Empty;
                    var applId = string.Empty;

                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        applId = GetApplId(RemoveSpCharacters(v_SubLine), con);
                    }
                    replySubject = $"applid={applId}, category=eRA Notification, sub=Application Withdrawn, extract=1, {currentItem.Subject}";

                    var outmail = currentItem.Forward();
                    if (debug == "n")
                    {
                        outmail.Recipients.Add(_eFileEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Recipients.Add(_eGrantsTestEmail);
                        outmail.Recipients.Add(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        //CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                        outmail.Recipients.Add(_dBugEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }

                }
                else if (v_SubLine.StartsWith("RPPR Reminder"))
                {
                    CommonUtilities.ShowDiagnosticIfVerbose($"Handlin RPPR for Reminder w/ this subject :  {v_SubLine}", verbose);
                    var replySubject = string.Empty;
                    var applId = string.Empty;

                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        applId = GetApplId(RemoveSpCharacters(v_SubLine), con);
                    }
                    CommonUtilities.ShowDiagnosticIfVerbose($"applId :  {applId}", verbose);
                    CommonUtilities.ShowDiagnosticIfVerbose($"replySubject :  {replySubject}", verbose);
                    replySubject = $"applid={applId}, category=RPPR, sub=Reminder, extract=1, {currentItem.Subject}";

                    var outmail = currentItem.Forward();
                    if (debug == "n")
                    {
                        outmail.Recipients.Add(_eFileEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Recipients.Add(_eGrantsTestEmail);
                        outmail.Recipients.Add(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        //CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                        outmail.Recipients.Add(_dBugEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("IRPPR Reminder"))
                {
                    var replySubject = string.Empty;
                    var applId = string.Empty;

                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        applId = GetApplId(RemoveSpCharacters(v_SubLine), con);
                    }
                    replySubject = $"applid={applId}, category=IRPPR, sub=Reminder, extract=1, {currentItem.Subject}";

                    var outmail = currentItem.Forward();
                    if (debug == "n")
                    {
                        outmail.Recipients.Add(_eFileEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Recipients.Add(_eGrantsTestEmail);
                        outmail.Recipients.Add(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        //CommonUtilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                        outmail.Recipients.Add(_dBugEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.ToLower().Contains("closeout action required"))
                {
                    CommonUtilities.ShowDiagnosticIfVerbose($"Hello you are closing out a thing ...", verbose);
                    var applId = string.Empty;

                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        var isolated = GetNthWord(v_SubLine, 4);
                        CommonUtilities.ShowDiagnosticIfVerbose($"Isolated : {isolated}", verbose);
                        applId = GetApplId(RemoveSpCharacters(isolated), con);
                        CommonUtilities.ShowDiagnosticIfVerbose($"Appl Id : {applId}", verbose);
                    }

                    var replySubject = $"category=closeout, sub=Past Due Documents Reminder, applid={applId}, extract=1, {currentItem.Subject}";

                    var outmail = currentItem.Forward();
                    if (debug == "n")
                    {
                        outmail.Recipients.Add(_eFileEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Recipients.Add(_eGrantsTestEmail);
                        outmail.Recipients.Add(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"FOUND -> {v_SubLine}", verbose);
                        outmail.Recipients.Add(_dBugEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    CommonUtilities.ShowDiagnosticIfVerbose($"Hello you closed out a thing", verbose);
                }
                else if (v_SubLine.ToLower().Contains("closeout program action required"))
                {
                    CommonUtilities.ShowDiagnosticIfVerbose($"Hello you are closing out a PROGRAM thing ...", verbose);
                    // example subject here : Closeout Program Action Required: 5R21CA249649-02 - Sun, Jingjing F-RPPR Acceptance Past Due
                    var applId = string.Empty;

                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        var isolated = GetNthWord(v_SubLine.Trim(), 5);
                        CommonUtilities.ShowDiagnosticIfVerbose($"Isolated : {isolated}", verbose);
                        applId = GetApplId(RemoveSpCharacters(isolated), con);
                        CommonUtilities.ShowDiagnosticIfVerbose($"Appl Id : {applId}", verbose);
                    }

                    var replySubject = $"category=closeout, sub=F-RPPR Acceptance Past Due Reminder, applid={applId}, extract=1, {currentItem.Subject}";

                    var outmail = currentItem.Forward();
                    if (debug == "n")
                    {
                        outmail.Recipients.Add(_eFileEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Recipients.Add(_eGrantsTestEmail);
                        outmail.Recipients.Add(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        CommonUtilities.ShowDiagnosticIfVerbose($"FOUND -> {v_SubLine}", verbose);
                        outmail.Recipients.Add(_dBugEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    CommonUtilities.ShowDiagnosticIfVerbose($"Hello you closed out a PROGRAM thing", verbose);
                }
                else if (v_SubLine.Contains("FFR Reminder") && v_SubLine.Contains("FFR Past Due"))
                {
                    var replySubject = string.Empty;
                    var applId = string.Empty;

                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        applId = GetApplId(RemoveSpCharacters(v_SubLine), con);
                    }
                    replySubject = $"applid={applId}, category=FFR, sub=Reminder, extract=1, {currentItem.Subject}";

                    var outmail = currentItem.Forward();
                    if (debug == "n")
                    {
                        outmail.Recipients.Add(_eFileEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Recipients.Add(_eGrantsTestEmail);
                        outmail.Recipients.Add(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        outmail.Recipients.Add(_dBugEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("ClinicalTrials.gov Results Reporting for Grant"))
                {
                    var replySubject = string.Empty;
                    var applId = string.Empty;

                    var lastFourCharacters = string.Empty;
                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        var lastWordInSubject = GetLastWord(v_SubLine);
                        lastFourCharacters = lastWordInSubject.Substring(Math.Max(0, lastWordInSubject.Length - 4));
                        applId = GetApplId(RemoveSpCharacters(v_SubLine), con);
                    }
                    replySubject = $"applid={applId}, category=CT.gov, sub=Results Reporting Reminder NCT{lastFourCharacters} , extract=1, {currentItem.Subject}";

                    var outmail = currentItem.Forward();
                    if (debug == "n")
                    {
                        outmail.Recipients.Add(_eFileEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Recipients.Add(_eGrantsTestEmail);
                        outmail.Recipients.Add(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        outmail.Recipients.Add(_dBugEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                }
                else if (v_SubLine.Contains("SBIR/STTR Foreign Risk Management"))
                {
                    CommonUtilities.ShowDiagnosticIfVerbose("handling SBIR/STTR", verbose);
                    // example body we want to snatch the appl id from :
                    // 1R43CA291415-01 (10921643) has undergone SBIR/STTR risk management assessment in accordance with the SBIR and STTR Extension Act of 2022 on 04/25/2024 12:19 PM. 

                    // get the appl id from the grant number in the subject line
                    var replySubject = string.Empty;
                    var applId = string.Empty;
                    var lastFourCharacters = string.Empty;
                    if (!string.IsNullOrWhiteSpace(v_SubLine))
                    {
                        applId = v_Body.Split(' ')[1];   // e.g. (10921643)
                        CommonUtilities.ShowDiagnosticIfVerbose($"SBIR/STTR extraction 1 : {applId}", verbose);
                        applId = applId.Replace("(", "");
                        applId = applId.Replace(")", "");
                        CommonUtilities.ShowDiagnosticIfVerbose($"SBIR/STTR extraction 2 : {applId}", verbose);
                    }
                    replySubject = $"applid={applId}, category=Funding, sub=DCI-InTh Cleared, extract=1, {currentItem.Subject}";
                    if (v_SubLine.Contains("Not Cleared"))
                        replySubject = $"applid={applId}, category=Funding, sub=DCI-InTh Not Cleared, extract=1, {currentItem.Subject}";
                    CommonUtilities.ShowDiagnosticIfVerbose($"replySubject : {replySubject}", verbose);

                    var outmail = currentItem.Forward();
                    if (debug == "n")
                    {
                        outmail.Recipients.Add(_eFileEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Recipients.Add(_eGrantsTestEmail);
                        outmail.Recipients.Add(_eGrantsStageEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    else
                    {
                        outmail.Recipients.Add(_dBugEmail);
                        outmail.Recipients.Add(_eGrantsDevEmail);
                        outmail.Subject = replySubject;
                        Send(outmail);
                    }
                    CommonUtilities.ShowDiagnosticIfVerbose("completed SBIR", verbose);
                }
                CommonUtilities.ShowDiagnosticIfVerbose("Finished handling the program type", verbose);
            }
            CommonUtilities.ShowDiagnosticIfVerbose("Done checking if it was undeliverable", verbose);
        }

        private static string GetLastWord(string inbound)
        {
            var wordz = inbound.Split(' ');
            var numberOfWords = wordz.Length;           // Ubound(wordz)
            var lastWord = wordz[numberOfWords - 1];    // Remember arrays start at index 0
            return lastWord;
        }

        private static string GetNthWord(string inbound, int number)
        {
            var wordz = inbound.Split(' ');
            var lastWord = wordz[number - 1];
            return lastWord;
        }

        private static string GetApplId(string str, SqlConnection con)
        {
            var queryText = "select dbo.Imm_fn_applid_match(  @LocalId ) as applid";
            try
            {
                using (SqlCommand command = new SqlCommand(queryText, con))
                {
                    command.Parameters.Add(new SqlParameter("@LocalId", str));
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int returnedVal = reader.GetInt32(0);
                            string applId = $"{returnedVal}";
                            return applId;
                        }
                    }
                }
                return string.Empty;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Query failed.");
                Console.WriteLine($"The string parameter for Imm_fn_applid_match was '{str}'");
                Console.WriteLine($"The query text (without inferred params) : '{queryText}'");
                throw new System.Exception($"Get Appl Id query failed. Input string : '{str}'\r\n Message: {ex.Message}");
            }
        }

        private static string RemoveSpCharacters(string text)
        {
            var result = text;
            result = result.Replace("vbLf", "vbCrLF");
            result = result.Replace(":", " ");
            result = result.Replace("/", " ");
            result = result.Replace("\\", " ");
            result = result.Replace("&", "and");
            result = result.Replace(";", " ");
            result = result.Replace("<", " ");
            result = result.Replace(">", " ");
            result = result.Replace("<<", " ");
            result = result.Replace(">>", " ");
            result = result.Replace("^", " ");
            result = result.Replace("%", " ");
            result = result.Replace("@", " ");
            result = result.Replace("'", " ");
            result = result.Replace(" ", "");
            return result;
        }

        private static string GetSenderId(Outlook.MailItem currentItem)
        {
            string id = string.Empty;

            if (string.IsNullOrWhiteSpace(currentItem.SenderEmailAddress))
                return id;

            if (currentItem.SenderEmailType.Equals("ex", StringComparison.InvariantCultureIgnoreCase))
            {
                var objectSender = currentItem.Sender;
                if (objectSender != null)
                {
                    var objectExchangeUser = currentItem.Sender.GetExchangeUser();
                    if (objectExchangeUser != null)
                    {
                        id = objectExchangeUser.Alias;
                    }
                }

                // MLH : saw this in the VB script and I don't see GetAlias in the code, interwebs, or outlook APIs
                //if (string.IsNullOrWhiteSpace(id))
                //{
                //    //id = GetAlias(currentItem);
                //}

            }
            else if (currentItem.SenderEmailType.Equals("smtp", StringComparison.InvariantCultureIgnoreCase))
            {
                id = currentItem.SenderEmailAddress;
            }

            return id;
        }






    }
}
