using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace egrants_new.Integration.Models
{
    public class IntegrationEnums
    {
        public enum AuthenticationType
        {
            UserPassword = 0,
            Certificate,
            OAuth
        }

        public enum ReconciliationBehavior
        {
            AddAllDuplicationOk = 0, //This will work for the first integration because we'll ony be querying for most recent records by date
            AddNewOnly,  //Implement Later
            AddNewUpdateExisting, //Implement Later 
            UpdateOnlyRejectNew //Possibly Implement Later
        }

        public enum DateTimeUnits
        {
            Seconds = 0,
            Minutes,
            Hours,
            Days,
            Weeks,
            Months,
            Years
        }

        public enum Interval
        {
            Interval = 0,
            AtSetDateTime

        }

        public enum EvalType
        {
            Contains = 0,
            StartsWith,
            EndsWith,
            Equals,
            LessThan,
            GreaterThan,
            NotEqual
        }

        public enum CriteriaType
        {
            Generic
        }


        public enum EmailActionType
        {
            EmailFileCopyMoveAction,
            EmailForwardAction,
            EmailCreateTextFileAction,
            EmailCreatePdfAction,
            EmailCreateSendNewEmailAction, 
            EmailAdminSupplementProcess
        }

        public enum SaveType
        {
            eRaWebServiceData,
            MicrosoftGraphApi
        }

        public enum MetadataType
        {
            String,
            Integer,
            Date,
            Decimal,

        }

        public enum MessageSaveType
        {
            Text,
            Html,
            Pdf
        }
    }
}