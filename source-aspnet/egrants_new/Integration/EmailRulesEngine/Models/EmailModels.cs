using System.Linq;
using System.Web;
using egrants_new.Integration.EmailRulesEngine;

namespace egrants_new.Integration.EmailRulesEngine.Models
{
    public class EmailFields
    {
        public int Id { get; set; }
        public string FieldName { get; set; }
        public int DataTypeId { get; set; }
    }

    public class EmailFieldDataTypes
    {
        public int Id { get; set; }
        public string Type { get; set; }
    }
}