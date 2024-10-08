using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailConcatenationPOC
{
    internal class Constants
    {
        public static readonly List<string> SupportedImageTypes = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".tif" };
        public static readonly List<string> UndiscoveredFileTypes = new List<string> { ".wpd", ".mdi", ".oft", ".mht" };
        public static readonly List<string> ExplicitlyUnsupportedFileTypes = new List<string> { ".nal", ".qrp", ".exe", ".dot", ".pd", ".ms", ".wma", ".zip", ".inf", ".doc" };
        public static readonly List<string> FormattedTextTypes = new List<string> { ".txt", ".log", ".dat" };
        public static readonly List<string> ExcelTypes = new List<string> { ".xslt", ".xls", ".xlsm", ".xlsb", ".xltx", ".xlm", ".slk", ".xlt", ".xlw" };

        // example 1 ... no embedded message files
        public const string ExamplePath1 = ".\\Outlook\\example_email_to_be_concatenated.msg";

        // example 2 ... contains embedded message files
        public const string ExamplePath2 = ".\\Outlook\\mixed_message_example.msg";

        // example 3 ... contains embedded excel file
        public const string ExamplePath3 = ".\\Excel\\email_test_with_single_sheet_excel_html.msg";

        // example 4 ... contains gif, jpg, png, tif image types as attachments
        public const string ExamplePath4 = ".\\Images\\four_image_types.msg";

        // example 5 ... simple text
        public const string ExamplePath5 = ".\\Text\\simple_text_attach.msg";

        // example 6 ... all (non-RTF) text types
        public const string ExamplePath6 = ".\\Text\\all_simple_text_types.msg";

        // example 7 ... RTF 1 (from Word) (has weird stuff at the beginning and a TON at the end ... (renders the junk in Acrobat and Chrome))
        public const string ExamplePath7 = ".\\Text\\RTF_email_attachment_example.msg";

        // example 8 ... RTF 2 (from Word Pad, PDF converion looks great)
        public const string ExamplePath8 = ".\\Text\\RTF_file_from_wordpad.msg";

        // example 9 ... RTF 3 (from Google)
        public const string ExamplePath9 = ".\\Text\\rtf_from_google_docs.msg";

        // example 10 ... mixed security (cheesecake pdf)
        public const string ExamplePath10 = ".\\Secure\\attachments_mixed_security.msg";
        //IronSoftware.Exceptions.IronSoftwareNativeException: 'Error while opening document from bytes:
        //'Error while opening document from 132497 bytes: Invalid password'.

        public const string ExamplePath11 = ".\\Fillable\\fillable_form.msg";

        public const string ExamplePath12 = ".\\Unsupported\\unsupported_file_types.msg";

        // simple, obsolete Excel (xls) .. majority of eGrants spreadsheets are this
        public const string ExamplePath13 = ".\\Excel\\three_excel_versions.msg";

        // bold, italic, font size, borders, shading, underline, font color, justification
        public const string ExamplePath14 = ".\\Excel\\complex_excel.msg";

        // xlsm, formatting test, improved width for test
        public const string ExamplePath15 = ".\\Excel\\complex_excel_files_b.msg";

        // added missing xls file
        public const string ExamplePath16 = ".\\Excel\\complex_excel_b2.msg";

    }
}
