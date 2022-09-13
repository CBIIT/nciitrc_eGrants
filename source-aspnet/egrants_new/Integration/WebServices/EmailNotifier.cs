#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  EmailNotifier.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-03-31
// Contributors:
//      - Briggs, Robin (NIH/NCI) [C] - briggsr2
//      -
// Copyright (c) National Institute of Health
// 
// <Description of the file>
// 
// This source is subject to the NIH Softwre License.
// See https://ncihub.org/resources/899/download/Guidelines_for_Releasing_Research_Software_04062015.pdf
// All other rights reserved.
// 
// THE SOFTWARE IS PROVIDED "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT ARE DISCLAIMED. IN NO EVENT SHALL THE NATIONAL
// CANCER INSTITUTE (THE PROVIDER), THE NATIONAL INSTITUTES OF HEALTH, THE
// U.S. GOVERNMENT OR THE INDIVIDUAL DEVELOPERS BE LIABLE FOR ANY DIRECT,
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
// \***************************************************************************/

#endregion

#region

using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;

#endregion

namespace egrants_new.Integration.WebServices
{
    /// <summary>
    /// The email notifier.
    /// </summary>
    public class EmailNotifier
    {

        /// <summary>
        /// The generate exception message.
        /// </summary>
        public void GenerateExceptionMessage()
        {
            var config = ConfigurationManager.AppSettings;
            var enabled = false;
            enabled = bool.Parse(config["Enabled"]);

            if (enabled)
            {
                var repo = new IntegrationRepository();
                var exceptions = repo.GetExceptions();

                if (exceptions.Count > 0)
                {
                    var message = new MailMessage
                                      {
                                          From = new MailAddress(config["FromAddress"]),
                                          IsBodyHtml = true,
                                          Priority = MailPriority.Normal,
                                          Subject = config["SubjectLine"]
                                      };

                    var addresses = config["ToAddress"].Split(';');

                    foreach (var address in addresses)
                        message.To.Add(new MailAddress(address));

                    var mailContent = "<hr>eGrants Web Service Exceptions<hr>";
                    var exCount = 1;

                    foreach (var ex in exceptions)
                    {
                        mailContent
                            += $"<b>{exCount}) WebService:</b> {ex.WebServiceName}<br>   <b>Endpoint Uri:</b> {ex.EndpointUriSent} <br>   <b>Sent at:</b> {ex.DateTriggered} <br>   <b>ExceptionDetails:</b><br> {ex.ExceptionMessage}<br><br>";

                        exCount++;
                    }

                    var mailTemplate = File.ReadAllText(config["MailTemplate"]);
                    mailContent = mailTemplate.Replace("###Exceptions", mailContent);
                    message.Body = mailContent;
                    this.SendEmail(message);
                    exceptions.ForEach(h => repo.MarkHistorySent(h));
                }
            }
        }

        /// <summary>
        /// The generate sql job error message.
        /// </summary>
        public void GenerateSQLJobErrorMessage()
        {
            var config = ConfigurationManager.AppSettings;
            var enabled = true;
            enabled = bool.Parse(config["SQLNotificationEnabled"]);

            if (enabled)
            {
                var repo = new IntegrationRepository();
                var errors = repo.GetSQLJobErrors();

                if (errors.Count > 0)
                {
                    var message = new MailMessage
                                      {
                                          From = new MailAddress(config["FromAddress"]),
                                          IsBodyHtml = true,
                                          Priority = MailPriority.Normal,
                                          Subject = config["SQLSubjectLine"]
                                      };

                    var addresses = config["SQLToAddress"].Split(';');

                    foreach (var address in addresses)
                        message.To.Add(new MailAddress(address));

                    var mailContent = "<hr>eGrants SQL Job Error Messages<hr>";
                    var errCount = 1;

                    foreach (var error in errors)
                    {
                        mailContent
                            += $"<b>{errCount}) SQJ Job Name:</b> {error.JobName}<br>   <b>Job Step:</b> {error.StepId.ToString()} <br>   <b>Error Date/Time:</b> {error.ErrorDateTime.ToString()} <br>   <b>ErrorMessage:</b><br> {error.ErrorMessage}<br><br>";

                        errCount++;
                    }

                    var mailTemplate = File.ReadAllText(config["SQLMailTemplate"]);
                    mailContent = mailTemplate.Replace("###Exceptions", mailContent);
                    message.Body = mailContent;
                    this.SendEmail(message);
                    errors.ForEach(err => repo.MarkSQLJobErrorSent(err));
                }
            }
        }

        /// <summary>
        /// The generate test message.
        /// </summary>
        public void GenerateTestMessage()
        {
            var message = new MailMessage();

            message.Body = "This is a test message";
            this.SendEmail(message);
        }

        /// <summary>
        /// The send email.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        private void SendEmail(MailMessage message)
        {
            var config = ConfigurationManager.AppSettings;

            try
            {
                var smtp = new SmtpClient(config["SMTPServer"]);
                smtp.Port = int.Parse(config["SMTPPort"]);
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                // TODO:  Handle Exception on email send
            }
        }
    }

}