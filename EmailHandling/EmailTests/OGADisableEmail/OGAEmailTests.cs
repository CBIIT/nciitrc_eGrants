using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OGARequestAccountDisable;
using Microsoft.Office.Interop.Outlook;

namespace EmailTests.OGADisableEmail
{
    [TestClass]
    public class OGAEmailTests
    {
        private List<DisabledListItem> GetFakeCandidates()
        {
            var candidates = new List<DisabledListItem>
            {
                new DisabledListItem {FirstNameFromDB = "Søren", LastNameFromDB = "Kierkegaard", PersonNameFromDB = "Kierkegaard, Søren" },
                new DisabledListItem {FirstNameFromDB = "Miguel", LastNameFromDB = "Unamuno", PersonNameFromDB = "Unamuno, Miguel" },
                new DisabledListItem {FirstNameFromDB = "Friedrich", LastNameFromDB = "", PersonNameFromDB = "" },
                new DisabledListItem {FirstNameFromDB = "", LastNameFromDB = "", PersonNameFromDB = "NCI OGA PROGRESS REPORT" },
                new DisabledListItem {FirstNameFromDB = "", LastNameFromDB = "", PersonNameFromDB = "ncigabawardunit" },
                new DisabledListItem {FirstNameFromDB = "", LastNameFromDB = "", PersonNameFromDB = "CA ERA NOTIFICATIONS" },
            };

            return candidates;
        }

        [TestMethod]
        public void IncludeGoodUsers()
        {
            // Arrange
            var candidates = GetFakeCandidates();

            // Act
            var result = Processor.FilterOutUsersWithMissingInfo(candidates);

            // Assert
            var containsSoren = result.Count(r => r.FirstNameFromDB.Equals("Søren")) == 1;
            var containsMiguel = result.Count(r => r.FirstNameFromDB.Equals("Miguel")) == 1;
            Assert.IsTrue(containsSoren && containsMiguel);
        }

        [TestMethod]
        public void IncludeServiceAccounts ()
        {
            // Arrange
            var candidates = GetFakeCandidates();

            // Act
            var result = Processor.FilterOutUsersWithMissingInfo(candidates);

            // Assert
            var containsProgressReport = result.Count(r => r.PersonNameFromDB.Equals("NCI OGA PROGRESS REPORT")) == 1;
            var containsAwardUnit = result.Count(r => r.PersonNameFromDB.Equals("ncigabawardunit")) == 1;
            var containsEraNotifications = result.Count(r => r.PersonNameFromDB.Equals("CA ERA NOTIFICATIONS")) == 1;

            Assert.IsTrue(containsProgressReport && containsAwardUnit && containsEraNotifications);
        }

        [TestMethod]
        public void RemoveNietzsche()
        {
            // Arrange
            var candidates = GetFakeCandidates();

            // Act
            var result = Processor.FilterOutUsersWithMissingInfo(candidates);

            // Assert
            var containsNietzsche = result.Count(r => r.FirstNameFromDB.Equals("Friedrich")) == 1;

            Assert.IsFalse(containsNietzsche);
        }

        [TestMethod]
        public void FilteredOutFlagSet()
        {
            // Arrange
            var candidates = GetFakeCandidates();

            // Act
            var result = Processor.FilterOutUsersWithMissingInfo(candidates);

            // Assert
            var nietzsche = candidates.Where(r => r.FirstNameFromDB.Equals("Friedrich")).FirstOrDefault();

            Assert.IsTrue(nietzsche != null && nietzsche.FailedToRenderName);
        }

        [TestMethod]
        public void FilteredOutFlagSetNegative()
        {
            // Arrange
            var candidates = GetFakeCandidates();

            // Act
            var result = Processor.FilterOutUsersWithMissingInfo(candidates);

            // Assert
            var soren = candidates.Where(r => r.FirstNameFromDB.Equals("Søren")).FirstOrDefault();

            Assert.IsTrue(soren != null && !soren.FailedToRenderName);
        }
    }
}
