using eGrants.Services;
using eGrants.Tests.Utilities;

using Microsoft.AspNetCore.Http;

namespace eGrants.Tests.Unit.Service
{
    public class SessionInfoServiceTests
    {
        [Fact]
        public void GetSessionInfo_WhenCrossIcKeysPresent_MapsAllFields()
        {
            var session = new TestSession();
            session.SetString("ic", "CSR");
            session.SetString("home_ic", "CSR");
            session.SetString("authority_ic", "nci");
            session.SetString("effective_role", "SpecialistCrossIc");
            session.SetString("access_scope", "CrossIcSpecialistBaseline");
            session.SetString("userid", "specialistx");

            var service = new SessionInfoService();

            var info = service.GetSessionInfo(session);

            Assert.Equal("CSR", info.Ic);
            Assert.Equal("CSR", info.HomeIc);
            Assert.Equal("nci", info.AuthorityIc);
            Assert.Equal("SpecialistCrossIc", info.EffectiveRole);
            Assert.Equal("CrossIcSpecialistBaseline", info.AccessScope);
            Assert.Equal("specialistx", info.UserId);
        }

        [Fact]
        public void GetSessionInfo_WhenCrossIcKeysMissing_FallsBackToIc()
        {
            var session = new TestSession();
            session.SetString("ic", "NCI");
            session.SetString("userid", "user1");

            var service = new SessionInfoService();

            var info = service.GetSessionInfo(session);

            Assert.Equal("NCI", info.Ic);
            Assert.Equal("NCI", info.HomeIc);
            Assert.Equal("NCI", info.AuthorityIc);
            Assert.Equal(string.Empty, info.EffectiveRole);
            Assert.Equal(string.Empty, info.AccessScope);
            Assert.Equal("user1", info.UserId);
        }
    }
}
