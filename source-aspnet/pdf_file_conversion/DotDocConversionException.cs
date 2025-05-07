using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdf_file_conversion
{
    public class DotDocConversionException : Exception
    {
        string _Message;

        public DotDocConversionException(string message) : base(message)
        {
            _Message = message;
        }

        public override string Message
        {
            get { return _Message; }
        }
    }
}
