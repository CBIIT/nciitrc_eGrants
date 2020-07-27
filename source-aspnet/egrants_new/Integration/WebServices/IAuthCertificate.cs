using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace egrants_new.Integration.WebServices
{
    interface IAuthCertificate
    {
        string CertificatePath { get; set; }
        string CertificatePwd { get; set; }
    }
}
