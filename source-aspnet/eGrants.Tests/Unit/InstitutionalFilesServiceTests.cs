using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eGrants.DTOs;
using eGrants.Models;
using eGrants.Repositories.Interfaces;
using eGrants.Services;

using Moq;

namespace eGrants.Tests.Unit
{
    public class InstitutionalFilesServiceTests
    {
        private readonly Mock<IInstitutionalFilesRepository> _mockRepo;
        private readonly InstitutionalFilesService _service;

        public InstitutionalFilesServiceTests()
        {
            _mockRepo = new Mock<IInstitutionalFilesRepository>();
            _service = new InstitutionalFilesService(_mockRepo.Object);
        }

        [Fact]
        public async Task FindOrg_ReturnsExpectedResult()
        {
            // Arrange
            int orgId = 1;
            string orgName = "TestOrg";
            var expected = new InstFileFindOrgDTO { OrgId = orgId, OrgName = orgName };

            _mockRepo.Setup(r => r.FindOrg(orgId, orgName))
                     .ReturnsAsync(expected);

            // Act
            var result = await _service.FindOrg(orgId, orgName);

            // Assert
            Assert.Equal(expected.OrgId, result.OrgId);
            Assert.Equal(expected.OrgName, result.OrgName);
        }

        [Fact]
        public async Task LoadOrgNameCharacterIndices_ReturnsList()
        {
            // Arrange
            var expected = new List<InsitutionalOrgNameIndex>
        {
            new InsitutionalOrgNameIndex { CharacterIndex = "1", IndexSeq = 0 },
            new InsitutionalOrgNameIndex { CharacterIndex = "2", IndexSeq = 1 }
        };

            _mockRepo.Setup(r => r.LoadOrgNameCharacterIndices())
                     .ReturnsAsync(expected);

            // Act
            var result = await _service.LoadOrgNameCharacterIndices();

            // Assert
            Assert.Equal(expected.Count, result.Count);
            Assert.Equal(expected[0].CharacterIndex, result[0].CharacterIndex);
        }

        [Fact]
        public async Task LoadOrgDocList_ReturnsDocuments()
        {
            // Arrange
            int orgId = 42;
            var expected = new List<InstFileLoadOrgDocListDTO>
        {
            new InstFileLoadOrgDocListDTO { DocumentId = 1, category_name = "Doc1" },
            new InstFileLoadOrgDocListDTO { DocumentId = 2, category_name = "Doc2" }
        };

            _mockRepo.Setup(r => r.LoadOrgDocList(orgId))
                     .ReturnsAsync(expected);

            // Act
            var result = await _service.LoadOrgDocList(orgId);

            // Assert
            Assert.Equal(expected.Count, result.Count);
            Assert.Equal(expected[1].category_name, result[1].category_name);
        }

        [Fact]
        public async Task FindOrg_ReturnsNull_WhenRepositoryReturnsNull()
        {
            // Arrange
            int orgId = 999;
            string orgName = "UnknownOrg";

            _mockRepo.Setup(r => r.FindOrg(orgId, orgName))
                     .ReturnsAsync((InstFileFindOrgDTO)null);

            // Act
            var result = await _service.FindOrg(orgId, orgName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindOrg_ThrowsException_WhenRepositoryFails()
        {
            // Arrange
            int orgId = 1;
            string orgName = "TestOrg";

            _mockRepo.Setup(r => r.FindOrg(orgId, orgName))
                     .ThrowsAsync(new System.Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<System.Exception>(() => _service.FindOrg(orgId, orgName));
        }

        [Fact]
        public async Task LoadOrgNameCharacterIndices_ReturnsEmptyList_WhenRepositoryReturnsEmpty()
        {
            // Arrange
            _mockRepo.Setup(r => r.LoadOrgNameCharacterIndices())
                     .ReturnsAsync(new List<InsitutionalOrgNameIndex>());

            // Act
            var result = await _service.LoadOrgNameCharacterIndices();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task LoadOrgNameCharacterIndices_ThrowsException_WhenRepositoryFails()
        {
            // Arrange
            _mockRepo.Setup(r => r.LoadOrgNameCharacterIndices())
                     .ThrowsAsync(new System.Exception("Repository failure"));

            // Act & Assert
            await Assert.ThrowsAsync<System.Exception>(() => _service.LoadOrgNameCharacterIndices());
        }

        [Fact]
        public async Task LoadOrgDocList_ReturnsEmptyList_WhenNoDocumentsFound()
        {
            // Arrange
            int orgId = 123;

            _mockRepo.Setup(r => r.LoadOrgDocList(orgId))
                     .ReturnsAsync(new List<InstFileLoadOrgDocListDTO>());

            // Act
            var result = await _service.LoadOrgDocList(orgId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task LoadOrgDocList_ThrowsException_WhenRepositoryFails()
        {
            // Arrange
            int orgId = 42;

            _mockRepo.Setup(r => r.LoadOrgDocList(orgId))
                     .ThrowsAsync(new System.Exception("Data access error"));

            // Act & Assert
            await Assert.ThrowsAsync<System.Exception>(() => _service.LoadOrgDocList(orgId));
        }
    }
}
