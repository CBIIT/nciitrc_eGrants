using System;
using System.Collections.Generic;
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
            string msgBody = (string) JObject.Parse(msg.Body)["content"];
            string subcatname = "";
            string catname = "";
            string pa = "";
            string notification_filetype = "txt";
            int applId;

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
                string notificationId = base.ExtractValue(msgBody, "Notification Id=");
                applid = GetTempApplId(notificationId);

				ExtractedMessageDetails msgDetails = new ExtractedMessageDetails();

                msgDetails.Body = msgBody;
                msgDetails.Catname = catname;
                msgDetails.Filetype = notification_filetype;
                msgDetails.Pa = pa;
                msgDetails.Parentapplid = applid;
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


            if (msg.Sender.Contains("caeranotifications"))
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

                if (msg.Subject.Length > 0)
                {
                    applId = GetApplId(msg.Subject);
                }



            }
    //        ELSEIF Lcase(v_SenderID) = "caeranotifications"  THEN

				//param = 0
				//beginpos = 1

				//IF(InStr(v_SubLine, " Supplement Requested ") > 0) Then
				//	subcatname = "Supplement Requested"
				//else
				//subcatname = "Unknown"
				//End If

				//Notification_filetype = "txt"

				//''substr = v_SubLine
				//'------------------FW: NIH Automated Email: IC ACTION REQUIRED - 1R15CA161634-01A1 Supplement Requested through PA-14-077
				//'------------------Grab equivalent appl id from grant number in subject line			
				//''---- - Try to get Parent ApplId  from subject line
  
				  IF  len(Trim(v_SubLine)) <> 0  THEN
						applid = getApplid(removespcharacters(v_SubLine), oConn)
				END IF


				''---- - If not found then Try to get Parent ApplId  from email body
  
				  IF  len(Trim(applid)) = 0  THEN
					  applid = getApplid(removespcharacters(CItem.body), oConn)
				END IF

				''---- - 'If Still appl id is blank then send this email to administrator
				IF len(Trim(applid)) = 0  THEN
					'If couldn't find a proper identification, email to emily to have a look into this in Admin area
					replysubj = "ERROR: Supplement could not identified"
					Set OutMail = CItem.Forward
			   		With OutMail
						.Recipients.Add("leul.ayana@nih.gov")
						.Recipients.Add("guillermo.choy-leon@nih.gov")
						.Recipients.Add("leul.ayana@nih.gov")
						.Subject = replysubj
						.Body = replyText & vbNewLine & vbNewLine & CItem.body
						.Send
		   			End With
					Set OutMail = nothing
				ELSE
					pa = getpa(removespcharacters(v_SubLine), oConn)


					''''THERE ARE TWO CONDITION 1) IF PA IS GARBLED AND DOES NOT MATCH TEXT PATTERN THEN ERROR IT OUT AND INFORM EMILY / IMRAN
					''''' IF PA MATCHES THE PATTERN THEN ALLOTHERS RULLE WILL BE APPLIED				
						''Wscript.echo applid &vbTab & pa & vbTab & catname & vbTab & subcatname
							cmd.CommandText = "getPlaceHolder_new"
							cmd.CommandType = 4
							'cmd.CommandType = adCmdText				
							cmd.Parameters.Refresh
							cmd.Parameters(1).Value = applid
							cmd.Parameters(2).Value = pa
							cmd.Parameters(3).Value = CItem.ReceivedTime
							cmd.Parameters(4).Value = catname
							cmd.Parameters(5).Value = Notification_filetype
							cmd.Parameters(6).Value = CItem.Subject
							cmd.Parameters(7).Value = CItem.Body
							cmd.Parameters(8).Value = subcatname

					Set oRS = cmd.Execute

					If(oRS.BOF = True and oRS.EOF = True) Then
						''set oRS = Nothing
						'''MsgBox "No data found Sending email to admin"
						'If getPlaceHolder_new did not returned any thing this means there is an error to be investigated for ward this to admin. 
						replysubj = "ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new"
						Set OutMail = CItem.Forward
						With OutMail
							.Recipients.Add("leul.ayana@nih.gov")
							.Recipients.Add("guillermo.choy-leon@nih.gov")
							.Recipients.Add("leul.ayana@nih.gov")
							.Subject = replysubj
							.Body = replyText & vbNewLine & vbNewLine & CItem.body
							.Send
						End With
						Set OutMail = nothing
						'Exit Do
					Else

						filenumbername = oRS(0)
						''MsgBox  "Return from poroc=>" & filenumbername
						Alias = filenumbername & ".txt"
						''MsgBox filenumbername &"Data found creating document in" & OutDir & Alias
						''MsgBox OutDir &Alias
						CItem.SaveAs OutDir &Alias, olTXT
					   set oRS = Nothing
						''MsgBox "BODY  saved as Alias=" & Alias
					end if

				END IF

		ELSEIF Lcase(v_SenderID) = "driskelleb" or Lcase(v_SenderID)= "jonesni" OR Lcase(v_SenderID)= "omairi" OR Lcase(v_SenderID)= "woldezf" THEN

				''''RULE TO UPOLOAD VIA EMAIL UNDER SUPPLEMENT
				''''MUST HAVE: category = correspondence, OR category = application file, 
				''''MUST HAVE: grantnumber =<< full parent grant number >> IF FULL GRANT NUMBER IS NOT PRESENT IN SUBJECT LINE OR BODY
				''''ONE EMAIL ONE UPLOAD EITHER WITH BODY AND NO ATTACHMENT OR WITH ATTACHMENT AND NO EMAIL BODY
				''''IF THERE IS AN EMAIL BODY AND AN ATTACHENT IN EMAIL WITH  "category=application file, " in subject line only attachent wil be uploaded under category mentioned
				''''IF YOU HAVE AN EMAIL WITH BODY AND ATTACHMENT, THEN SEND TWO EMAILS ONE FOR BODY UNDER CORRESPONDENCE AND ANOTHER EMAIL WITH ATTCHMENT ONLY FOR APPLICATION FILE

				param = 0
				beginpos = 1
				substr = v_SubLine
				Set CAttachments = CItem.Attachments
				'''''Fetch category and Grant number
				WHILE InStr(substr, ",") > 0

								pos = InStr(substr, ",")

								V_saStr = Lcase(Mid(substr, 1, pos - 1))

					IF InStr(V_saStr,"grantnumber") > 0 Then
						fgn = Mid(V_saStr, InStr(V_saStr, "grantnumber"), pos - 1)
						fgn = Extractvalue(fgn, "grantnumber")
					ELSEIF InStr(V_saStr,"category") > 0 Then
						category = Mid(V_saStr, InStr(V_saStr, "category"), pos - 1)
						category = Extractvalue(category, "category")
					ELSEIF InStr(V_saStr,"applid") > 0 Then
						applid = Mid(V_saStr, InStr(V_saStr, "applid"), pos - 1)
						applid = Extractvalue(applid, "applid")
					ELSEIF InStr(V_saStr,"sub") > 0 Then
						subcat = Mid(V_saStr, InStr(V_saStr, "sub"), pos - 1)
						subcat = Extractvalue(subcat, "sub")
					End If

					beginpos = pos + 1
					substr = Mid(substr, beginpos, Len(substr))
					param = param + 1

						WEND
				If(Trim(Lcase(category)) = "") Then
					 category = "correspondence"
				end If

				If(Trim(Lcase(category)) = "correspondence") and(CAttachments.Count = 0) and(Len(Trim(CItem.Body)) > 0) THEN
					Notification_filetype = "txt"
				ELSEIF(Trim(Lcase(category)) = "application file" or Trim(Lcase(category)) = "applicationfile") and(CAttachments.Count > 0) THEN
					CName = removejunk(CAttachments(1).FileName)
					Notification_filetype = getFileType(CName)
					subcat = ""
				end if


				''---- - Try to get Parent ApplId  from subject line
  
				  IF(len(Trim(v_SubLine)) > 0) and(len(Trim(applid)) = 0) and(len(Trim(fgn)) > 0)  THEN
					 applid = getApplid(removespcharacters(fgn), oConn)
				ELSEIF(len(Trim(v_SubLine)) > 0) and(len(Trim(applid)) = 0) and(len(Trim(fgn)) = 0)  THEN
				   applid = getApplid(removespcharacters(v_SubLine), oConn)
				END IF


				''---- - If STILL APPL_ID not found then Try to get Parent ApplId  from email body
  
				  IF  len(Trim(applid)) = 0  THEN
					  applid = getApplid(removespcharacters(CItem.body), oConn)
				END IF

				''---- - 'If Still appl id is blank then send this email to administrator
				IF len(Trim(applid)) = 0  THEN
					'If couldn't find a proper identification, email to emily to have a look into this in Admin area
					replysubj = "ERROR: GRANT NUMBER OR APPL_ID COULD BE IDENTIFIED EITHER IN SUBJECT OR EMAIL BODY"
					Set OutMail = CItem.Forward
			   		With OutMail
						.Recipients.Add("leul.ayana@nih.gov")
						.Recipients.Add("leul.ayana@nih.gov")
						.Recipients.Add("leul.ayana@nih.gov")
						.Subject = replysubj
						.Body = replyText & vbNewLine & vbNewLine & CItem.body
						.Send
		   			End With
					Set OutMail = nothing
				ELSEIF(lcase(category) = "correspondence") and(len(Trim(subcat)) = 0)  THEN
					'If couldn't find a proper sub category, email to emily to have a look into this subject line.
					''RULE: if the category = correspondence then there must be a subcategory
					replysubj = "INVALID SUBJECT LINE"
					replyText = "Two parameter is Important. 1)category  2)grantnumber. If 1)category = coresspondence. You must add third parameter called sub=<<subcategoryname>>. Example category=correspondence,sub=admin supplement,grantnumber=SP30CA123456-65"
					replyText = replyText & vbNewLine & " If category=Application file, do not add third parameter sub=<<>> . Example : category=application file, grantnumber=SP30CA123456-65"
					Set OutMail = CItem.Forward
			   		With OutMail
						.Recipients.Add("leul.ayana@nih.gov")
						.Recipients.Add("leul.ayana@nih.gov")
						.Recipients.Add("leul.ayana@nih.gov")
						.Subject = replysubj
						.Body = replyText & vbNewLine & vbNewLine & CItem.body
						.Send
		   			End With
					Set OutMail = nothing

				ELSEIF((len(applid) <> 0)  and(Lcase(category) = "correspondence" or Lcase(category) = "application file" or Lcase(category) = "applicationfile")) THEN
						  pa = ""       'Dont need pa just to upload a file it is needed for a workflow
						cmd.CommandText = "getPlaceHolder_new"
						cmd.CommandType = 4
						'cmd.CommandType = adCmdText				
						cmd.Parameters.Refresh
						cmd.Parameters(1).Value = applid
						cmd.Parameters(2).Value = pa
						cmd.Parameters(3).Value = CItem.ReceivedTime
						cmd.Parameters(4).Value = category
						cmd.Parameters(5).Value = Notification_filetype
						cmd.Parameters(6).Value = ""
						cmd.Parameters(7).Value = ""
						cmd.Parameters(8).Value = subcat

					Set oRS = cmd.Execute

					If(oRS.BOF = True and oRS.EOF = True) Then
					   set oRS = Nothing
						'If getPlaceHolder_new did not returned any thing this means there is an error to be investigated for ward this to admin. 
						replysubj = "ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new"
						replybody = "appl_id=" & applid & " pa=" & ps & " Rec Time=" & CItem.ReceivedTime & " category=" & category & " Notification Type=" & Notification_filetype & " subcat=" & subcat
						Set OutMail = CItem.Forward
						With OutMail
							.Recipients.Add("leul.ayana@nih.gov")
							.Recipients.Add("guillermo.choy-leon@nih.gov")
							.Recipients.Add("leul.ayana@nih.gov")
							.Subject = replysubj
							.Body = replyText & vbNewLine & replybody & vbNewLine & CItem.body
							.Send
						End With
						Set OutMail = nothing
					ELSEIF(Trim(Lcase(category)) = "correspondence") and(CAttachments.Count = 0) and(Len(Trim(CItem.Body)) > 0) THEN
						filenumbername = oRS(0)
						set oRS = Nothing
						Alias = filenumbername & ".txt"
						CItem.SaveAs OutDir &Alias, olTXT
					ELSEIF(Trim(Lcase(category)) = "application file" and(CAttachments.Count > 0)) THEN
						filenumbername = oRS(0)
						Alias = filenumbername & "." & Notification_filetype
						CAttachments(1).SaveAsFile(OutDir & Alias)
						set oRS = Nothing
					end if

				END IF


			'''''IF v_SenderID IS NEITHER NCIOGASTAGE NOR caeranotifications NOR ExtractNotificationIDElement THIS MEANS THIS IS UNAUTHORIZED EMAIL , MOVE IT TO OLD AND INFORM ADMIN	
		ELSE
				'' IF THIS IS A REPLY FROM PD OR PI
				abc = ExtractNotificationIDElement(Trim(CItem.Body), 2)
				If len(Trim(abc)) > 0  THEN
					''''MsgBox "Notification_id=" & abc
					'''Assume this is a reply from PD check the DB for this Notification and inform Emily
					isReply = CheckIFreply(abc, v_SenderID, oConn)
					pa = getpa(removespcharacters(v_SubLine), oConn)
					applid = getTempApplid(abc, oConn)
					catname = "Correspondence"
					subcat = "Supplement Response"
					Notification_filetype = "txt"
							cmd.CommandText = "getPlaceHolder_new"
							cmd.CommandType = 4
							'cmd.CommandType = adCmdText				
							cmd.Parameters.Refresh
							cmd.Parameters(1).Value = applid
							cmd.Parameters(2).Value = pa
							cmd.Parameters(3).Value = CItem.ReceivedTime
							cmd.Parameters(4).Value = catname
							cmd.Parameters(5).Value = Notification_filetype
							cmd.Parameters(6).Value = CItem.Subject
							cmd.Parameters(7).Value = CItem.Body
							cmd.Parameters(8).Value = subcat

					Set oRS = cmd.Execute

					If(oRS.BOF = True and oRS.EOF = True) Then
					   set oRS = Nothing
						''MsgBox "No data found"
						'If getPlaceHolder_new did not returned any thing this means there is an error to be investigated for ward this to admin. 
						replysubj = "ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new"
						Set OutMail = CItem.Forward
						With OutMail
							.Recipients.Add("leul.ayana@nih.gov")
							.Recipients.Add("guillermo.choy-leon@nih.gov")
							.Recipients.Add("leul.ayana@nih.gov")
							.Subject = replysubj
							.Body = replyText & vbNewLine & vbNewLine & CItem.body
							.Send
						End With
						Set OutMail = nothing
						'Exit Do
					Else
						''MsgBox oRS(0)
						filenumbername = oRS(0)
						set oRS = Nothing
						''MsgBox  "Return from poroc=>" & filenumbername
						Alias = filenumbername & ".txt"
						'''''MsgBox OutDir & Alias
						CItem.SaveAs OutDir &Alias, olTXT
						''MsgBox "BODY  saved as Alias=" & Alias
					end if
					''MsgBox "2==>SENDER=caeranotifications==>db IS DONE"
				''IF THIS EMAIL IS NOT EVEN A REPLY FROM PD/ PI THEN THIS IS JUNK
				ELSE
					'If couldn't find a proper Notification ID, this means this email from somebody else OR JUNK
					replysubj = "UN Identified email: NCIOGASupplent public folder: "
					Set OutMail = CItem.Forward
			   		With OutMail
						.Recipients.Add("leul.ayana@nih.gov")
						.Recipients.Add("guillermo.choy-leon@nih.gov")
						.Recipients.Add("leul.ayana@nih.gov")
						.Subject = replysubj
						.Body = replyText & vbNewLine & vbNewLine & CItem.body
						.Send
		   			End With
					Set OutMail = nothing
				END If

		END IF   '''''	








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



    }
}