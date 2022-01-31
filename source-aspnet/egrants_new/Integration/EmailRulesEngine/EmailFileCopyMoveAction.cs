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
using Newtonsoft.Json.Linq;


namespace egrants_new.Integration.EmailRulesEngine
{
    public class EmailFileCopyMoveAction : BaseEmailAction
    {

        public EmailFileCopyMoveAction(EmailRule rule, EmailRuleAction action) : base(rule, action)
        {

        }

        public override void DelegatedAction(EmailMsg msg)
        {
            EmailMsg message = msg;

            string tmpActionMsg = "Action Initialized";

            //SaveAttachmentAndFileMoveCopy
            var destinationPath = Action.TargetValue;

            //Create the file
            tmpActionMsg = "Creating TXT file layout";
            string txtFileContents = $@"From:	{msg.EmailFrom}
Sent:	{msg.SentDateTime}
To:	{msg.ToRecipients}
Subject:	{msg.Subject} 



{msg.Body}";
            tmpActionMsg = "Getting eGrants Document Placeholder Filename";
            //string fileName = GetPlaceHolderFileName();
            string fileName = string.Join(".", Guid.NewGuid().ToString(), "txt");
            string localPath = ConfigurationManager.AppSettings["EmailAttachmentTempFolder"];
            string localFile = Path.Combine(localPath, fileName);
            string destinationFile = Path.Combine(destinationPath, fileName);

            tmpActionMsg = "Writing File to Disk";
            File.WriteAllText(localFile, txtFileContents);

            //Move File to Remote Dir
            tmpActionMsg = "Copying File to Remote directory";
            File.Copy(localFile, destinationFile);

            tmpActionMsg = "Action Completed";
        }




