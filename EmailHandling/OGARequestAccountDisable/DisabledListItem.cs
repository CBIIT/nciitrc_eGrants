using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGARequestAccountDisable
{
    public class DisabledListItem
    {
        public int PersonIdFromDB { get; set; }
        public string EmailFromDB { get; set; }

        public string PersonNameFromDB { get; set; }
        public string FirstNameFromDB { get; set; }
        public string LastNameFromDB { get; set; }

        public string FinalNameForOGA { get; set; }

        public bool FailedToRenderName { get; set; }
    }
}
