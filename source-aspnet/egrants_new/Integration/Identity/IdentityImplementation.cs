using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Identity.Client;


//namespace egrants_new.Integration.Identity
//{
//    public class IdentityImplementation
//    {

//        string[] scopes = new string[] {"https://graph.microsoft.com/.default"};

//        AuthenticationResult result = null;

//        public void Authenticate()
//        {
//            try
//            {
//                result = await app.AcquireTokenForClient(scopes)
//                    .ExecuteAsync();
//            }
//            catch (MsalUiRequiredException ex)
//            {
//                // The application doesn't have sufficient permissions.
//                // - Did you declare enough app permissions during app creation?
//                // - Did the tenant admin grant permissions to the application?
//            }
//            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
//            {
//                // Invalid scope. The scope has to be in the form "https://resourceurl/.default"
//                // Mitigation: Change the scope to be as expected.

//            }


//        }

//    }
//}