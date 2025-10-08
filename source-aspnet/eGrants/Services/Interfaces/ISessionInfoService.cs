using eGrants.Models;

namespace eGrants.Services.Interfaces
{
    public interface ISessionInfoService
    {
        /// <summary>
        /// Extracts and returns session-related information from the current user session.
        /// </summary>
        /// <param name="sessionInfo">The current session object containing user and environment data.</param>
        /// <returns>A <see cref="SessionInfo"/> object populated with relevant session details.</returns>
        SessionInfo GetSessionInfo(ISession sessionInfo);

    }
}
