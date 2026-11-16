using eGrants.Services;
using eGrants.Tests.Utilities;

using Microsoft.AspNetCore.Http;

namespace eGrants.Tests.Unit.Service
{
    public class SessionInfoServiceTests
    {
        [Fact]
        public void GetSessionInfo_WhenKeysPresent_MapsAllFields()
        {
            var session = new TestSession();
            session.SetString("ic", "CSR");
            session.SetString("home_ic", "CSR");
            session.SetString("userid", "specialistx");

            var service = new SessionInfoService();

            var info = service.GetSessionInfo(session);

            Assert.Equal("CSR", info.Ic);
            Assert.Equal("CSR", info.HomeIc);
            Assert.Equal("specialistx", info.UserId);
        }

        [Fact]
        public void GetSessionInfo_WhenHomeIcMissing_FallsBackToIc()
        {
            var session = new TestSession();
            session.SetString("ic", "NCI");
            session.SetString("userid", "user1");

            var service = new SessionInfoService();

            var info = service.GetSessionInfo(session);

            Assert.Equal("NCI", info.Ic);
            Assert.Equal("NCI", info.HomeIc);
            Assert.Equal("user1", info.UserId);
        }
    }
}
