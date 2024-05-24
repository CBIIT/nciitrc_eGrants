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

            Utilities.ShowDiagnosticIfVerbose($"Opening SQL connection ...", verbose);
            con.Open();
            Utilities.ShowDiagnosticIfVerbose($"SQL connection opened.", verbose);

            char sepchar = '\\';

            var _dBugEmail = "leul.ayana@nih.gov";
            var _eGrantsDevEmail = "eGrantsDev@mail.nih.gov";
            var _eGrantsTestEmail = "eGrantsTest1@mail.nih.gov";
            var _eGrantsStageEmail = "eGrantsStage@mail.nih.gov";
            var _eFileEmail = "efile@mail.nih.gov";
            var _nciGrantsPostAwardEmail = "NCIGrantsPostAward@nih.gov";

            Utilities.ShowDiagnosticIfVerbose($"dirpath: {dirPath}", verbose);
            //Parse inputstr and Navigate to the folder
            if (!string.IsNullOrWhiteSpace(dirPath))
            {
                var dirs = dirPath.Split(sepchar);
                var i = 0;
                Utilities.ShowDiagnosticIfVerbose($"Setting objNS.Folders to {dirs[0]}", verbose);
                Outlook.MAPIFolder startingFolder = null;

                // try going directly to the folder and put it currentFolder...
                //Outlook.MAPIFolder currentFolder = oNS.Folders[dirPath];
                //		currentFolder.Name	error CS1061: 'MAPIFolder' does not contain a definition for 'Name' and no accessible extension method 'Name' accepting a first argument of type 'MAPIFolder' could be found (are you missing a using directive or an assembly reference?)	

                //oNS.Folders.GetFolder(dirPath);        // this is a vbscript only command

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
                Utilities.ShowDiagnosticIfVerbose("Finished stepping through CFolder xarray", verbose);

                // this works !!
                //foreach (var item in currentFolder.Items)
                //{
                //    Outlook.MailItem mail = item as Outlook.MailItem;
                //    Utilities.ShowDiagnosticIfVerbose($"Item : {item.ToString()}", verbose);
                //}
                //Utilities.ShowDiagnosticIfVerbose("Set", verbose);

                Outlook.MAPIFolder oldFolder = currentFolder.Folders["Old emails"];

                Utilities.ShowDiagnosticIfVerbose("went to Old emails", verbose);
                Utilities.ShowDiagnosticIfVerbose($"Mail count={currentFolder.Items.Count}", verbose);

                var itemCount = currentFolder.Items.Count;

                int itemsToProcess = 0;
                int itemsProcessed = 0;

                foreach (var item in currentFolder.Items)
                {
                    Outlook.MailItem currentItem = item as Outlook.MailItem;
                    Utilities.ShowDiagnosticIfVerbose($"Item : {currentItem.ToString()}", verbose);

                    // TODO refactor these variable names
                    var v_SubLine = currentItem.Subject;
                    var v_Body = currentItem.Body;

                    Utilities.ShowDiagnosticIfVerbose($"Subject : {v_SubLine}", verbose);
                    Utilities.ShowDiagnosticIfVerbose($"Body : {v_Body}", verbose);

                    if (!v_SubLine.ToLower().Contains("undeliverable: "))
                    {
                        var v_SenderID = GetSenderId(currentItem);
                        Utilities.ShowDiagnosticIfVerbose($"Sender : {v_SenderID}", verbose);

                        // IF(InStr(v_SubLine, "eSNAP Received at NIH") > 0)  OR(InStr(v_SubLine, "eRA Commons: RPPR for Grant ") > 0) Then
                        if (v_SubLine.Contains("eSNAP Received at NIH") || v_SubLine.Contains("eRA Commons: RPPR for Grant"))
                        {
                            //IF (InStr(v_SubLine," submitted to NIH with a Non-Compliance ") > 0) Then
                            if (v_SubLine.Contains("submitted to NIH with a Non-Compliance"))
                            {
                                //(1) load into eGrants
                                //---- IMP: STRIP SPACES FROM CATEGORY NAME "ERA NOTIFICATION"
                                var replysubj = $"category=eRANotification, sub=RPPR Non-Compliance, extract=1,{v_SubLine}";
                                Utilities.ShowDiagnosticIfVerbose($"Found : {v_SubLine}", verbose);
                                Utilities.ShowDiagnosticIfVerbose($"replysubj : {replysubj}", verbose);
                                var outmail = currentItem.Forward();
                                if (debug == "n")
                                {
                                    outmail.Recipients.Add(_eFileEmail);
                                    outmail.Recipients.Add(_eGrantsDevEmail);
                                    outmail.Recipients.Add(_eGrantsTestEmail);
                                    outmail.Recipients.Add(_eGrantsStageEmail);
                                    outmail.Subject = replysubj;
                                    outmail.Send();
                                }
                                else
                                {
                                    outmail.Recipients.Add(_dBugEmail);
                                    outmail.Recipients.Add(_eGrantsDevEmail);
                                    outmail.Subject = replysubj;
                                    outmail.Send();
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
                                outmail2.Send();
                            }
                            else
                            {
                                outmail2.Recipients.Add(_dBugEmail);
                                outmail2.Recipients.Add(_eGrantsDevEmail);
                                outmail2.Send();
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
                                outmail2.Send();
                            }
                            else
                            {
                                Utilities.ShowDiagnosticIfVerbose($"Found subject : {v_SubLine}", verbose);
                                outmail2.Recipients.Add(_dBugEmail);
                                outmail2.Recipients.Add(_eGrantsDevEmail);
                                outmail2.Send();
                            }
                        }
                        else if (!v_SubLine.Contains(" Supplement Requested through "))
                        {
                            Utilities.ShowDiagnosticIfVerbose($"Found subject : {v_SubLine}", verbose);
                            var outmail2 = currentItem.Forward();
                            if (debug == "n")
                            {
                                outmail2.Recipients.Add("NCIOGASupplements@mail.nih.gov");
                                outmail2.Send();
                            }
                            else
                            {
                                Utilities.ShowDiagnosticIfVerbose($"Found subject : {v_SubLine}", verbose);
                                outmail2.Recipients.Add(_dBugEmail);
                                outmail2.Recipients.Add(_eGrantsDevEmail);
                                outmail2.Send();
                            }
                        }
                        //ELSEIF (InStr(v_SubLine," FCOI ") > 0  AND InStr(1,v_SubLine,"Automatic reply:") = 0) Then
                        // MLH : I'm confident that InStr(1,v_SubLine,"Automatic reply:") = 0 here means that it didn't begin with "automatic reply"
                        else if (v_SubLine.Contains(" FCOI ") && !v_SubLine.StartsWith("Automatic reply:"))
                        {
                            string applId = string.Empty;
                            if (string.IsNullOrWhiteSpace(v_SubLine))
                            {
                                applId = GetApplId(Utilities.RemoveSpaceCharacters(v_SubLine), con);
                                Utilities.ShowDiagnosticIfVerbose($"FCOI => applid : {applId}", verbose);
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
                                    command.ExecuteReader();
                                    SqlDataReader reader = command.ExecuteReader();

                                    while (reader.Read())
                                    {
                                        Utilities.ShowDiagnosticIfVerbose($" Fields(ABC).value Data found creating document in OutDir Alias {reader["ABC"]} Data found creating document ", verbose);
                                        p_SpecEmail = $"{reader["Email_address_p"]}";
                                        b_SpecEmail = $"{reader["Email_address_b"]}";
                                        Utilities.ShowDiagnosticIfVerbose($"Return from poroc (SPEC EMAIL)=>{p_SpecEmail}", verbose);
                                        Utilities.ShowDiagnosticIfVerbose($"Return from poroc (BACKUP_SPEC EMAIL)=>{b_SpecEmail}", verbose);
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
                                outmail2.Send();
                            }
                            else
                            {
                                outmail2.Recipients.Add(_dBugEmail);
                                outmail2.Recipients.Add(_eGrantsDevEmail);
                                var replysubj = string.Empty;
                                // if they're not equal, send to both
                                if (!string.IsNullOrWhiteSpace(p_SpecEmail) && !string.IsNullOrWhiteSpace(b_SpecEmail)
                                    && !p_SpecEmail.Equals(b_SpecEmail, StringComparison.CurrentCultureIgnoreCase))
                                    replysubj = $"P={p_SpecEmail}B={b_SpecEmail}";
                                // they do equal, just send to p specEmail
                                else if (!string.IsNullOrWhiteSpace(p_SpecEmail) && !string.IsNullOrWhiteSpace(b_SpecEmail)
                                    && p_SpecEmail.Equals(b_SpecEmail, StringComparison.CurrentCultureIgnoreCase))
                                    replysubj = $"P={p_SpecEmail}";
                                else if (!string.IsNullOrWhiteSpace(p_SpecEmail) && string.IsNullOrWhiteSpace(b_SpecEmail))
                                    replysubj = $"P={p_SpecEmail}";
                                else if (!string.IsNullOrWhiteSpace(b_SpecEmail) && string.IsNullOrWhiteSpace(p_SpecEmail))
                                    replysubj = $"B={b_SpecEmail}";
                                Utilities.ShowDiagnosticIfVerbose($"FCOI FOUND SPEC ID->{replysubj}", verbose);
                                outmail2.Subject = replysubj;
                                outmail2.Send();
                            }
                        }
                        //---- IMP: STRIP SPACES FROM CATEGORY NAME "ERA NOTIFICATION"	
                        else if (v_SubLine.Contains("No Cost Extension Submitted"))
                        {
                            var replysubj = $"category=eRANotification, sub=No Cost Extension, extract=1,{currentItem.Subject}";
                            Utilities.ShowDiagnosticIfVerbose($"FOUND->{v_SubLine}", verbose);
                            Utilities.ShowDiagnosticIfVerbose($"reply : {replysubj}", verbose);
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
                                outmail.Send();
                            }
                            else
                            {
                                Utilities.ShowDiagnosticIfVerbose($"reply : {replysubj}", verbose);
                                //                              outmail.Recipients.Add(_dBugEmail);
                                outmail.Recipients.Add(_eGrantsDevEmail);
                                outmail.Subject = replysubj;
                                outmail.Send();
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
                                outmail.Send();
                            }
                            else
                            {
                                Utilities.ShowDiagnosticIfVerbose($"FOUND->{v_SubLine}", verbose);
                                outmail.Recipients.Add(_dBugEmail);
                                outmail.Recipients.Add(_eGrantsDevEmail);
                                outmail.Subject = replysubj;
                                outmail.Send();
                            }
                        }
                        else if (v_SenderID.ToLower().Contains("public"))
                        {
                            Utilities.ShowDiagnosticIfVerbose($"Found a public access email", verbose);
                            // get the appl id from the grant number in the subject line
                            // example: PASC: 5U24CA213274 - 08 - RUDIN, CHARLES M
                            // example2: Compliant PASC: 5R01CA258784 - 04 - SEN, TRIPARNA
                            if (!string.IsNullOrWhiteSpace(v_SubLine))
                            {
                                Utilities.ShowDiagnosticIfVerbose($"FOUND subject ->{v_SubLine}", verbose);
                                string[] tokens = v_SubLine.Split(new[] { ": " }, StringSplitOptions.None);
                                var secondPart = tokens[1].Trim();
                                Utilities.ShowDiagnosticIfVerbose($"Second part : {secondPart}", verbose);

                                var subCat = string.Empty;
                                if (v_SubLine.ToLower().Contains("compliant"))
                                {
                                    Utilities.ShowDiagnosticIfVerbose($"Found compliant", verbose);
                                    subCat = "Compliant";
                                }
                                else
                                {
                                    Utilities.ShowDiagnosticIfVerbose($"Found non compliant", verbose);
                                }

                                string[] tokens2 = secondPart.Split(new[] { " - " }, StringSplitOptions.None);
                                var middle = tokens2[0];
                                Utilities.ShowDiagnosticIfVerbose($"Isolated middle part :{middle}", verbose);

                                var applId = GetApplId(RemoveSpCharacters(middle), con);
                                Utilities.ShowDiagnosticIfVerbose($"appl id :{applId}", verbose);

                                // replysubj="category=PublicAccess, sub=" & subCat & ", applid=" & applid & ", extract=1, " & CItem.subject
                                var replySubj = $"category=PublicAccess, sub={subCat}, applid={applId}, extract=1, {currentItem.Subject}";

                                Utilities.ShowDiagnosticIfVerbose($"dBugEmail :{_dBugEmail}", verbose);
                                Utilities.ShowDiagnosticIfVerbose($"eGrantsDevEmail :{_eGrantsDevEmail}", verbose);
                                Utilities.ShowDiagnosticIfVerbose($"replySubj :{replySubj}", verbose);

                                var outmail = currentItem.Forward();
                                if (debug == "n")
                                {
                                    outmail.Recipients.Add(_eFileEmail);
                                    outmail.Recipients.Add(_eGrantsDevEmail);
                                    outmail.Recipients.Add(_eGrantsTestEmail);
                                    outmail.Recipients.Add(_eGrantsStageEmail);
                                    //===>STOPPING THIS UNTILL FIX IT/5/30 started
                                    outmail.Subject = replySubj;
                                    outmail.Send();
                                }
                                else
                                {
                                    Utilities.ShowDiagnosticIfVerbose($"reply : {replySubj}", verbose);
                                    outmail.Recipients.Add(_dBugEmail);
                                    outmail.Recipients.Add(_eGrantsDevEmail);
                                    outmail.Subject = replySubj;
                                    outmail.Send();
                                }
                                Utilities.ShowDiagnosticIfVerbose($"done w / handling a public email", verbose);
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
                                outmail.Send();
                            }
                            else
                            {
                                Utilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                                outmail.Recipients.Add(_dBugEmail);
                                outmail.Recipients.Add(_eGrantsDevEmail);
                                outmail.Subject = replySubj + "URGENT ERROR";
                                outmail.Send();
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
                                outmail.Send();
                            }
                            else
                            {
                                Utilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                                outmail.Recipients.Add(_dBugEmail);
                                outmail.Subject = replySubj;
                                outmail.Send();
                            }
                        }
                        else if (v_SubLine.Contains("NIH Automated Email: ACTION REQUIRED - Overdue Progress Report for Grant"))
                        {
                            var replySubj = string.Empty;
                            if (v_SubLine.Contains(" R15 "))
                                replySubj = $"category=eRANotification, sub=Late Progress Report, extract=1, {currentItem.Subject}";
                            Utilities.ShowDiagnosticIfVerbose($"Current subject : {currentItem.Subject}", verbose);
                            Utilities.ShowDiagnosticIfVerbose($"Reply subject :  {replySubj}", verbose);
                            var outmail = currentItem.Forward();
                            if (debug == "n")
                            {
                                outmail.Recipients.Add(_eFileEmail);
                                outmail.Recipients.Add(_eGrantsDevEmail);
                                outmail.Recipients.Add(_eGrantsTestEmail);
                                outmail.Recipients.Add(_eGrantsStageEmail);
                                //outmail.Recipients.Add("leul.ayana@nih.gov")
                                outmail.Subject = replySubj;
                                outmail.Send();
                            }
                            else
                            {
                                Utilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                                outmail.Recipients.Add(_dBugEmail);
                                outmail.Recipients.Add(_eGrantsDevEmail);
                                outmail.Subject = replySubj;
                                outmail.Send();
                            }
                        }
                        else if (v_SubLine.Contains("Expiring Funds") || v_SubLine.Contains("EXPIRING FUNDS-"))
                        {
                            //Only attached document has to be extracted so many make Body=""
                            var replySubj = $"category=Closeout, extract=2, {currentItem.Subject}";
                            Utilities.ShowDiagnosticIfVerbose($"Reply subject : {replySubj}", verbose);
                            var outmail = currentItem.Forward();
                            if (debug == "n")
                            {
                                outmail.Recipients.Add(_eFileEmail);
                                outmail.Recipients.Add(_eGrantsDevEmail);
                                outmail.Recipients.Add(_eGrantsTestEmail);
                                outmail.Recipients.Add(_eGrantsStageEmail);
                                outmail.Subject = replySubj;
                                outmail.Send();
                            }
                            else
                            {
                                Utilities.ShowDiagnosticIfVerbose($"DON'T WANT THIS {v_SubLine}", verbose);
                                outmail.Recipients.Add(_dBugEmail);
                                outmail.Recipients.Add(_eGrantsDevEmail);
                                outmail.Subject = replySubj;
                                outmail.Send();
                            }
                        }
                        else if (v_SubLine.Contains("Prior Approval: "))
                        {
                            var outmail = currentItem.Forward();
                            if (debug == "n")
                            {
                                outmail.Recipients.Add(_nciGrantsPostAwardEmail);
                                outmail.Send();
                            }
                            else
                            {
                                outmail.Recipients.Add(_dBugEmail);
                                outmail.Recipients.Add(_eGrantsDevEmail);
                                outmail.Send();
                            }
                        } // WWLLO is FFR

                    }

                }


            }




            return itemsProcessedCount;
        }

        private static string GetApplId(string str, SqlConnection con)
        {
            try
            {
                var queryText = $"select dbo.Imm_fn_applid_match( ' @LocalId ') as applid";
                using (SqlCommand command = new SqlCommand(queryText, con))
                {
                    command.Parameters.Add(new SqlParameter("LocalId", str));
                    command.ExecuteReader();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string applId = $"{reader.GetInt64(0)}";    // appl Id
                        return applId;
                    }
                }
                return string.Empty;
            }
            catch
            {
                Console.WriteLine("Query failed.");
                throw new System.Exception($"Get Appl Id query failed. Input string : '{str}'");
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
