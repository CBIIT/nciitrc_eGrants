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
            //string msgBody = (string) JObject.Parse(msg.Body)["content"];
            string subcatname = "";
            string catname = "";
            string pa = "";
            string notification_filetype = "txt";
            int applId = 0;
            string fgn = "";
            string alias = "";
            string notificationId = "";
            string outputFolder = this.Action.TargetValue;


            if (msg.Sender.Contains("nciogaegrantsprod"))
            {
                catname = "Correspondence";

                if (msg.Subject.Contains("Change in Status"))
                {
                    subcatname = "Supplement Status Change";

                }
                else if (msg.Subject.Contains("Admin Supplement "))
                {
                    subcatname = "Admin Supplement";

                }
                else if (msg.Subject.Contains("Response Required"))
                {
                    subcatname = "Supplement Response Required";

                }
                else if (msg.Subject.Contains("Diversity Supplement"))
                {
                    subcatname = "Diversity Supplement";
                }
                else
                {
                    subcatname = "Unknown";
                }


                notification_filetype = "txt";
                //Get Notification ID
                notificationId = base.ExtractValue(msg.MessageBody, "Notification Id=");
                applId = GetTempApplId(notificationId);

                ExtractedMessageDetails msgDetails = new ExtractedMessageDetails();

                msgDetails.Body = msg.MessageBody;
                msgDetails.Catname = catname;
                msgDetails.Filetype = notification_filetype;
                msgDetails.Pa = pa;
                msgDetails.Parentapplid = applId;
                msgDetails.Rcvd_dt = msg.ReceivedDateTime;
                msgDetails.Sub = msg.Subject;
                msgDetails.Subcatname = subcatname;


                string filenumbername = base.GetPlaceholder(msgDetails);
                if (string.IsNullOrWhiteSpace(filenumbername))
                {
                    //TODO: Email Error
                    //    .Recipients.Add("leul.ayana@nih.gov")
                    //    .Recipients.Add("leul.ayana@nih.gov")
                    //    .Recipients.Add("guillermo.choy-leon@nih.gov")
                    //this to admin.replysubj =
                    //    "ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new to load OGA_Notification"
                }


                alias = filenumbername + ".txt";
                // Generate file from email and save 
                SaveMessage(msg, alias, outputFolder, MessageSaveType.Text);

            }
            else if (msg.Sender.Contains("caeranotifications"))
            {
                notification_filetype = "txt";

                catname = "eRA Notification";
                if (msg.Subject.Contains(" Supplement Requested "))
                {
                    subcatname = "Supplement Requested";
                }
                else
                {
                    subcatname = "Unknown";
                }

                if (!string.IsNullOrWhiteSpace(msg.Subject))
                {
                    applId = GetApplId(msg.Subject);
                }

                if (applId == 0)
                {
                    applId = GetApplId(msg.MessageBody);
                }

                if (applId == 0)
                {
                    throw new Exception("ERROR: Supplement could not identified");
                    // Email That there was an error determining the 
                    //replysubj = "ERROR: Supplement could not identified"
                    //.Recipients.Add("leul.ayana@nih.gov")
                    //    .Recipients.Add("guillermo.choy-leon@nih.gov")
                    //    .Recipients.Add("leul.ayana@nih.gov")
                }
                else
                {
                    pa = GetPa(msg.Subject);

                    ExtractedMessageDetails msgDetails = new ExtractedMessageDetails();

                    msgDetails.Body = msg.MessageBody;
                    msgDetails.Catname = catname;
                    msgDetails.Filetype = notification_filetype;
                    msgDetails.Pa = pa;
                    msgDetails.Parentapplid = applId;
                    msgDetails.Rcvd_dt = msg.ReceivedDateTime;
                    msgDetails.Sub = msg.Subject;
                    msgDetails.Subcatname = subcatname;

                    string filenumbername = GetPlaceholder(msgDetails);
                    if (string.IsNullOrWhiteSpace(filenumbername))
                    {
                        throw new Exception("ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new to load OGA_Notification");
                        //TODO: Email Error
                        //    .Recipients.Add("leul.ayana@nih.gov")
                        //    .Recipients.Add("leul.ayana@nih.gov")
                        //    .Recipients.Add("guillermo.choy-leon@nih.gov")
                        //this to admin.replysubj =
                        //    "ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new to load OGA_Notification"
                    }

                    alias = filenumbername + ".txt";
                    // Generate file from email and save 
                    SaveMessage(msg, alias, outputFolder, MessageSaveType.Text);

                }
            }
            else if (msg.Sender.Contains("driskelleb") || (msg.Sender.Contains("jonesni")) ||
                      (msg.Sender.Contains("omairi")) || (msg.Sender.Contains("woldezf")))
            {
                fgn = ExtractValue(msg.Subject, "grantnumber=");
                catname = ExtractValue(msg.Subject, "category=");
                applId = int.Parse(ExtractValue(msg.Subject, "applid="));
                subcatname = ExtractValue(msg.Subject, "sub=");


                if (string.IsNullOrWhiteSpace(catname))
                {
                    catname = "correspondence";
                }

                if (catname == "correspondence" && !msg.HasAttachments && msg.MessageBody.Length > 0)
                {
                    notification_filetype = "txt";

                }
                else if ((catname == "application file" || catname == "applicationfile") && msg.HasAttachments)
                {


                    ExtractedMessageDetails msgDetails = new ExtractedMessageDetails();

                    msgDetails.Body = msg.MessageBody;
                    msgDetails.Catname = catname;
                    msgDetails.Filetype = notification_filetype;
                    msgDetails.Pa = pa;
                    msgDetails.Parentapplid = applId;
                    msgDetails.Rcvd_dt = msg.ReceivedDateTime;
                    msgDetails.Sub = msg.Subject;
                    msgDetails.Subcatname = subcatname;

                    string filenumbername = GetPlaceholder(msgDetails);

                    //alias = filenumbername + "." + notification_filetype;

                    var attachments = EmailRepo.GetEmailAttachments(msg.GraphId);
                    if (attachments.Count > 1)
                    {
                        throw new Exception("There were more than 1 attachments included");
                    }

                    var attachment = attachments.FirstOrDefault();
                    string ext = attachment?.Name.Split('.')[1];
                    attachment?.SaveToDisk(outputFolder, filenumbername, ext);
                }

                if (!string.IsNullOrWhiteSpace(msg.Subject) && applId == 0 && string.IsNullOrWhiteSpace(fgn))
                {
                    applId = int.Parse(ExtractValue(fgn, "applid="));
                }
                else if (msg.Subject.Length > 0 && applId == 0 && fgn.Length == 0)
                {
                    applId = int.Parse(ExtractValue(msg.Subject, "applid="));
                }

                if (applId == 0)
                {
                    applId = GetApplId(msg.MessageBody);
                }

                if (applId == 0)
                {
                    throw new Exception("ERROR: GRANT NUMBER OR APPL_ID COULD BE IDENTIFIED EITHER IN SUBJECT OR EMAIL BODY");
                    //Send Email that there was failure
                    //replysubj = "ERROR: GRANT NUMBER OR APPL_ID COULD BE IDENTIFIED EITHER IN SUBJECT OR EMAIL BODY"

                    //Set OutMail = CItem.Forward

                    //With OutMail
                    //    .Recipients.Add("leul.ayana@nih.gov")
                    //    .Recipients.Add("leul.ayana@nih.gov")
                    //    .Recipients.Add("leul.ayana@nih.gov")
                    //    .Subject = replysubj
                    //    .Body = replyText & vbNewLine & vbNewLine & CItem.body
                    //    .Send

                    //End With


                }
                else if (catname == "correspondence" && string.IsNullOrWhiteSpace(subcatname))
                {
                    throw new Exception("INVALID SUBJECT LINE - Two parameter is Important. 1)category  2)grantnumber. If 1)category = coresspondence. You must add third parameter called sub=<<subcategoryname>>. Example category=correspondence,sub=admin supplement,grantnumber=SP30CA123456-65 ");
                    //replysubj = "INVALID SUBJECT LINE"
                    //replyText = "Two parameter is Important. 1)category  2)grantnumber. If 1)category = coresspondence. You must add third parameter called sub=<<subcategoryname>>. Example category=correspondence,sub=admin supplement,grantnumber=SP30CA123456-65"
                    //replyText = replyText & vbNewLine & " If category=Application file, do not add third parameter sub=<<>> . Example : category=application file, grantnumber=SP30CA123456-65"
                    //Set OutMail = CItem.Forward
                    //With OutMail
                    //    .Recipients.Add("leul.ayana@nih.gov")
                    //    .Recipients.Add("leul.ayana@nih.gov")
                    //    .Recipients.Add("leul.ayana@nih.gov")
                    //    .Subject = replysubj
                    //    .Body = replyText & vbNewLine & vbNewLine & CItem.body
                    //    .Send
                    //End With
                    //Set OutMail = nothing

                }
                else if (applId != 0 && (catname == "correspondence" || catname == "application file" || catname == "applicationfile"))
                {
                    pa = "";

                    ExtractedMessageDetails msgDetails = new ExtractedMessageDetails();

                    msgDetails.Body = msg.MessageBody;
                    msgDetails.Catname = catname;
                    msgDetails.Filetype = notification_filetype;
                    msgDetails.Pa = pa;
                    msgDetails.Parentapplid = applId;
                    msgDetails.Rcvd_dt = msg.ReceivedDateTime;
                    msgDetails.Sub = msg.Subject;
                    msgDetails.Subcatname = subcatname;

                    string filenumbername = GetPlaceholder(msgDetails);
                    if (string.IsNullOrWhiteSpace(filenumbername))
                    {
                        throw new Exception("UN Identified email: NCIOGASupplent public folder");
                        //TODO: Email Error
                        //    .Recipients.Add("leul.ayana@nih.gov")
                        //    .Recipients.Add("guillermo.choy-leon@nih.gov")
                        //    .Recipients.Add("leul.ayana@nih.gov")
                        //    .Subject = replysubj
                        //    .Body = replyText & vbNewLine & replybody & vbNewLine & CItem.body
                        //    "ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new to load OGA_Notification"
                    }
                    else if ((catname == "correspondence") && !msg.HasAttachments && msg.MessageBody.Length > 0)
                    {

                        alias = filenumbername + ".txt";
                        // Generate file from email and save 
                        SaveMessage(msg, alias, outputFolder, MessageSaveType.Text);
                    }
                    else if (catname == "application file" && msg.HasAttachments)
                    {
                        //TODO: Get Attachment and Save  

                        alias = filenumbername + "." + notification_filetype;

                        var attachments = EmailRepo.GetEmailAttachments(msg.GraphId);
                        if (attachments.Count > 1)
                        {
                            throw new Exception("There were more than 1 attachments included");
                        }
                        else
                        {
                            var attachment = attachments.FirstOrDefault();
                            string ext = attachment.Name.Split('.')[1];
                            attachment.SaveToDisk(outputFolder, filenumbername, ext);
                        }
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
                    pa = GetPa(msg.Subject);
                    applId = GetTempApplId(notificationId);
                    catname = "Correspondence";
                    subcatname = "Supplement Response";
                    notification_filetype = "txt";

                    ExtractedMessageDetails msgDetails = new ExtractedMessageDetails();

                    msgDetails.Body = msg.MessageBody;
                    msgDetails.Catname = catname;
                    msgDetails.Filetype = notification_filetype;
                    msgDetails.Pa = pa;
                    msgDetails.Parentapplid = applId;
                    msgDetails.Rcvd_dt = msg.ReceivedDateTime;
                    msgDetails.Sub = msg.Subject;
                    msgDetails.Subcatname = subcatname;

                    string filenumbername = base.GetPlaceholder(msgDetails);
                    if (string.IsNullOrWhiteSpace(filenumbername))
                    {
                        throw new Exception("ERROR: Could not create entry in WIP. ");
                        //If getPlaceHolder_new did not returned any thing this means there is an error to be investigated for ward this to admin. 
                        //TODO: Email Error
                        //    .Recipients.Add("leul.ayana@nih.gov")
                        //    .Recipients.Add("leul.ayana@nih.gov")
                        //    .Recipients.Add("guillermo.choy-leon@nih.gov")
                        //this to admin.replysubj =
                        //    "ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new to load OGA_Notification"
                    }
                    else
                    {
                        //Save the File 
                        alias = filenumbername + ".txt";
                        // Generate file from email and save 
                        SaveMessage(msg, alias, outputFolder, MessageSaveType.Text);
                    }
                }
                else
                {
                    throw new Exception("UN Identified email: NCIOGASupplent public folder");
                    //Set OutMail = CItem.Forward
                    //	replysubj="UN Identified email: NCIOGASupplent public folder: "
                    //With OutMail
                    //    .Recipients.Add("leul.ayana@nih.gov")
                    //    .Recipients.Add("guillermo.choy-leon@nih.gov")
                    //    .Recipients.Add("leul.ayana@nih.gov")
                    //    .Subject = replysubj
                    //    .Body = replyText & vbNewLine & vbNewLine & CItem.body
                    //    .Send

                    //End With

                }
            }
        }

    }
}