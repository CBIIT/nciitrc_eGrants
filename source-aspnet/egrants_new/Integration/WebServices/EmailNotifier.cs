using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using egrants_new.Integration.Models;
using Hangfire.Common;

namespace egrants_new.Integration.WebServices
{
    public class EmailNotifier
    {

        public void GenerateExceptionMessage()
        {

            var config = ConfigurationManager.AppSettings;
            bool enabled = false;
            enabled = bool.Parse(config["Enabled"]);
            if (enabled)
            {

                var repo = new IntegrationRepository();
                List<WebServiceHistory> exceptions = repo.GetExceptions();

                if (exceptions.Count > 0)
                {


                    var message = new MailMessage()
                    {
                        From = new MailAddress(config["FromAddress"]),
                        IsBodyHtml = true,
                        Priority = MailPriority.Normal,
                        Subject = config["SubjectLine"],
                    };


                    var addresses = config["ToAddress"].Split(';');
                    foreach (var address in addresses)
                    {
                        message.To.Add(new MailAddress(address));
                    }

                    string mailContent = "";
                    int exCount = 1;
                    foreach (var ex in exceptions)
                    {
                        mailContent +=
                            $"<b>{exCount}) WebService:</b> {ex.WebServiceName}<br>   <b>Endpoint Uri:</b> {ex.EndpointUriSent} <br>   <b>Sent at:</b> {ex.DateTriggered} <br>   <b>ExceptionDetails:</b><br> {ex.ExceptionMessage}<br><br>";
                        exCount++;
                    }

                    string mailTemplate = File.ReadAllText(config["MailTemplate"]);
                    mailContent = mailTemplate.Replace("###Exceptions", mailContent);
                    message.Body = mailContent;
                    SendEmail(message);
                    exceptions.ForEach(h => repo.MarkHistorySent(h));
                }
            }
        }


        public void GenerateSQLJobErrorMessage()
        {

            var config = ConfigurationManager.AppSettings;
            bool enabled = true;
            //enabled = bool.Parse(config["Enabled"]);
            if (enabled)
            {

                var repo = new IntegrationRepository();
                List<SQLJobError> errors = repo.GetSQLJobErrors();

                if (errors.Count > 0)
                {


                    var message = new MailMessage()
                    {
                        From = new MailAddress(config["FromAddress"]),
                        IsBodyHtml = true,
                        Priority = MailPriority.Normal,
                        //Subject = config["SubjectLine"],
                        Subject = "Test SQL Job Error Message"
                    };


                    var addresses = config["ToAddress"].Split(';');
                    foreach (var address in addresses)
                    {
                        message.To.Add(new MailAddress(address));
                    }

                    string mailContent = "";
                    int errCount = 1;
                    foreach (var error in errors)
                    {
                        mailContent +=
                            $"<b>{errCount}) SQJ Job Name:</b> {error.JobName}<br>   <b>Job Step:</b> {error.StepId} <br>   <b>Error Date/Time:</b> {error.ErrorDateTime} <br>   <b>ErrorMessage:</b><br> {error.ErrorMessage}<br><br>";
                        errCount++;
                    }

                    string mailTemplate = File.ReadAllText(config["MailTemplate"]);
                    mailContent = mailTemplate.Replace("###Exceptions", mailContent);
                    message.Body = mailContent;
                    SendEmail(message);
                    errors.ForEach(err => repo.MarkSQLJobErrorSent(err));
                }
            }
        }




        public void GenerateTestMessage()
        {
            var message = new MailMessage();

            message.Body = "This is a test message";
            SendEmail(message);
        }


        private void SendEmail(MailMessage message)
        {
            var config = ConfigurationManager.AppSettings;

            try
            {
                SmtpClient smtp = new SmtpClient(config["SMTPServer"]);
                smtp.Port = int.Parse(config["SMTPPort"]);
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                //TODO:  Handle Exception on email send
            }

        }


    }



}



