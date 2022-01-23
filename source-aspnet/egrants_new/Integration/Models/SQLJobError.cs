using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace egrants_new.Integration.Models
{
    public class SqlJobError
    {

        public int ErrorId;
        public string UserName;
        public int ErrorNumber;
        public int ErrorState;
        public int ErrorSeverity;
        public int ErrorLine;
        public string ErrorProcedure;
        public string ErrorMessage;
        public DateTime ErrorDateTime;
        public string JobName;
        public string JobId;
        public int StepId;
        public bool EmailSent;

	}
}