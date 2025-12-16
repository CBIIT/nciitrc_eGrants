using eGrants.Models;

namespace eGrants.Services.Interfaces
{
    public interface IApplService
    {
        Task<List<ApplType>> LoadApplTypeAsync();
        Task<List<ActivityCode>> LoadActivityCodeAsync(string adminCode);
        Task<List<Appls>> LoadApplsBySerialNumAsync(string adminCode, int serialNum);
    }
}