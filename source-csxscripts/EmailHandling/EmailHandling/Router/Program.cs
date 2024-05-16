using System.Data;
using System.Data.OleDb;
using System.Text;
using Outlook = Microsoft.Office.Interop.Outlook;


// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var sb = new StringBuilder();

Console.WriteLine("Here we go ...");
sb.Append("Here we go ...\r\n");
Outlook.Application oApp = new Outlook.Application();
Console.WriteLine("Created the object ...");
sb.Append("Created the object ...\r\n");
Outlook.NameSpace oNS = oApp.GetNamespace("MAPI");
oNS.Logon("", "", false, true);
Outlook.MAPIFolder oInbox = oNS.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
Outlook.Items oItems = oInbox.Items;
Console.WriteLine(oItems.Count);
sb.Append($"Found {oItems.Count} many emails\r\n");
Outlook.MailItem oMsg = (Outlook.MailItem)oItems.GetFirst();
Console.WriteLine(oMsg.Subject);
sb.Append($"Here's the first subject : {oMsg.Subject}\r\n");
oNS.Logoff();
