using System.Collections.Generic;
using System.Security.Claims;

using eGrants.Common;

using Xunit;

namespace eGrants.Tests.Unit.Authentication
{
    /// <summary>
    /// Comprehensive tests for <see cref="EntraIdUserResolver"/>, which resolves the eGrants
    /// user identity and Institute/Center (IC) code from Microsoft Entra ID (Azure AD) claims.
    ///
    /// These tests validate the exact behavior the request pipeline in Program.cs relies on:
    ///   - preferred_username ? UPN ? email claim preference order
    ///   - username extraction from a UPN (strip the @domain)
    ///   - the "no identity" path (which triggers the redirect to egrants_default.htm)
    ///   - IC resolution from the department claim with an NCI fallback
    /// </summary>
    public class EntraIdUserResolverTests
    {
        // Well-known Entra ID claim type URIs (as emitted before/after default claim mapping).
        private const string PreferredUsername = "preferred_username";
        private const string DepartmentClaim = "department";

        private static ClaimsPrincipal BuildAuthenticatedPrincipal(params Claim[] claims)
        {
            var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        private static ClaimsPrincipal BuildUnauthenticatedPrincipal(params Claim[] claims)
        {
            // No authenticationType => IsAuthenticated is false.
            var identity = new ClaimsIdentity(claims);
            return new ClaimsPrincipal(identity);
        }

        #region ResolveUserId - happy path / claim preference

        [Fact]
        public void ResolveUserId_UsesPreferredUsername_AndStripsDomain()
        {
            var user = BuildAuthenticatedPrincipal(
                new Claim(PreferredUsername, "dehuffdc@nih.gov"));

            var result = EntraIdUserResolver.ResolveUserId(user);

            Assert.Equal("dehuffdc", result);
        }

        [Fact]
        public void ResolveUserId_FallsBackToUpn_WhenPreferredUsernameMissing()
        {
            var user = BuildAuthenticatedPrincipal(
                new Claim(ClaimTypes.Upn, "janedoe@nih.gov"));

            var result = EntraIdUserResolver.ResolveUserId(user);

            Assert.Equal("janedoe", result);
        }

        [Fact]
        public void ResolveUserId_FallsBackToEmail_WhenPreferredUsernameAndUpnMissing()
        {
            var user = BuildAuthenticatedPrincipal(
                new Claim(ClaimTypes.Email, "someone@example.org"));

            var result = EntraIdUserResolver.ResolveUserId(user);

            Assert.Equal("someone", result);
        }

        [Fact]
        public void ResolveUserId_PrefersPreferredUsername_OverUpnAndEmail()
        {
            var user = BuildAuthenticatedPrincipal(
                new Claim(PreferredUsername, "preferred@nih.gov"),
                new Claim(ClaimTypes.Upn, "upn@nih.gov"),
                new Claim(ClaimTypes.Email, "email@nih.gov"));

            var result = EntraIdUserResolver.ResolveUserId(user);

            Assert.Equal("preferred", result);
        }

        [Fact]
        public void ResolveUserId_PrefersUpn_OverEmail_WhenPreferredUsernameMissing()
        {
            var user = BuildAuthenticatedPrincipal(
                new Claim(ClaimTypes.Upn, "upn@nih.gov"),
                new Claim(ClaimTypes.Email, "email@nih.gov"));

            var result = EntraIdUserResolver.ResolveUserId(user);

            Assert.Equal("upn", result);
        }

        [Fact]
        public void ResolveUserId_ReturnsValueUnchanged_WhenNoAtSymbolPresent()
        {
            var user = BuildAuthenticatedPrincipal(
                new Claim(PreferredUsername, "dehuffdc"));

            var result = EntraIdUserResolver.ResolveUserId(user);

            Assert.Equal("dehuffdc", result);
        }

        [Theory]
        [InlineData("dehuffdc@nih.gov", "dehuffdc")]
        [InlineData("first.last@nci.nih.gov", "first.last")]
        [InlineData("user123", "user123")]
        [InlineData("UPPER@NIH.GOV", "UPPER")]
        public void ResolveUserId_ExtractsUsernameCorrectly(string upn, string expected)
        {
            var user = BuildAuthenticatedPrincipal(new Claim(PreferredUsername, upn));

            var result = EntraIdUserResolver.ResolveUserId(user);

            Assert.Equal(expected, result);
        }

        #endregion

        #region ResolveUserId - "no identity" path (drives redirect to egrants_default.htm)

        [Fact]
        public void ResolveUserId_ReturnsNull_WhenNoRelevantClaimsPresent()
        {
            // Authenticated, but no username/upn/email claims -> the "else" branch in Program.cs.
            var user = BuildAuthenticatedPrincipal(
                new Claim("name", "Some Person"),
                new Claim("oid", "00000000-0000-0000-0000-000000000000"));

            var result = EntraIdUserResolver.ResolveUserId(user);

            Assert.Null(result);
        }

        [Fact]
        public void ResolveUserId_ReturnsNull_WhenPrincipalHasNoClaimsAtAll()
        {
            var user = BuildAuthenticatedPrincipal();

            var result = EntraIdUserResolver.ResolveUserId(user);

            Assert.Null(result);
        }

        [Fact]
        public void ResolveUserId_ReturnsNull_WhenPrincipalIsNull()
        {
            var result = EntraIdUserResolver.ResolveUserId(null);

            Assert.Null(result);
        }

        [Fact]
        public void ResolveUserId_ReturnsNull_WhenPreferredUsernameIsEmpty()
        {
            var user = BuildAuthenticatedPrincipal(new Claim(PreferredUsername, string.Empty));

            var result = EntraIdUserResolver.ResolveUserId(user);

            Assert.Null(result);
        }

        [Fact]
        public void ResolveUserId_FallsBackToUpn_WhenPreferredUsernameIsEmpty()
        {
            // An empty (but non-null) preferred_username must not shadow a valid UPN.
            var user = BuildAuthenticatedPrincipal(
                new Claim(PreferredUsername, string.Empty),
                new Claim(ClaimTypes.Upn, "janedoe@nih.gov"));

            var result = EntraIdUserResolver.ResolveUserId(user);

            Assert.Equal("janedoe", result);
        }

        [Fact]
        public void ResolveUserId_FallsBackToEmail_WhenPreferredUsernameAndUpnAreEmpty()
        {
            var user = BuildAuthenticatedPrincipal(
                new Claim(PreferredUsername, string.Empty),
                new Claim(ClaimTypes.Upn, string.Empty),
                new Claim(ClaimTypes.Email, "someone@example.org"));

            var result = EntraIdUserResolver.ResolveUserId(user);

            Assert.Equal("someone", result);
        }

        #endregion

        #region ResolveIc - department claim with NCI fallback

        [Fact]
        public void ResolveIc_ReturnsDepartmentClaim_WhenPresent()
        {
            var user = BuildAuthenticatedPrincipal(
                new Claim(PreferredUsername, "dehuffdc@nih.gov"),
                new Claim(DepartmentClaim, "NHLBI"));

            var result = EntraIdUserResolver.ResolveIc(user);

            Assert.Equal("NHLBI", result);
        }

        [Fact]
        public void ResolveIc_DefaultsToNci_WhenDepartmentClaimMissing()
        {
            var user = BuildAuthenticatedPrincipal(
                new Claim(PreferredUsername, "dehuffdc@nih.gov"));

            var result = EntraIdUserResolver.ResolveIc(user);

            Assert.Equal(EntraIdUserResolver.DefaultIc, result);
            Assert.Equal("NCI", result);
        }

        [Fact]
        public void ResolveIc_DefaultsToNci_WhenDepartmentClaimEmpty()
        {
            var user = BuildAuthenticatedPrincipal(
                new Claim(DepartmentClaim, string.Empty));

            var result = EntraIdUserResolver.ResolveIc(user);

            Assert.Equal("NCI", result);
        }

        [Fact]
        public void ResolveIc_DefaultsToNci_WhenPrincipalIsNull()
        {
            var result = EntraIdUserResolver.ResolveIc(null);

            Assert.Equal("NCI", result);
        }

        #endregion

        #region HasResolvableIdentity - the gate the pipeline checks before issuing the challenge

        [Fact]
        public void HasResolvableIdentity_True_WhenAuthenticatedWithUsername()
        {
            var user = BuildAuthenticatedPrincipal(
                new Claim(PreferredUsername, "dehuffdc@nih.gov"));

            Assert.True(EntraIdUserResolver.HasResolvableIdentity(user));
        }

        [Fact]
        public void HasResolvableIdentity_False_WhenNotAuthenticated()
        {
            // Has a username claim, but the identity is not authenticated.
            var user = BuildUnauthenticatedPrincipal(
                new Claim(PreferredUsername, "dehuffdc@nih.gov"));

            Assert.False(EntraIdUserResolver.HasResolvableIdentity(user));
        }

        [Fact]
        public void HasResolvableIdentity_False_WhenAuthenticatedButNoUsernameClaim()
        {
            var user = BuildAuthenticatedPrincipal(
                new Claim("name", "Some Person"));

            Assert.False(EntraIdUserResolver.HasResolvableIdentity(user));
        }

        [Fact]
        public void HasResolvableIdentity_False_WhenPrincipalIsNull()
        {
            Assert.False(EntraIdUserResolver.HasResolvableIdentity(null));
        }

        #endregion
    }
}
