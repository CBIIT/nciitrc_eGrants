using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace egrants_new.Integration.Models
{
    public class SqlJobError
    {

        public int ErrorId { get; set; }
        public string UserName { get; set; }
        public int ErrorNumber { get; set; }
        public int ErrorState { get; set; }
        public int ErrorSeverity { get; set; }
        public int ErrorLine { get; set; }
        public string ErrorProcedure { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime ErrorDateTime { get; set; }
        public string JobName { get; set; }
        public string JobId { get; set; }
        public int StepId { get; set; }
        public bool EmailSent { get; set; }

	}
}