# EmailHandling Tests

Comprehensive test suite for the eGrants Email Handling projects, providing unit tests, integration tests, and mocks for Outlook COM automation.

## Overview

The EmailTests project provides:
- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test database and file system operations
- **Mocking Framework**: Mock Outlook COM objects for testing
- **Test Fixtures**: Reusable test data and configurations
- **Performance Tests**: Verify processing efficiency
- **Regression Tests**: Ensure bug fixes remain fixed

## Test Coverage

### Projects Tested

- ? **Router**: Email routing logic and patterns
- ? **ExchangeFixed**: Document processing and PDF generation
- ? **AddSuppEmailer**: Notification email sending
- ? **AddSuppProd**: Supplement request processing
- ? **AddSuppVoteCollection**: Vote collection processing
- ? **CommonUtilities**: Shared utility functions
- ? **DocManEmail**: Document management integration
- ? **LoadPfr**: PFR XML processing
- ? **LoadSuppPfr**: Supplement PFR XML processing
- ? **EGrantsAcmAuditReport**: Audit report processing
- ? **OGARequestAccountDisable**: Account disable automation
- ? **StartOutlook**: Outlook startup utility

### Test Categories

1. **Unit Tests**: Pure logic testing
2. **Integration Tests**: Database and file system
3. **Smoke Tests**: End-to-end executable launch and configuration validation
4. **Process Tests**: Full process execution with Outlook availability checks
5. **COM Automation Tests**: Outlook integration (with mocks)
6. **Email Parsing Tests**: Subject line parsing
7. **Database Tests**: SQL operations
8. **File Processing Tests**: File I/O operations
9. **Logging Tests**: Output and error handling verification
10. **Configuration Tests**: Connection string and environment variable validation

## Running Tests

### All Tests

```bash
cd EmailTests
dotnet test
```

### Specific Test Category

```bash
# Unit tests only
dotnet test --filter Category=Unit

# Integration tests only
dotnet test --filter Category=Integration

# Smoke tests only (executable launch, dependencies, logging)
dotnet test --filter Category=SmokeTest

# Process tests only (end-to-end executable execution)
dotnet test --filter Category=Process

# Specific project tests
dotnet test --filter FullyQualifiedName~Router
```

**For detailed smoke test documentation, see [`ProcessSmokeTests/README.md`](ProcessSmokeTests/README.md).**

### With Code Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Verbose Output

```bash
dotnet test --logger "console;verbosity=detailed"
```

## Test Structure

### Directory Layout

```
EmailTests/
??? Unit/
?   ??? RouterTests/
?   ??? ExchangeFixedTests/
?   ??? CommonUtilitiesTests/
?   ??? ...
??? Integration/
?   ??? DatabaseTests/
?   ??? FileSystemTests/
?   ??? OutlookTests/
??? Mocks/
?   ??? MockOutlookObjects.cs
?   ??? MockDatabase.cs
?   ??? MockFileSystem.cs
??? Fixtures/
?   ??? TestData.cs
?   ??? TestConfiguration.cs
?   ??? TestHelpers.cs
??? README.md
```

## Key Test Classes

### RouterProcessorTests

Tests Router email processing logic:
```csharp
[Test]
public void HandleSingleEmail_FCOI_ExtractsApplicationId()
{
    // Arrange
    var processor = new Processor();
    var mockEmail = CreateMockFCOIEmail();

    // Act
    processor.HandleSingleEmail(mockEmail, ...);

    // Assert
    Assert.That(processor.ExtractedApplicationId, Is.EqualTo("12345678"));
}
```

### ExchangeFixedProcessorTests

Tests document processing:
```csharp
[Test]
public void ProcessExtractBody_PublicAccess_GeneratesPDF()
{
    // Test PDF generation for Public Access category
}
```

### CommonUtilitiesTests

Tests utility functions:
```csharp
[Test]
public void RemoveSpaceCharacters_RemovesAllSpecialChars()
{
    // Arrange
    string input = "5R01CA258784-04";

    // Act
    string result = CommonUtilities.RemoveSpaceCharacters(input);

    // Assert
    Assert.That(result, Is.EqualTo("5R01CA25878404"));
}
```

## Mocking Outlook COM Objects

### MockMailItem

Simulates Outlook MailItem:
```csharp
public class MockMailItem
{
    public string Subject { get; set; }
    public string Body { get; set; }
    public string SenderEmailAddress { get; set; }
    public DateTime ReceivedTime { get; set; }
    public MockAttachments Attachments { get; set; }

    public MockMailItem Forward()
    {
        return new MockMailItem { Subject = "FW: " + Subject };
    }
}
```

### MockOutlookApplication

Simulates Outlook.Application:
```csharp
public class MockOutlookApplication
{
    public MockNamespace GetNamespace(string type)
    {
        return new MockNamespace();
    }

    public MockMailItem CreateItem(int itemType)
    {
        return new MockMailItem();
    }
}
```

## Test Data

### Sample Emails

