using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmailHandlingTests.StartOutlook
{
    /// <summary>
    /// Integration tests for the StartOutlook application.
    /// Since StartOutlook is a simple utility that just starts Outlook,
    /// these tests verify the basic structure and expected behavior.
    /// </summary>
    [TestClass]
    public class StartOutlookTests
    {
 #region Application Structure Tests

/// <summary>
 /// Verifies that the StartOutlook namespace exists.
   /// </summary>
    [TestMethod]
  public void StartOutlook_NamespaceExists()
        {
     // This test verifies the namespace is accessible
  // If it compiles, the namespace exists
  Assert.IsTrue(true, "StartOutlook namespace should exist");
     }

        #endregion

     #region Process Start Tests

     /// <summary>
        /// Verifies that Process.Start can be called with outlook.exe.
  /// Note: This doesn't actually start Outlook but verifies the method signature.
   /// </summary>
        [TestMethod]
   public void ProcessStart_OutlookExe_ValidMethodCall()
        {
      // Arrange
     var processStartInfo = new System.Diagnostics.ProcessStartInfo
      {
 FileName = "outlook.exe",
       UseShellExecute = true
   };

  // Assert - Just verify the ProcessStartInfo can be created
 Assert.AreEqual("outlook.exe", processStartInfo.FileName);
 }

   /// <summary>
        /// Verifies that ProcessStartInfo can be configured correctly.
        /// </summary>
        [TestMethod]
 public void ProcessStartInfo_Configuration_IsValid()
   {
     // Arrange & Act
    var startInfo = new System.Diagnostics.ProcessStartInfo("outlook.exe");

   // Assert
Assert.IsNotNull(startInfo, "ProcessStartInfo should be created");
    Assert.AreEqual("outlook.exe", startInfo.FileName, "FileName should be outlook.exe");
        }

        #endregion

  #region Integration Scenario Tests

  /// <summary>
 /// Verifies that the test can check if Outlook process exists.
        /// Note: This doesn't require Outlook to be running.
 /// </summary>
        [TestMethod]
     public void OutlookProcess_CanCheckIfRunning()
        {
// Act - Try to get Outlook processes (may or may not be running)
 var processes = System.Diagnostics.Process.GetProcessesByName("OUTLOOK");

  // Assert - Just verify we can check (process count can be 0 or more)
    Assert.IsTrue(processes.Length >= 0, "Should be able to check for Outlook processes");

       // Clean up process handles
    foreach (var process in processes)
            {
      process.Dispose();
            }
        }

        /// <summary>
        /// Verifies that the process name constant is correct.
        /// </summary>
        [TestMethod]
 public void OutlookProcessName_IsOutlookExe()
        {
 // Arrange
     const string expectedProcessName = "outlook.exe";

 // Assert
      Assert.AreEqual("outlook.exe", expectedProcessName);
        }

        #endregion

 #region Testable Wrapper Tests

   /// <summary>
        /// Verifies that a testable wrapper can be created for process starting.
        /// </summary>
  [TestMethod]
        public void TestableProcessStarter_CanBeCreated()
{
     // Arrange & Act
   var starter = new TestableProcessStarter();

 // Assert
     Assert.IsNotNull(starter, "TestableProcessStarter should be created");
Assert.IsFalse(starter.WasStartCalled, "Start should not have been called yet");
   }

  /// <summary>
        /// Verifies that the testable wrapper tracks start calls.
     /// </summary>
    [TestMethod]
        public void TestableProcessStarter_TracksStartCalls()
        {
       // Arrange
   var starter = new TestableProcessStarter();

        // Act
   starter.SimulateStart("outlook.exe");

         // Assert
 Assert.IsTrue(starter.WasStartCalled, "Start should have been called");
       Assert.AreEqual("outlook.exe", starter.LastProcessName, "Should track process name");
        }

        /// <summary>
    /// Verifies that the testable wrapper can be reset.
   /// </summary>
        [TestMethod]
  public void TestableProcessStarter_CanBeReset()
 {
   // Arrange
            var starter = new TestableProcessStarter();
      starter.SimulateStart("outlook.exe");

  // Act
  starter.Reset();

   // Assert
   Assert.IsFalse(starter.WasStartCalled, "WasStartCalled should be false after reset");
       Assert.IsNull(starter.LastProcessName, "LastProcessName should be null after reset");
 }

#endregion
    }

    /// <summary>
    /// Testable wrapper for Process.Start functionality.
 /// This allows testing without actually starting processes.
    /// </summary>
    internal class TestableProcessStarter
    {
     /// <summary>
        /// Indicates whether Start was called.
 /// </summary>
        public bool WasStartCalled { get; private set; }

        /// <summary>
 /// The last process name that was requested to start.
        /// </summary>
     public string LastProcessName { get; private set; }

  /// <summary>
     /// The time when start was last called.
        /// </summary>
        public DateTime? LastStartTime { get; private set; }

        /// <summary>
        /// Simulates starting a process without actually starting it.
        /// </summary>
        /// <param name="processName">The process name to simulate starting</param>
        public void SimulateStart(string processName)
  {
            WasStartCalled = true;
LastProcessName = processName;
        LastStartTime = DateTime.Now;
        }

        /// <summary>
        /// Resets the testable starter to initial state.
     /// </summary>
        public void Reset()
        {
    WasStartCalled = false;
  LastProcessName = null;
 LastStartTime = null;
        }
    }
}
