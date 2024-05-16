using System.Data;
using System.Data.OleDb;
using System.Text;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace OutlookAccessADO
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            String sqlText = "SELECT Subject, Contents FROM Inbox";

            // Build the connection string.
            String connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" +
                "Outlook 9.0;" +
                //"MAPILEVEL=" + mailboxNameTextBox.Text + "|;" +
                // "PROFILE=" + profileTextBox.Text + ";" +
                "TABLETYPE=0;" +
                "DATABASE=" + System.IO.Path.GetTempPath();

            // Create the DataAdapter.
            OleDbDataAdapter da = new OleDbDataAdapter(sqlText, connectionString);

            // Create and fill the table.
            DataTable dt = new DataTable("Inbox");
            try
            {
                da.Fill(dt);
                // dataGrid.DataSource = dt.DefaultView;
                outputTextBox.Text = dt.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
                return;
            }

        }

        private void interOpButton_Click(object sender, EventArgs e)
        {
            // Office 365 should reference Office 16 ... prolly different from the version listed on the DLL ...

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

            outputTextBox.Text = sb.ToString();
        }
    }
}
