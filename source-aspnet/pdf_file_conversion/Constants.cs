using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailConcatenation
{
    internal class Constants
    {
        public static readonly List<string> SupportedImageTypes = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".tif" };
        public static readonly List<string> UndiscoveredFileTypes = new List<string> { ".wpd", ".mdi", ".oft", ".mht" };
        public static readonly List<string> ExplicitlyUnsupportedFileTypes = new List<string> { ".nal", ".qrp", ".exe", ".dot", ".pd", ".ms", ".wma", ".zip", ".inf", ".doc" };
        public static readonly List<string> FormattedTextTypes = new List<string> { ".txt", ".log", ".dat" };
        public static readonly List<string> ExcelTypes = new List<string> { ".xlst", ".xls", ".xlsm", ".xlsb", ".xltx", ".xlm", ".slk", ".xlw" };
        public static readonly List<string> UnsupportedExcelTypes = new List<string> { ".xlst", ".xlsb", ".xltx", ".xlm", ".slk", ".xlt", ".xlw" };

        // example 1 ... no embedded message files
        public const string ExamplePath1 = ".\\Outlook\\example_email_to_be_concatenated.msg";

        // example 2 ... contains embedded message files
        public const string ExamplePath2 = ".\\Outlook\\mixed_message_example.msg";

        // example 3 ... contains gif, jpg, png, tif image types as attachments
        public const string ExamplePath3 = ".\\Images\\four_image_types.msg";

        // example 4 ... simple text
        public const string ExamplePath4 = ".\\Text\\simple_text_attach.msg";

        // example 5 ... all (non-RTF) text types
        public const string ExamplePath5 = ".\\Text\\all_simple_text_types.msg";

        // example 6 ... RTF 1 (from Word) (has weird stuff at the beginning and a TON at the end ... (renders the junk in Acrobat and Chrome))
        public const string ExamplePath6 = ".\\Text\\RTF_email_attachment_example.msg";

        // example 7 ... RTF 2 (from Word Pad, PDF converion looks great)
        public const string ExamplePath7 = ".\\Text\\RTF_file_from_wordpad.msg";

        // example 8 ... RTF 3 (from Google)
        public const string ExamplePath8 = ".\\Text\\rtf_from_google_docs.msg";

        // example 9 ... mixed security (cheesecake pdf)
        public const string ExamplePath9 = ".\\Secure\\attachments_mixed_security.msg";
        //IronSoftware.Exceptions.IronSoftwareNativeException: 'Error while opening document from bytes:
        //'Error while opening document from 132497 bytes: Invalid password'.

        public const string ExamplePath10 = ".\\Fillable\\fillable_form.msg";

        public const string ExamplePath11 = ".\\Unsupported\\unsupported_file_types.msg";

        // added missing xls file
        public const string ExamplePath12 = ".\\Excel\\complex_excel_b2.msg";

        // very formatted Word files (docx and doc)
        public const string ExamplePath13 = ".\\Word\\very_formatted_word.msg";

        // forumulas
        public const string ExamplePath14 = ".\\Excel\\calcs_test.msg";

        // xlt 
        public const string ExamplePath15 = ".\\Excel\\xlt_email.msg";

        // kitchen sink 
        public const string ExamplePath16 = ".\\kitchen_sink_test.msg";

        // lisa test
        public const string ExamplePathLisa = ".\\lisa_test.msg";

        // smaller version of lisa test, checking for quality, accuracy, completeness
        public const string ExamplePathLisaSmall = ".\\Excel\\simple_small_excel_example.msg";

        // mega example xlsx version
        public const string ExamplePathMegaXls = ".\\mega_xls_example2.msg";
    }
}
