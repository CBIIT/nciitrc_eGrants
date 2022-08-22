using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace egrants_new.Egrants.Models
{
    public class DownloadModel
    {
        /// <summary>
        /// Gets or sets the appl_id.
        /// </summary>
        public string ApplId { get; set; }

        public string Handle { get; set; }

        public List<DownloadData> DownloadDataList { get; set; }
    }

    public class DownloadData
    {
        public string Url { get; set; }

        public string FileDownloaded { get; set; }

        public string Error { get; set; }
    }
}