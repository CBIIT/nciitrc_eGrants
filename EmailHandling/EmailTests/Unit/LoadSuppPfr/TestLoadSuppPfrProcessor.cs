using System;
using System.Collections.Generic;
using System.Xml;
using LoadSuppPfr;
using CommonUtilties;

namespace EmailHandlingTests.Unit.LoadSuppPfr
{
    internal class TestLoadSuppPfrProcessor : Processor
{
        public List<TestSuppPfrRecord> SuppPfrRecordsProcessedThisSession { get; } = new List<TestSuppPfrRecord>();
        public List<TestFileCopyOperation> FileCopyOperations { get; } = new List<TestFileCopyOperation>();
   public List<TestFileBackupOperation> FileBackupOperations { get; } = new List<TestFileBackupOperation>();
     public int ProcessedCount { get; private set; }
        public bool ErrorOccurred { get; private set; }
        public string LastErrorMessage { get; private set; }
        public List<SimulatedSuppPfrXmlFile> SimulatedXmlFiles { get; set; } = new List<SimulatedSuppPfrXmlFile>();

   public TestSuppPfrRecord TestProcessSingleXmlRecord(string applId, string folderId, string fileName,
     string docDate, string fileType, string verbose = "n")
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

     var record = new TestSuppPfrRecord
                {
            ApplId = applId,
          CategoryName = catName,
    FileName = fileName,
       DocumentDate = docDate,
 FileType = fileType,
      SubjectLine = BuildSubjectLine(applId),
      SubCategory = "PFR",
      ProcessedTime = DateTime.Now,
   WasInsertedToDatabase = true
  };

          SuppPfrRecordsProcessedThisSession.Add(record);

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

    public TestSuppPfrXmlData ParseXmlNodePublic(string xmlContent)
        {
    var data = new TestSuppPfrXmlData();
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
             }
        }
                }
            }
        catch { }

         return data;
        }

    public bool IsValidSuppPfrFolder(string folderId)
 {
      return folderId == "19";
  }

        public string BuildSubjectLine(string applId)
        {
      if (string.IsNullOrEmpty(applId)) return "";
          return $"Supplement PFR - {applId}";
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

        public void AddSimulatedXmlFile(string xmlFileName, List<SimulatedSuppPfrRecord> records)
   {
            SimulatedXmlFiles.Add(new SimulatedSuppPfrXmlFile
            {
        XmlFileName = xmlFileName,
          Records = records
          });
        }

   public void AddSimulatedXmlFileWithSingleRecord(string xmlFileName, string applId, string folderId,
            string fileName, string docDate, string fileType)
        {
            var records = new List<SimulatedSuppPfrRecord>
    {
                new SimulatedSuppPfrRecord
    {
     ApplId = applId,
   FolderId = folderId,
FileName = fileName,
    DocDate = docDate,
             FileType = fileType
 }
       };
    AddSimulatedXmlFile(xmlFileName, records);
      }

        public void Reset()
 {
  SuppPfrRecordsProcessedThisSession.Clear();
        FileCopyOperations.Clear();
         FileBackupOperations.Clear();
      ProcessedCount = 0;
         ErrorOccurred = false;
    LastErrorMessage = null;
            SimulatedXmlFiles.Clear();
  }
    }

    public class TestSuppPfrRecord
    {
        public string ApplId { get; set; }
        public string CategoryName { get; set; }
        public string FileName { get; set; }
        public string DocumentDate { get; set; }
     public string FileType { get; set; }
        public string SubjectLine { get; set; }
    public string SubCategory { get; set; }
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

  public class SimulatedSuppPfrXmlFile
    {
        public string XmlFileName { get; set; }
        public List<SimulatedSuppPfrRecord> Records { get; set; } = new List<SimulatedSuppPfrRecord>();
    }

    public class SimulatedSuppPfrRecord
    {
        public string ApplId { get; set; }
        public string FolderId { get; set; }
        public string FileName { get; set; }
        public string DocDate { get; set; }
 public string FileType { get; set; }
    }

    public class TestSuppPfrXmlData
    {
        public string ApplId { get; set; }
        public string FolderId { get; set; }
     public string CategoryName { get; set; }
      public string FileName { get; set; }
        public string DocDate { get; set; }
        public string FileType { get; set; }
    }
}
