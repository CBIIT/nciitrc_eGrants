using System;
using System.Collections.Generic;
using System.Xml;
using LoadPfr;
using CommonUtilties;

namespace EmailTests.LoadPfr
{
    internal class TestLoadPfrProcessor : Processor
    {
        public List<TestPfrRecord> PfrRecordsProcessedThisSession { get; } = new List<TestPfrRecord>();
        public List<TestFileCopyOperation> FileCopyOperations { get; } = new List<TestFileCopyOperation>();
        public List<TestFileBackupOperation> FileBackupOperations { get; } = new List<TestFileBackupOperation>();
  public int ProcessedCount { get; private set; }
      public bool ErrorOccurred { get; private set; }
        public string LastErrorMessage { get; private set; }
        public List<SimulatedPfrXmlFile> SimulatedXmlFiles { get; set; } = new List<SimulatedPfrXmlFile>();

  public TestPfrRecord TestProcessSingleXmlRecord(string applId, string folderId, string fileName,
      string docDate, string fileType, string createdBy, string verbose = "n")
        {
            try
            {
           // Validate folder ID - only process if folderId is "19" (PFR)
      string catName = "";
                if (folderId == "19") catName = "PFR";

   if (string.IsNullOrEmpty(applId) || string.IsNullOrEmpty(fileName))
  {
       return null;
  }

       ProcessedCount++;

    var record = new TestPfrRecord
             {
    ApplId = applId,
  CategoryName = catName,
        FileName = fileName,
  DocumentDate = docDate,
           FileType = fileType,
        CreatedBy = createdBy,
      ProcessedTime = DateTime.Now,
    WasInsertedToDatabase = true
    };

        PfrRecordsProcessedThisSession.Add(record);

   // Simulate file copy operation
  string alias = $"placeholder.{fileType}";
            FileCopyOperations.Add(new TestFileCopyOperation
      {
    SourceFile = fileName,
     DestinationFile = alias,
  DestinationType = "FinalDestination",
           Success = true
                });

          // Simulate backup operations
       FileBackupOperations.Add(new TestFileBackupOperation
                {
         FileName = fileName,
   BackupType = "PDF",
 Success = true
            });

                return record;
    }
        catch (Exception ex)
   {
 ErrorOccurred = true;
LastErrorMessage = ex.Message;
          return null;
         }
        }

        public int TestProcessSimulatedXmlFiles(string verbose = "n")
        {
          try
   {
        int processed = 0;
     foreach (var xmlFile in SimulatedXmlFiles)
        {
          foreach (var record in xmlFile.Records)
        {
      var result = TestProcessSingleXmlRecord(
          record.ApplId,
record.FolderId,
 record.FileName,
         record.DocDate,
        record.FileType,
        record.CreatedBy,
                verbose);
    if (result != null) processed++;
          }
             
    // Simulate XML file backup
            FileBackupOperations.Add(new TestFileBackupOperation
   {
          FileName = xmlFile.XmlFileName,
     BackupType = "XML",
     Success = true
   });
      }
     return processed;
}
         catch (Exception ex)
    {
            ErrorOccurred = true;
        LastErrorMessage = ex.Message;
    return ProcessedCount;
 }
      }

        public TestPfrXmlData ParseXmlNodePublic(string xmlContent)
   {
         var data = new TestPfrXmlData();
   if (string.IsNullOrEmpty(xmlContent)) return data;

    try
            {
  var xmlDoc = new XmlDocument();
    xmlDoc.LoadXml(xmlContent);

    var documentElement = xmlDoc.DocumentElement;
       if (documentElement == null) return data;

      foreach (XmlNode listNode in documentElement.ChildNodes)
      {
       foreach (XmlNode fieldNode in listNode.ChildNodes)
             {
           switch (fieldNode.Name.ToLower())
{
        case "applid": data.ApplId = fieldNode.InnerText; break;
      case "folderid": 
     data.FolderId = fieldNode.InnerText;
         if (fieldNode.InnerText == "19") data.CategoryName = "PFR"; 
    break;
      case "filename": data.FileName = fieldNode.InnerText; break;
      case "date": data.DocDate = fieldNode.InnerText; break;
             case "file_type": data.FileType = fieldNode.InnerText; break;
              case "uid": data.CreatedBy = fieldNode.InnerText; break;
          }
    }
    }
       }
  catch { }

   return data;
   }

        public bool IsValidPfrFolder(string folderId)
  {
 return folderId == "19";
        }

        public string BuildDestinationAlias(string fileNumberName, string fileType)
        {
            if (string.IsNullOrEmpty(fileNumberName)) return null;
 return $"{fileNumberName}.{fileType}";
   }

    public bool IsValidFileExtension(string fileType)
     {
     if (string.IsNullOrEmpty(fileType)) return false;
    string lower = fileType.ToLower();
        return lower == "pdf" || lower == "doc" || lower == "docx" || lower == "txt";
      }

     public void AddSimulatedXmlFile(string xmlFileName, List<SimulatedPfrRecord> records)
        {
  SimulatedXmlFiles.Add(new SimulatedPfrXmlFile
  {
       XmlFileName = xmlFileName,
       Records = records
   });
   }

  public void AddSimulatedXmlFileWithSingleRecord(string xmlFileName, string applId, string folderId,
            string fileName, string docDate, string fileType, string createdBy)
        {
     var records = new List<SimulatedPfrRecord>
 {
 new SimulatedPfrRecord
             {
            ApplId = applId,
        FolderId = folderId,
         FileName = fileName,
      DocDate = docDate,
     FileType = fileType,
      CreatedBy = createdBy
       }
            };
            AddSimulatedXmlFile(xmlFileName, records);
        }

    public void Reset()
        {
    PfrRecordsProcessedThisSession.Clear();
 FileCopyOperations.Clear();
            FileBackupOperations.Clear();
            ProcessedCount = 0;
            ErrorOccurred = false;
     LastErrorMessage = null;
            SimulatedXmlFiles.Clear();
        }
    }

    public class TestPfrRecord
    {
      public string ApplId { get; set; }
   public string CategoryName { get; set; }
        public string FileName { get; set; }
        public string DocumentDate { get; set; }
    public string FileType { get; set; }
     public string CreatedBy { get; set; }
        public DateTime ProcessedTime { get; set; }
   public bool WasInsertedToDatabase { get; set; }
    }

    public class TestFileCopyOperation
    {
        public string SourceFile { get; set; }
        public string DestinationFile { get; set; }
      public string DestinationType { get; set; }
        public bool Success { get; set; }
    }

    public class TestFileBackupOperation
    {
        public string FileName { get; set; }
        public string BackupType { get; set; }
        public bool Success { get; set; }
}

    public class SimulatedPfrXmlFile
    {
        public string XmlFileName { get; set; }
        public List<SimulatedPfrRecord> Records { get; set; } = new List<SimulatedPfrRecord>();
    }

    public class SimulatedPfrRecord
    {
        public string ApplId { get; set; }
 public string FolderId { get; set; }
        public string FileName { get; set; }
        public string DocDate { get; set; }
        public string FileType { get; set; }
        public string CreatedBy { get; set; }
    }

    public class TestPfrXmlData
    {
        public string ApplId { get; set; }
  public string FolderId { get; set; }
        public string CategoryName { get; set; }
        public string FileName { get; set; }
        public string DocDate { get; set; }
 public string FileType { get; set; }
     public string CreatedBy { get; set; }
    }
}
