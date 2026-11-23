using System;

using eGrants.Services;

using Xunit;

namespace eGrants.Tests.Unit.Service
{
    /// <summary>
    /// Unit tests for the configurable hidden-profiles filtering used by
    /// <see cref="CommonService.LoadProfiles"/>. The filtering decision is exposed
    /// via <see cref="CommonService.IsHiddenProfile"/> so it can be exercised
    /// without a live database connection.
    /// </summary>
    public class CommonServiceTests
    {
        [Fact]
        public void IsHiddenProfile_ReturnsTrue_WhenProfileIsInHiddenList()
        {
            var hidden = new[] { "NCMHD" };

            Assert.True(CommonService.IsHiddenProfile("NCMHD", hidden));
        }

        [Fact]
        public void IsHiddenProfile_IsCaseInsensitive()
        {
            var hidden = new[] { "NCMHD" };

            Assert.True(CommonService.IsHiddenProfile("ncmhd", hidden));
        }

        [Fact]
        public void IsHiddenProfile_ReturnsFalse_WhenProfileNotInHiddenList()
        {
            var hidden = new[] { "NCMHD" };

            Assert.False(CommonService.IsHiddenProfile("NCI", hidden));
        }

        [Fact]
        public void IsHiddenProfile_ReturnsFalse_WhenHiddenListEmpty()
        {
            Assert.False(CommonService.IsHiddenProfile("NCMHD", Array.Empty<string>()));
        }

        [Fact]
        public void IsHiddenProfile_ReturnsFalse_WhenHiddenListNull()
        {
            Assert.False(CommonService.IsHiddenProfile("NCMHD", null));
        }
    }
}