Located in `Fixtures/TestData.cs`:
```csharp
public static class TestEmails
{
    public static MockMailItem CreateFCOIEmail() { ... }
    public static MockMailItem CreatePublicAccessEmail() { ... }
    public static MockMailItem CreateJITEmail() { ... }
    // ... more email types
}
```

### Sample Configuration

```csharp
public static class TestConfiguration
{
    public static IConfiguration GetTestConfig()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["AppSettings:Debug"] = "y",
                ["AppSettings:Verbose"] = "y"
            })
            .Build();
    }
}
```

## Integration Tests

### Database Tests

Require test database:
```csharp
[Test]
[Category("Integration")]
public void GetApplId_ValidGrantNumber_ReturnsApplicationId()
{
    // Requires test database connection
    using (var connection = new SqlConnection(TestConfig.ConnectionString))
    {
        connection.Open();

        string applId = Processor.GetApplId("5R01CA258784", connection);

        Assert.That(applId, Is.Not.Empty);
    }
}
```

### File System Tests

Use temporary directories:
```csharp
[Test]
[Category("Integration")]
public void ProcessFile_SavesAttachments()
{
    // Arrange
    string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(tempDir);

    try
    {
        // Act - process files

        // Assert - verify files saved
    }
    finally
    {
        Directory.Delete(tempDir, true);
    }
}
```

## Test Configuration

### appsettings.test.json

```json
{
  "AppSettings": {
    "Debug": "y",
    "Verbose": "y",
    "LogDir": "C:\\Temp\\TestLogs\\"
  },
  "ConnectionStrings": {
    "EIM": "Server=(localdb)\\mssqllocaldb;Database=EIM_Test;User Id=%DB_USER%;Password=%DB_PASSWORD%;TrustServerCertificate=True"
  }
}
```

### Environment Variables

Tests use environment variables for database credentials:

```powershell
# Set test database credentials
[System.Environment]::SetEnvironmentVariable('DB_USER', 'test_user', [System.EnvironmentVariableTarget]::User)
[System.Environment]::SetEnvironmentVariable('DB_PASSWORD', 'test_password', [System.EnvironmentVariableTarget]::User)
```

**Note:** Integration tests require valid database credentials. Unit tests and smoke tests handle missing credentials gracefully.

## Continuous Integration

### GitHub Actions Workflow

```yaml
name: Test Email Handling

on: [push, pull_request]

jobs:
  test:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: 8.0.x
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal --filter Category!=Integration
```

## Test Best Practices

### ? DO:
- Mock Outlook COM objects
- Use test databases (not production)
- Clean up temporary files
- Use meaningful test names
- Test edge cases
- Test error conditions
- Use setup/teardown methods

### ? DON'T:
- Test against production databases
- Leave test files on disk
- Test too many things in one test
- Use hard-coded paths
- Ignore failing tests
- Skip cleanup code

## Common Test Scenarios

### Email Parsing Tests

```csharp
[TestCase("category=PublicAccess, sub=Compliant, applid=12345", "PublicAccess")]
[TestCase("category=JIT Info, sub=Reminder", "JIT Info")]
public void ParseSubjectLine_ExtractsCategory(string subject, string expectedCategory)
{
    var result = SubjectParser.Parse(subject);
    Assert.That(result.Category, Is.EqualTo(expectedCategory));
}
```

### Application ID Extraction

```csharp
[TestCase("5R01CA258784-04", "12345678")]
[TestCase("5U24CA213274-08", "87654321")]
public void ExtractApplicationId_ValidGrantNumber(string grantNumber, string expectedApplId)
{
    // Test grant number to applid conversion
}
```

### File Processing

```csharp
[Test]
public void ProcessAttachments_SavesAllFiles()
{
    var email = CreateEmailWithAttachments(3);
    processor.ProcessAttachments(email);

    Assert.That(Directory.GetFiles(outputDir).Length, Is.EqualTo(3));
}
```

## Debugging Tests

### Visual Studio

1. Set breakpoint in test
2. Right-click test ? Debug Test
3. Step through code

### Console Output

```csharp
[Test]
public void DebugTest()
{
    Console.WriteLine("Test output");
    Debug.WriteLine("Debug output");
    TestContext.WriteLine("Test context output");
}
```

## Performance Testing

```csharp
[Test]
[Category("Performance")]
public void ProcessEmails_HandlesLargeVolume()
{
    var stopwatch = Stopwatch.StartNew();

    processor.Process(1000); // Process 1000 emails

    stopwatch.Stop();
    Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(30000)); // < 30 seconds
}
```

## Dependencies

- **NUnit**: Test framework
- **Moq**: Mocking framework
- **FluentAssertions**: Assertion library
- **Microsoft.Data.SqlClient**: Database testing
- **System.IO**: File system testing

## Notes

- Integration tests require test database setup
- COM automation tests may require Outlook installed
- Some tests marked `[Explicit]` must be run manually
- Test database should mirror production schema
- Use LocalDB for developer machines
- CI/CD runs unit tests only (no integration tests)
- Mock objects cover most COM automation scenarios
- Real Outlook testing should be done in dedicated test environment
