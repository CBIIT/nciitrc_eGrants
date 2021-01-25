using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace egrants_new.Integration.Models
{
    public class WSNodeMapping
    {
        public int WSNodeMapping_Id { get; set; }
        public string NodeName { get; set; }
        public string DataType { get; set; }
//        public string DestinationTable { get; set; }
        public string DestinationField { get; set; }
        public bool TransformData { get; set; }
        public string TransformationFunc { get; set; }
        public bool IsPrimaryKey { get; set; }
    }
}