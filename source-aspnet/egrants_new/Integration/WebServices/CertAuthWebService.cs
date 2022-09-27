#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  CertAuthWebService.cs
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
using System.Net;
using System.Security.Cryptography.X509Certificates;

using egrants_new.Integration.Models;

#endregion

namespace egrants_new.Integration.WebServices
{
    /// <summary>
    /// The cert auth web service.
    /// </summary>
    public class CertAuthWebService : BaseWebService
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CertAuthWebService"/> class.
        /// </summary>
        /// <param name="ws">
        /// The ws.
        /// </param>
        public CertAuthWebService(WebServiceEndPoint ws)
            : base(ws)
        {
            this.WebService.AuthenticationType = Enumerations.AuthenticationType.Certificate;
        }

        /// <summary>
        /// The add authentication.
        /// </summary>
        /// <param name="webRequest">
        /// The web request.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// </exception>
        public override void AddAuthentication(ref HttpWebRequest webRequest)
        {
            if (this.WebService.AuthenticationType == Enumerations.AuthenticationType.Certificate)
            {
                var certPath = ConfigurationManager.AppSettings[this.WebService.CertificatePath];
                var certPwd = ConfigurationManager.AppSettings[this.WebService.CertificatePwd];

                if (File.Exists(certPath))
                    try
                    {
                        var certificate = new X509Certificate2(certPath, certPwd);
                        webRequest.ClientCertificates.Add(certificate);
                    }
                    catch
                    {
                        throw new Exception("Adding Certificate Failed");
                    }
                else
                    throw new FileNotFoundException("The Specified Client Certificate for Authentication could not be found");
            }
            else
            {
                throw new Exception("Something when wrong.  Authentication not set.");
            }
        }
    }

}