        public override void ExtractMessageDetails(EmailMsg msg)
        {
            //string msgBody = (string) JObject.Parse(msg.Body)["content"];
            string subcatname = "";
            string catname = "";
            string pa = "";
            string notification_filetype = "txt";
            int applId=0;
            string fgn = "";
            string alias = "";
            string notificationId = "";


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


                string Alias = filenumbername + ".txt";
                // Generate file from email and save 
                
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

                if (string.IsNullOrWhiteSpace(msg.Subject))
                {
                    applId = GetApplId(msg.Subject);
                }

                if (applId == 0)
                {
                    applId = GetApplId(msgBody);
                }

                if (applId == 0)
				{
					// Email That there was an error determining the 
					//replysubj = "ERROR: Supplement could not identified"
                    //.Recipients.Add("leul.ayana@nih.gov")
                    //    .Recipients.Add("guillermo.choy-leon@nih.gov")
                    //    .Recipients.Add("leul.ayana@nih.gov")
				}
                else
                {
                    pa = base.GetPa(msg.Subject);

                    ExtractedMessageDetails msgDetails = new ExtractedMessageDetails();

                    msgDetails.Body = msgBody;
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

                    string Alias = filenumbername + ".txt";

                    // Generate file from email and save  

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
                else if((catname == "application file" || catname == "applicationfile") && msg.HasAttachments)
                {
                    //Get attachment 
                    //Get attachment filename
                    FileName = removejunk(CAttachments(1).FileName);
                    notification_filetype = getFileType(CName);
                }

                if (!string.IsNullOrWhiteSpace(msg.Subject) && applId == 0 && string.IsNullOrWhiteSpace(fgn))
                {
                    applId = int.Parse(ExtractValue(fgn, "applid="));
                }
                else if(msg.Subject.Length>0 && applId == 0 && fgn.Length ==0)
                {
                    applId = int.Parse(ExtractValue(msg.Subject, "applid="));
                }

                if (applId == 0)
                {
                    applId = GetApplId(msg.MessageBody);
                }

                if (applId == 0)
                {
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

                }else if (applId != 0 && (catname == "correspondence" || catname == "application file" || catname == "applicationfile"))
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
                        //TODO: Email Error
                        //    .Recipients.Add("leul.ayana@nih.gov")
                        //    .Recipients.Add("guillermo.choy-leon@nih.gov")
                        //    .Recipients.Add("leul.ayana@nih.gov")
                        //    .Subject = replysubj
                        //    .Body = replyText & vbNewLine & replybody & vbNewLine & CItem.body
                        //    "ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new to load OGA_Notification"
                    }else if ((catname == "correspondence") && !msg.HasAttachments && msg.MessageBody.Length > 0)
                    {

                        alias = filenumbername + ".txt";
                        //Generate Text of Email
                    }
                    else if (catname == "application file" && msg.HasAttachments )
                    {
                        alias = filenumbername + "." + notification_filetype;
                        //TODO: Get Attachment and Save   
                    }
                }
            }
            else
            {
                //If this is a reply from PD OR PI
                notificationId = base.ExtractValue(msg.MessageBody, "Notification Id=");
                if (string.IsNullOrWhiteSpace(notificationId))
                {

                    bool isReply = CheckIsReply(notificationId, msg.Sender);
                    pa = GetPa(msg.Subject);
                    applId = GetTempApplId(notificationId);
                    catname = "Correspondence";
                    subcatname = "Supplement Response";
                    notification_filetype = "txt";

                    ExtractedMessageDetails msgDetails = new ExtractedMessageDetails();

                    msgDetails.Body = msgBody;
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
                        //Save Email as Text


                    }
                }
                else
                {
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


        private string GetPlaceHolderFileName(EmailMsg msg)
        {
            var repo = new EmailIntegrationRepository();

            var msgDetails = EmailActionModule.ExtractMessageDetails(msg, base.EmailRule);

            string filename = repo.GetPlaceHolder(msgDetails);
            filename = string.Join(".", filename, msgDetails.Filetype);

            return filename;
        }

        private string GetSubcat(List<EmailMsgMetadata> metadata)
        {
            string subcatname = "Unknown";
            string subject = metadata.Where(m => m.Name == "Subject").Select(m => m.metadata).ToString();

            if (subject.ToLower().Contains("Change in Status".ToLower()))
            {
                subcatname = "Supplement Status Change";
            }
            else if (subject.ToLower().Contains("Admin Supplement ".ToLower()))
            {
                subcatname = "Admin Supplement";
            }
            else if (subject.ToLower().Contains("Response Required".ToLower()))
            {
                subcatname = "Supplement Response Required";
            }
            else if (subject.ToLower().Contains("Diversity Supplement ".ToLower()))
            {
                subcatname = "Diversity Supplement";
            }

            return subcatname;
        }


        private void CreateTextFile(EmailMsg msg)
        {

            string tmpActionMsg = "Action Initialized";

            //SaveAttachmentAndFileMoveCopy
            var destinationPath = Action.TargetValue;

            //Create the file
            tmpActionMsg = "Creating TXT file layout";
            string txtFileContents = $@"From:       {msg.EmailFrom}
Sent:       {msg.SentDateTime}
To:         {msg.ToRecipients}
Subject:	{msg.Subject} 



{msg.MessageBody}";
            tmpActionMsg = "Getting eGrants Document Placeholder Filename";
            //string fileName = GetPlaceHolderFileName();
            string fileName = string.Join(".", Guid.NewGuid().ToString(), "txt");
            string localPath = ConfigurationManager.AppSettings["EmailAttachmentTempFolder"];
            string localFile = Path.Combine(localPath, fileName);
            string destinationFile = Path.Combine(destinationPath, fileName);

            tmpActionMsg = "Writing File to Disk";
            File.WriteAllText(localFile, txtFileContents);

            //Move File to Remote Dir
            tmpActionMsg = "Copying File to Remote directory";
            File.Copy(localFile, destinationFile);

            tmpActionMsg = "Action Completed";

        }



    }
}