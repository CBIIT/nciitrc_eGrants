using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmailHandlingTests.AddSuppProd
{
    /// <summary>
    /// Unit tests for AddSuppProd.Processor helper methods.
    /// These tests verify text processing, parsing, and utility functions
    /// without requiring database or Outlook dependencies.
    /// </summary>
    [TestClass]
    public class AddSuppProdUnitTests
    {
        private TestAddSuppProdProcessor _processor;

        [TestInitialize]
        public void Setup()
        {
            _processor = new TestAddSuppProdProcessor();
        }

        #region ExtractNotificationID Tests

        /// <summary>
        /// Verifies that a valid notification ID is extracted correctly.
        /// </summary>
        [TestMethod]
        public void ExtractNotificationID_WithValidID_ReturnsCorrectID()
        {
            // Arrange
            string body = "This is a notification email. Notification Id=12345 sent to you.";

            // Act
            string result = _processor.TestExtractNotificationID(body);

            // Assert
            Assert.AreEqual("12345", result, "Should extract the notification ID");
        }

        /// <summary>
        /// Verifies case-insensitive matching.
        /// </summary>
        [TestMethod]
        public void ExtractNotificationID_CaseInsensitive_ReturnsID()
        {
            // Arrange
            string body = "Please review. NOTIFICATION ID=67890 is attached.";

            // Act
            string result = _processor.TestExtractNotificationID(body);

            // Assert
            Assert.AreEqual("67890", result, "Should extract ID regardless of case");
        }

        /// <summary>
        /// Verifies that no ID is returned when pattern is missing.
        /// </summary>
        [TestMethod]
        public void ExtractNotificationID_NoPattern_ReturnsEmpty()
        {
            // Arrange
            string body = "This email has no notification ID pattern.";

            // Act
            string result = _processor.TestExtractNotificationID(body);

            // Assert
            Assert.AreEqual("", result, "Should return empty string when no match");
        }

        /// <summary>
        /// Verifies that empty body returns empty string.
        /// </summary>
        [TestMethod]
        public void ExtractNotificationID_EmptyBody_ReturnsEmpty()
        {
            // Arrange
            string body = "";

            // Act
            string result = _processor.TestExtractNotificationID(body);

            // Assert
            Assert.AreEqual("", result, "Should return empty string for empty body");
        }

        /// <summary>
        /// Verifies extraction with ID at the beginning of the body.
        /// </summary>
        [TestMethod]
        public void ExtractNotificationID_IDAtStart_ReturnsID()
        {
            // Arrange
            string body = "Notification Id=99999\n\nThe rest of the email body...";

            // Act
            string result = _processor.TestExtractNotificationID(body);

            // Assert
            Assert.AreEqual("99999", result, "Should extract ID at start of body");
        }

        #endregion

        #region RemoveSpecialCharacters Tests

        /// <summary>
        /// Verifies that colons are replaced with spaces.
        /// </summary>
        [TestMethod]
        public void RemoveSpecialCharacters_RemovesColons()
        {
            // Arrange
            string input = "Test:Subject:Line";

            // Act
            string result = _processor.TestRemoveSpecialCharacters(input);

            // Assert
            Assert.IsFalse(result.Contains(":"), "Result should not contain colons");
        }

        /// <summary>
        /// Verifies that forward slashes are replaced.
        /// </summary>
        [TestMethod]
        public void RemoveSpecialCharacters_RemovesForwardSlashes()
        {
            // Arrange
            string input = "Test/Subject/Line";

            // Act
            string result = _processor.TestRemoveSpecialCharacters(input);

            // Assert
            Assert.IsFalse(result.Contains("/"), "Result should not contain forward slashes");
        }

        /// <summary>
        /// Verifies that backslashes are replaced.
        /// </summary>
        [TestMethod]
        public void RemoveSpecialCharacters_RemovesBackslashes()
        {
            // Arrange
            string input = @"Test\Subject\Line";

            // Act
            string result = _processor.TestRemoveSpecialCharacters(input);

            // Assert
            Assert.IsFalse(result.Contains(@"\"), "Result should not contain backslashes");
        }

        /// <summary>
        /// Verifies that ampersands are replaced with 'and'.
        /// </summary>
        [TestMethod]
        public void RemoveSpecialCharacters_ReplacesAmpersandWithAnd()
        {
            // Arrange
            string input = "Test&Subject";

            // Act
            string result = _processor.TestRemoveSpecialCharacters(input);

            // Assert
            Assert.IsTrue(result.Contains("and"), "Result should contain 'and'");
            Assert.IsFalse(result.Contains("&"), "Result should not contain ampersand");
        }

        /// <summary>
        /// Verifies that all special characters are removed.
        /// </summary>
        [TestMethod]
        public void RemoveSpecialCharacters_RemovesAllSpecialChars()
        {
            // Arrange
            string input = "Test:123/456\\789&abc;def<ghi>jkl^mno%pqr@stu'vwx";

            // Act
            string result = _processor.TestRemoveSpecialCharacters(input);

            // Assert
            Assert.IsFalse(result.Contains(":"), "Should not contain :");
            Assert.IsFalse(result.Contains("/"), "Should not contain /");
            Assert.IsFalse(result.Contains("\\"), "Should not contain \\");
            Assert.IsFalse(result.Contains(";"), "Should not contain ;");
            Assert.IsFalse(result.Contains("<"), "Should not contain <");
            Assert.IsFalse(result.Contains(">"), "Should not contain >");
            Assert.IsFalse(result.Contains("^"), "Should not contain ^");
            Assert.IsFalse(result.Contains("%"), "Should not contain %");
            Assert.IsFalse(result.Contains("@"), "Should not contain @");
            Assert.IsFalse(result.Contains("'"), "Should not contain '");
        }

        /// <summary>
        /// Verifies that empty string is handled correctly.
        /// </summary>
        [TestMethod]
        public void RemoveSpecialCharacters_EmptyString_ReturnsEmpty()
        {
            // Arrange
            string input = "";

            // Act
            string result = _processor.TestRemoveSpecialCharacters(input);

            // Assert
            Assert.AreEqual("", result, "Should return empty string");
        }

        /// <summary>
        /// Verifies that null is handled correctly.
        /// </summary>
        [TestMethod]
        public void RemoveSpecialCharacters_NullString_ReturnsEmpty()
        {
            // Arrange
            string input = null;

            // Act
            string result = _processor.TestRemoveSpecialCharacters(input);

            // Assert
            Assert.AreEqual("", result, "Should return empty string for null");
        }

        /// <summary>
        /// Verifies that spaces are removed.
        /// </summary>
        [TestMethod]
        public void RemoveSpecialCharacters_RemovesSpaces()
        {
            // Arrange
            string input = "Test Subject Line";

            // Act
            string result = _processor.TestRemoveSpecialCharacters(input);

            // Assert
            Assert.IsFalse(result.Contains(" "), "Result should not contain spaces");
            Assert.AreEqual("TestSubjectLine", result, "Should concatenate without spaces");
        }

        #endregion

        #region ParseSubjectParameters Tests

        /// <summary>
        /// Verifies parsing of all staff upload parameters.
        /// </summary>
        [TestMethod]
        public void ParseSubjectParameters_ValidStaffEmail_ReturnsAllParameters()
        {
            // Arrange
            string subject = "category=correspondence,sub=admin supplement,grantnumber=1R01CA123456-01";

            // Act
            var result = _processor.TestParseSubjectParameters(subject);

            // Assert
            Assert.AreEqual(3, result.Count, "Should have 3 parameters");
            Assert.AreEqual("correspondence", result["category"]);
            Assert.AreEqual("admin supplement", result["sub"]);
            Assert.AreEqual("1R01CA123456-01", result["grantnumber"]);
        }

        /// <summary>
        /// Verifies parsing of application file parameters.
        /// </summary>
        [TestMethod]
        public void ParseSubjectParameters_ApplicationFile_ParsesCorrectly()
        {
            // Arrange
            string subject = "category=application file,grantnumber=5R01CA987654-02";

            // Act
            var result = _processor.TestParseSubjectParameters(subject);

            // Assert
            Assert.AreEqual(2, result.Count, "Should have 2 parameters");
            Assert.AreEqual("application file", result["category"]);
            Assert.AreEqual("5R01CA987654-02", result["grantnumber"]);
        }

        /// <summary>
        /// Verifies case-insensitive key matching.
        /// </summary>
        [TestMethod]
        public void ParseSubjectParameters_CaseInsensitive_MatchesKeys()
        {
            // Arrange
            string subject = "CATEGORY=correspondence,SUB=test,GrantNumber=123";

            // Act
            var result = _processor.TestParseSubjectParameters(subject);

            // Assert
            Assert.IsTrue(result.ContainsKey("category"), "Should match 'category' case-insensitively");
            Assert.IsTrue(result.ContainsKey("sub"), "Should match 'sub' case-insensitively");
            Assert.IsTrue(result.ContainsKey("grantnumber"), "Should match 'grantnumber' case-insensitively");
        }

        /// <summary>
        /// Verifies that empty subject returns empty dictionary.
        /// </summary>
        [TestMethod]
        public void ParseSubjectParameters_EmptySubject_ReturnsEmpty()
        {
            // Arrange
            string subject = "";

            // Act
            var result = _processor.TestParseSubjectParameters(subject);

            // Assert
            Assert.AreEqual(0, result.Count, "Should return empty dictionary");
        }

        /// <summary>
        /// Verifies handling of parameters with extra whitespace.
        /// </summary>
        [TestMethod]
        public void ParseSubjectParameters_WithWhitespace_TrimsValues()
        {
            // Arrange
            string subject = "category = correspondence , sub = admin supplement , grantnumber = 1R01CA123456-01 ";

            // Act
            var result = _processor.TestParseSubjectParameters(subject);

            // Assert
            Assert.AreEqual("correspondence", result["category"], "Should trim whitespace");
            Assert.AreEqual("admin supplement", result["sub"], "Should trim whitespace");
            Assert.AreEqual("1R01CA123456-01", result["grantnumber"], "Should trim whitespace");
        }

        /// <summary>
        /// Verifies handling of parameters with applid.
        /// </summary>
        [TestMethod]
        public void ParseSubjectParameters_WithApplId_ParsesCorrectly()
        {
            // Arrange
            string subject = "category=correspondence,sub=test,applid=8765432";

            // Act
            var result = _processor.TestParseSubjectParameters(subject);

            // Assert
            Assert.AreEqual(3, result.Count, "Should have 3 parameters");
            Assert.AreEqual("8765432", result["applid"]);
        }

        /// <summary>
        /// Verifies that invalid parameters (no equals sign) are skipped.
        /// </summary>
        [TestMethod]
        public void ParseSubjectParameters_InvalidParameter_SkipsIt()
        {
            // Arrange
            string subject = "category=correspondence,invalidparam,sub=test";

            // Act
            var result = _processor.TestParseSubjectParameters(subject);

            // Assert
            Assert.AreEqual(2, result.Count, "Should skip invalid parameter");
            Assert.IsTrue(result.ContainsKey("category"));
            Assert.IsTrue(result.ContainsKey("sub"));
        }

        #endregion

        #region GetFileExtension Tests

        /// <summary>
        /// Verifies extraction of PDF extension.
        /// </summary>
        [TestMethod]
        public void GetFileExtension_PdfFile_ReturnsPdf()
        {
            // Arrange
            string fileName = "document.pdf";

            // Act
            string result = _processor.TestGetFileExtension(fileName);

            // Assert
            Assert.AreEqual("pdf", result, "Should extract 'pdf' extension");
        }

        /// <summary>
        /// Verifies extraction of DOC extension.
        /// </summary>
        [TestMethod]
        public void GetFileExtension_DocFile_ReturnsDoc()
        {
            // Arrange
            string fileName = "proposal.doc";

            // Act
            string result = _processor.TestGetFileExtension(fileName);

            // Assert
            Assert.AreEqual("doc", result, "Should extract 'doc' extension");
        }

        /// <summary>
        /// Verifies extraction of DOCX extension.
        /// </summary>
        [TestMethod]
        public void GetFileExtension_DocxFile_ReturnsDocx()
        {
            // Arrange
            string fileName = "application.DOCX";

            // Act
            string result = _processor.TestGetFileExtension(fileName);

            // Assert
            Assert.AreEqual("docx", result, "Should extract 'docx' extension and convert to lowercase");
        }

        /// <summary>
        /// Verifies handling of file without extension.
        /// </summary>
        [TestMethod]
        public void GetFileExtension_NoExtension_ReturnsTxt()
        {
            // Arrange
            string fileName = "filenamewithoutextension";

            // Act
            string result = _processor.TestGetFileExtension(fileName);

            // Assert
            Assert.AreEqual("txt", result, "Should return 'txt' as default");
        }

        /// <summary>
        /// Verifies handling of empty filename.
        /// </summary>
        [TestMethod]
        public void GetFileExtension_EmptyString_ReturnsTxt()
        {
            // Arrange
            string fileName = "";

            // Act
            string result = _processor.TestGetFileExtension(fileName);

            // Assert
            Assert.AreEqual("txt", result, "Should return 'txt' as default");
        }

        /// <summary>
        /// Verifies handling of null filename.
        /// </summary>
        [TestMethod]
        public void GetFileExtension_NullString_ReturnsTxt()
        {
            // Arrange
            string fileName = null;

            // Act
            string result = _processor.TestGetFileExtension(fileName);

            // Assert
            Assert.AreEqual("txt", result, "Should return 'txt' as default");
        }

        /// <summary>
        /// Verifies handling of filename with multiple dots.
        /// </summary>
        [TestMethod]
        public void GetFileExtension_MultipleDots_ReturnsLastExtension()
        {
            // Arrange
            string fileName = "file.backup.doc";

            // Act
            string result = _processor.TestGetFileExtension(fileName);

            // Assert
            Assert.AreEqual("doc", result, "Should return extension after last dot");
        }

        /// <summary>
        /// Verifies case conversion to lowercase.
        /// </summary>
        [TestMethod]
        public void GetFileExtension_UpperCase_ReturnsLowerCase()
        {
            // Arrange
            string fileName = "FILE.PDF";

            // Act
            string result = _processor.TestGetFileExtension(fileName);

            // Assert
            Assert.AreEqual("pdf", result, "Should convert to lowercase");
        }

        #endregion

        #region Edge Cases and Integration

        /// <summary>
        /// Verifies handling of real-world grant number with special characters.
        /// </summary>
        [TestMethod]
        public void RemoveSpecialCharacters_RealGrantNumber_FormatsCorrectly()
        {
            // Arrange
            string input = "1 R01 CA123456-01";

            // Act
            string result = _processor.TestRemoveSpecialCharacters(input);

            // Assert
            Assert.AreEqual("1R01CA123456-01", result, "Should remove spaces");
        }

        /// <summary>
        /// Verifies notification ID extraction from realistic email body.
        /// </summary>
        [TestMethod]
        public void ExtractNotificationID_RealisticEmail_ExtractsCorrectly()
        {
            // Arrange
            string body = @"
Dear Program Director,

You have received an administrative supplement notification.

Notification Id=54321
Grant Number: 5R01CA123456-03
PI: Dr. John Smith

Please review and respond.

Thank you,
NCI OGA
";

            // Act
            string result = _processor.TestExtractNotificationID(body);

            // Assert
            Assert.AreEqual("54321", result, "Should extract notification ID from realistic email");
        }

        /// <summary>
        /// Verifies parsing of complete staff upload subject line.
        /// </summary>
        [TestMethod]
        public void ParseSubjectParameters_CompleteStaffUpload_ParsesAll()
        {
            // Arrange
            string subject = "category=Correspondence,sub=Diversity Supplement,grantnumber=5 R44 CA987654-02,applid=1234567";

            // Act
            var result = _processor.TestParseSubjectParameters(subject);

            // Assert
            Assert.AreEqual(4, result.Count, "Should parse all 4 parameters");
            Assert.AreEqual("Correspondence", result["category"]);
            Assert.AreEqual("Diversity Supplement", result["sub"]);
            Assert.AreEqual("5 R44 CA987654-02", result["grantnumber"]);
            Assert.AreEqual("1234567", result["applid"]);
        }

        #endregion
    }
}
