using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Threading.Tasks;
using System.Configuration;
using System.EnterpriseServices.Internal;
using egrants_new.Integration.EmailRulesEngine.Models;
using egrants_new.Integration.Models;
using Newtonsoft.Json.Linq;
using static egrants_new.Integration.Models.IntegrationEnums;


namespace egrants_new.Integration.EmailRulesEngine
{
    public class EmailAdminSupplementProcess : BaseEmailAction
    {

        public EmailAdminSupplementProcess(EmailRule rule, EmailRuleAction action) : base(rule, action)
        {

        }

        public override void DelegatedAction(EmailMsg msg)
        {

            RunAdminSupplement(msg);

        }




        public void RunAdminSupplement(EmailMsg msg)
        {

            string fgn = "";
            string alias = "";
            string notificationId = "";
            GrantEmailAttachment attachment = null;
            string outputFolder = this.Action.TargetValue;
            //Although msgDetails was meant to help send data to Stored procedure thru the Repo
            //It can also serve as the state of the processing.
            ExtractedMessageDetails msgDetails = new ExtractedMessageDetails();
            msgDetails.Body = msg.MessageBody;
            msgDetails.Rcvd_dt = msg.ReceivedDateTime;
            msgDetails.Sub = msg.Subject;


            try
            {
                if (msg.Sender.ToLower().Contains("nciogaegrantsprod"))
                {
                    msgDetails.Catname = "Correspondence";

                    if (msg.Subject.Contains("Change in Status"))
                    {
                        msgDetails.Subcatname = "Supplement Status Change";

                    }
                    else if (msg.Subject.Contains("Admin Supplement "))
                    {
                        msgDetails.Subcatname = "Admin Supplement";

                    }
                    else if (msg.Subject.Contains("Response Required"))
                    {
                        msgDetails.Subcatname = "Supplement Response Required";

                    }
                    else if (msg.Subject.Contains("Diversity Supplement"))
                    {
                        msgDetails.Subcatname = "Diversity Supplement";
                    }
                    else
                    {
                        msgDetails.Subcatname = "Unknown";
                    }


                    msgDetails.Filetype = "txt";
                    //Get Notification ID
                    notificationId = base.ExtractValue(msg.MessageBody, "Notification Id=");
                    if (string.IsNullOrWhiteSpace(notificationId))
                    {
                        msgDetails.StatusOfProcessing = "";
                    }

                    msgDetails.Parentapplid = GetTempApplId(notificationId);

                    msgDetails.Filenumbername = base.GetPlaceholder(msgDetails);
                    if (string.IsNullOrWhiteSpace(msgDetails.Filenumbername))
                    {
                        //TODO: Email Error
                        throw new Exception("ERROR: Could not create entry in WIP.Check DB proc: getPlaceHolder_new to load OGA_Notification");
                        //    .Recipients.Add("leul.ayana@nih.gov")
                        //    .Recipients.Add("leul.ayana@nih.gov")
                        //    .Recipients.Add("guillermo.choy-leon@nih.gov")
                        //this to admin.replysubj =
                        //    "ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new to load OGA_Notification"
                    }


                    alias = msgDetails.Filenumbername + ".txt";
                    // Generate file from email and save 


                    SaveMessage(msg, alias, outputFolder, MessageSaveType.Text);

                }
                else if (msg.Sender.ToLower().Contains("caeranotifications"))
                {
                    msgDetails.Filetype = "txt";

                    msgDetails.Catname = "eRA Notification";
                    if (msg.Subject.Contains(" Supplement Requested "))
                    {
                        msgDetails.Subcatname = "Supplement Requested";
                    }
                    else
                    {
                        msgDetails.Subcatname = "Unknown";
                    }

                    if (!string.IsNullOrWhiteSpace(msg.Subject))
                    {
                        msgDetails.Parentapplid = GetApplId(msg.Subject);
                    }

                    if (msgDetails.Parentapplid == 0)
                    {
                        msgDetails.Parentapplid = GetApplId(msg.MessageBody);
                    }

                    if (msgDetails.Parentapplid == 0)
                    {
                        throw new Exception("ERROR: Supplement could not identified");
                        // Email That there was an error determining the 
                        //replysubj = "ERROR: Supplement could not identified"
                        //.Recipients.Add("leul.ayana@nih.gov")
                        //.Recipients.Add("guillermo.choy-leon@nih.gov")
                    }
                    else
                    {
                        msgDetails.Pa = GetPa(msg.Subject);

                        msgDetails.Filenumbername = GetPlaceholder(msgDetails);

                        if (string.IsNullOrWhiteSpace(msgDetails.Filenumbername))
                        {
                            throw new Exception("ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new to load OGA_Notification");
                            //TODO: Email Error
                            //    .Recipients.Add("leul.ayana@nih.gov")
                            //    .Recipients.Add("guillermo.choy-leon@nih.gov")
                            //    "ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new to load OGA_Notification"
                        }

                        alias = msgDetails.Filenumbername + ".txt";
                        // Generate file from email and save 
                        SaveMessage(msg, alias, outputFolder, MessageSaveType.Text);

                    }
                }
                else if (GrantAttachmentEmailPermission(msg.Sender.ToLower()))
                {
                    //msg.Sender.ToLower().Contains("driskelleb") || (msg.Sender.ToLower().Contains("jonesni")) ||
                    //    (msg.Sender.ToLower().Contains("omairi")) || (msg.Sender.ToLower().Contains("woldezf")) || (msg.Sender.ToLower().Contains("emily.driskell"))
                    //    || msg.Sender.ToLower()

                    fgn = ExtractValue(msg.Subject, "grantnumber=");
                    msgDetails.Catname = ExtractValue(msg.Subject, "category=");
                    msgDetails.Parentapplid = ExtractValue(msg.Subject, "applid=").Length > 0
                        ? int.Parse(ExtractValue(msg.Subject, "applid="))
                        : 0;
                    msgDetails.Subcatname = ExtractValue(msg.Subject, "sub=");
                    //For some reason old logic excludes the subject and body
                    //continuing that here
                    msgDetails.Sub = String.Empty;
                    msgDetails.Body = String.Empty;

                    if (string.IsNullOrWhiteSpace(msgDetails.Catname))
                    {
                        msgDetails.Catname = "correspondence";
                    }

                    if (msgDetails.Catname == "correspondence" && !msg.HasAttachments && msg.MessageBody.Length > 0)
                    {
                        msgDetails.Filetype = "txt";

                    }
                    else if ((msgDetails.Catname == "application file" || msgDetails.Catname == "applicationfile") &&
                             msg.HasAttachments)
                    {
                        msgDetails.Catname = "application file";
                        var attachments = EmailRepo.GetEmailAttachments(msg.GraphId);
                        if (attachments.Count > 1)
                        {
                            throw new Exception("There were more than 1 attachments included");
                        }

                        attachment = attachments.FirstOrDefault();
                        msgDetails.Filetype = attachment?.Name.Split('.')[1];
                        msgDetails.Subcatname = String.Empty;
                    }


                    //Try to get the applId 
                    if (!string.IsNullOrWhiteSpace(msg.Subject) && msgDetails.Parentapplid == 0 &&
                        string.IsNullOrWhiteSpace(fgn))
                    {
                        msgDetails.Parentapplid = base.GetApplId(base.RemoveSpecialCharacters(fgn));
                    }
                    else if (msg.Subject.Length > 0 && msgDetails.Parentapplid == 0 && fgn.Length == 0)
                    {
                        msgDetails.Parentapplid = base.GetApplId(RemoveSpecialCharacters(msg.Subject));
                    }

                    if (msgDetails.Parentapplid == 0)
                    {
                        msgDetails.Parentapplid = GetApplId(RemoveSpecialCharacters(msg.MessageBody));
                    }

                    if (msgDetails.Parentapplid == 0)
                    {
                        throw new Exception(
                            "ERROR: GRANT NUMBER OR APPL_ID COULD BE IDENTIFIED EITHER IN SUBJECT OR EMAIL BODY");
                        //Send Email that there was failure
                        //.Recipients.Add("leul.ayana@nih.gov")
                    }
                    else if (msgDetails.Catname == "correspondence" && string.IsNullOrWhiteSpace(msgDetails.Subcatname))
                    {
                        throw new Exception(
                            "INVALID SUBJECT LINE - Two parameters are important. 1)category  2)grantnumber. If 1)category = coresspondence. You must add third parameter called sub=<<subcategoryname>>. Example category=correspondence,sub=admin supplement,grantnumber=SP30CA123456-65 ");
                        //Set OutMail = CItem.Forward
                        //    .Recipients.Add("leul.ayana@nih.gov")

                    }
                    else if (msgDetails.Parentapplid != 0 &&
                             (msgDetails.Catname == "correspondence" || msgDetails.Catname == "application file" ||
                              msgDetails.Catname == "applicationfile"))
                    {
                        msgDetails.Pa = "";
                        msgDetails.Filenumbername = GetPlaceholder(msgDetails);
                        alias = string.Join(".",msgDetails.Filenumbername, msgDetails.Filetype);

                        if (string.IsNullOrWhiteSpace(msgDetails.Filenumbername))
                        {
                            throw new Exception(
                                "ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new");
                            //TODO: Email Error
                            //    .Recipients.Add("leul.ayana@nih.gov")
                            //    .Recipients.Add("guillermo.choy-leon@nih.gov")
                            //    "ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new to load OGA_Notification"
                        }
                        else if ((msgDetails.Catname == "correspondence") && !msg.HasAttachments &&
                                 msg.MessageBody.Length > 0)
                        {
                            // Generate file from email and save 
                            SaveMessage(msg, alias, outputFolder, MessageSaveType.Text);
                        }
                        else if (msgDetails.Catname == "application file" && msg.HasAttachments)
                        {
                            attachment?.SaveToDisk(LocalPath, msgDetails.Filenumbername, msgDetails.Filetype);

                            string localFile = Path.Combine(LocalPath, alias);
                            string destinationFile = Path.Combine(outputFolder, alias);
                            CopyFile(localFile, destinationFile);
                        }
                    }
                }
                else
                {
                    //If this is a reply from PD OR PI
                    notificationId = base.ExtractValue(msg.MessageBody, "Notification Id=");
                    if (!string.IsNullOrWhiteSpace(notificationId))
                    {

                        bool isReply = CheckIsReply(notificationId, msg.Sender);
                        msgDetails.Pa = GetPa(msg.Subject);
                        msgDetails.Parentapplid = GetTempApplId(notificationId);
                        msgDetails.Catname = "Correspondence";
                        msgDetails.Subcatname = "Supplement Response";
                        msgDetails.Filetype = "txt";

                        msgDetails.Filenumbername = base.GetPlaceholder(msgDetails);

                        if (string.IsNullOrWhiteSpace(msgDetails.Filenumbername))
                        {
                            //TODO:  Fix condition where this error is generated as fallback for not matching any criteria.
                            throw new Exception("ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new to load OGA_Notification");
                            //If getPlaceHolder_new did not returned any thing this means there is an error to be investigated forward this to admin. 
                            //TODO: Email Error
                            //    .Recipients.Add("guillermo.choy-leon@nih.gov")

                        }
                        else
                        {
                            //Save the File 
                            alias = msgDetails.Filenumbername + ".txt";
                            // Generate file from email and save 
                            SaveMessage(msg, alias, outputFolder, MessageSaveType.Text);
                        }
                    }
                    else
                    {
                        throw new Exception("UNIdentified email: NCIOGASupplement public folder");
                        //    .Recipients.Add("guillermo.choy-leon@nih.gov")
                    }
                }

                msgDetails.Body = string.Empty;
                JObject obj = JObject.FromObject(msgDetails);
                ActionMessage = obj.ToString();
            }
            catch (Exception ex)
            {
                msgDetails.Body = string.Empty;
                JObject obj = JObject.FromObject(msgDetails);
                ActionMessage = obj.ToString();
                throw ex;
            }

        }



        public bool GrantAttachmentEmailPermission(string sender)
        {
            return EmailRepo.GrantAttachmentPermission(sender);

        }



    }
}