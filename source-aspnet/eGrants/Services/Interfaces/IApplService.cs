using eGrants.Models;

namespace eGrants.Services.Interfaces
{
    public interface IApplService
    {
        Task<List<ApplType>> LoadApplTypeAsync();
        Task<List<ActivityCode>> LoadActivityCodeAsync(string adminCode);
        Task<List<Appls>> LoadApplsBySerialNumAsync(string adminCode, int serialNum);
        Task<string> CreateNewAppl(string admin_code, int serial_num, int appl_type, string activity_code, int support_year, string suffix_code, string ic, string userid);
    }
}