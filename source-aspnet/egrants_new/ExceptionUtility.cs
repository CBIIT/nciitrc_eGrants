#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  Global.asax.cs
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
using System.IO;
using System.Web;

//using Microsoft.ApplicationInsights.Extensibility;

#endregion

namespace egrants_new
{

        // Create our own utility for exceptions 
        /// <summary>
        /// The exception utility.
        /// </summary>
        public sealed class ExceptionUtility
        {
            // All methods are static, so this can be private 
            /// <summary>
            /// Prevents a default instance of the <see cref="ExceptionUtility"/> class from being created.
            /// </summary>
            private ExceptionUtility()
            {
            }

            // Log an Exception 
            /// <summary>
            /// The log exception.
            /// </summary>
            /// <param name="exc">
            /// The exc.
            /// </param>
            /// <param name="source">
            /// The source.
            /// </param>
            public static void LogException(Exception exc, string source)
            {
                // Include enterprise logic for logging exceptions 
                // Get the absolute path to the log file 
                var logFile = "~/App_Data/ErrorLog.txt";
                logFile = HttpContext.Current.Server.MapPath(logFile);

                // if (!File.Exists(logFile))
                // {
                // byte[] file = new byte[0];
                // File.Create(logFile);
                // }

                // Open the log file for append and write the log
                var sw = new StreamWriter(logFile, true);
                sw.WriteLine("********** {0} **********", DateTime.Now);

                if (exc.InnerException != null)
                {
                    sw.Write("Inner Exception Type: ");
                    sw.WriteLine(exc.InnerException.GetType().ToString());
                    sw.Write("Inner Exception: ");
                    sw.WriteLine(exc.InnerException.Message);
                    sw.Write("Inner Source: ");
                    sw.WriteLine(exc.InnerException.Source);

                    if (exc.InnerException.StackTrace != null)
                    {
                        sw.WriteLine("Inner Stack Trace: ");
                        sw.WriteLine(exc.InnerException.StackTrace);
                    }
                }

                sw.Write("Exception Type: ");
                sw.WriteLine(exc.GetType().ToString());
                sw.WriteLine("Exception: " + exc.Message);
                sw.WriteLine("Source: " + source);
                sw.WriteLine("Stack Trace: ");

                if (exc.StackTrace != null)
                {
                    sw.WriteLine(exc.StackTrace);
                    sw.WriteLine();
                }

                sw.Close();
            }

            /// <summary>
            /// Notify System Operators about an exception
            /// </summary>
            /// <param name="exc">
            /// The exc.
            /// </param>
            public static void NotifySystemOps(Exception exc)
            {
                // Include code for notifying IT system operators
            }
        }
    }
