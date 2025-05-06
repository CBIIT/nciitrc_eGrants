using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGARequestAccountDisable.DTOs
{
    internal class ErrorInfo
    {
        public string ErrorType { get; set; }
        public DateTime RecordedTime { get; set; }
        public int GrantId { get; set; }
        public string ErrorMessage { get; set; }
        public int ApplId { get; set; }
        public string GrantYear { get; set; }
    }
}
