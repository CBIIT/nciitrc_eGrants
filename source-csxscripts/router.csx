// Install support for CSX scripts (C# scripts) using this command on Windows Powershell:
//      (new-object Net.WebClient).DownloadString("https://raw.githubusercontent.com/dotnet-script/dotnet-script/master/install/install.ps1") | iex
// Run using :
//      dotnet script router.csx
// MLH 5/16/2024 Update : According to the interwebs, Office interop is not supported by .NET Core :(
// migrating the code from here to Dot Net Core

#r "nuget: SqlConnection, 1.0.4"
#r "C:\Users\hooverrl\Desktop\NCI\nciitrc_eGrants\source-csxscripts\Interop.Microsoft.Office.Interop.Outlook.dll"

using System.Data.SqlClient;
using Outlook = Microsoft.Office.Interop.Outlook;

var _startTimeStamp = DateTime.Now;

var _verbose = GetConfigVal("Verbose");
ShowDiagnosticIfVerbose($"_verbose: '{_verbose}'", _verbose);
var _debug = GetConfigVal("dBug");
ShowDiagnosticIfVerbose($"_debug: '{_debug}'", _verbose);
var _logDir = GetConfigVal("logDir");
ShowDiagnosticIfVerbose($"_logDir: '{_logDir}'", _verbose);
var _conStr = GetConfigVal("conStr");
ShowDiagnosticIfVerbose($"_conStr: '{_conStr}'", _verbose);
var _dirPath = GetConfigVal("dirpathRouter");
ShowDiagnosticIfVerbose($"_dirPath: '{_dirPath}'", _verbose);

ShowDiagnosticIfVerbose("Running the router", _verbose);

int _forAppending = 8;
var _taskStartMssg = "...........Task Started!...........";
WriteLog(_forAppending, _taskStartMssg, null, _startTimeStamp);

var _dBugEmail = "leul.ayana@nih.gov";
var _eGrantsDevEmail = "eGrantsDev@mail.nih.gov";
var _eGrantsTestEmail = "eGrantsTest1@mail.nih.gov";
var _eGrantsStageEmail = "eGrantsStage@mail.nih.gov";
var _eFileEmail = "efile@mail.nih.gov";
var _nciGrantsPostAwardEmail = "NCIGrantsPostAward@nih.gov";

SqlConnection _con = new SqlConnection(_conStr);

var _itemsProcessed = Process(_dirPath, _con, _verbose, _debug);

var _taskEndMssg = $"******* Task Completed! ******* {_itemsProcessed} Mail Items Have Been Processed";
var _endTimeStamp = DateTime.Now;
WriteLog(_forAppending, _taskEndMssg, null, _endTimeStamp);

public int Process(string dirPath, SqlConnection con, string verbose, string debug) {
    var itemsProcessed = 0;

    ShowDiagnosticIfVerbose("Hello you are in Process", verbose);

    Outlook.Application oApp = new Outlook.Application();
    ShowDiagnosticIfVerbose("Created the object ...", verbose);

    //string sepchar = "\";

    //var dirPathSections = dirPath.Split(sepchar);
    //var i = 1;
    //var CFolder = dirPathSections[0];
    //var appOutlook = new Microsoft.Office.Interop.Outlook.Application();

    	// 	xArray = Split(dirpath, sepchar)
		// i = 1
		// call ShowDiagnosticIfVerbose("Setting objNS.Folders to " & xArray(0), Verbose)
		// Set CFolder = objNS.Folders(xArray(0))
		// call ShowDiagnosticIfVerbose("Set", Verbose)

		// Do While i < UBound(xArray)
		// 	call ShowDiagnosticIfVerbose("xArray i " & xArray(i), Verbose)
		// 	Set CFolder = CFolder.Folders(xArray(i))
		// 	i = i + 1
		// Loop
    
    //var CFolder = GetFolder(dirPath, appOutlook);

    return itemsProcessed;
}

// Returns Folder object based on folder path
private Outlook.Folder GetFolder(string folderPath, Microsoft.Office.Interop.Outlook.Application application)
{
    Outlook.Folder folder;
    string backslash = @"\";
    try
    {
        if (folderPath.StartsWith(@"\\"))
        {
            folderPath = folderPath.Remove(0, 2);
        }
        String[] folders =
            folderPath.Split(backslash.ToCharArray());
        folder = 
            application.Session.Folders[folders[0]]
            //Outlook.Folders[folder[0]]
            //Outlook.Session.Folders[folder[0]]            // "Session" doesn't exist here
            //Application.Session.Folders[folders[0]]
            as Outlook.Folder;
        if (folder != null)
        {
            for (int i = 1; i <= folders.GetUpperBound(0); i++)
            {
                Outlook.Folders subFolders = folder.Folders;
                folder = subFolders[folders[i]]
                    as Outlook.Folder;
                if (folder == null)
                {
                    return null;
                }
            }
        }
        return folder;
    }
    catch { return null; }
}    

public void WriteLog(int code, string message, Exception errorInfo, DateTime timeStamp) {
    var fileName = $"eMailRouter-Log-{timeStamp.Year}-{timeStamp.Month}-{timeStamp.Day}.txt";

    var outputContent = string.Empty;
    if (errorInfo == null) {
        outputContent = $"{timeStamp}  -\t{message}";
    } else {
        outputContent = $"{timeStamp}  -\t{message}\t\t\t{errorInfo.Message}";
    }
    
    File.AppendAllText(_logDir + "\\" + fileName, outputContent + Environment.NewLine);
}

public static void ShowDiagnosticIfVerbose(string Message, string Verbose)
{
    if (Verbose.ToLower().Contains("y")) {
        Console.WriteLine(Message);
    }
}



public static string GetConfigVal(string name)
{
    string delimiter = ",,,,,";
    foreach (string line in File.ReadLines(@"config.csv"))
    {
        var sections = line.Split(delimiter);
        if (sections.Length > 1) {
            var key = sections[0];
            var value = sections[1];
            if (key.Equals(name)) {
                return value;
            }
        }
    }
    return "FAILED TO FIND VALUE";
}


public static void WaitForDebugger()
{
    Console.WriteLine("Attach Debugger (VS Code)");
    while(!Debugger.IsAttached)
    {
    }
}

