using FinanceTrackerServer.Models.DTO;
using FinanceTrackerServer.Models.Entities;

namespace FinanceTrackerServer.Services.Interfaces
{
    public interface IGroupService
    {
        Task<GroupDto> Create();
        Task Delete(); 
        Task<string> GenerateInviteCode();
        Task<int> ValidateInviteCode(string code);
    }
}
