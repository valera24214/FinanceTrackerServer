using FinanceTrackerServer.Models.DTO;
using FinanceTrackerServer.Models.Entities;

namespace FinanceTrackerServer.Services.Interfaces
{
    public interface IGroupService
    {
        Task<GroupDto> Create();
        Task Delete(int groupId); 
        string GenerateInviteCode(int groupId);
        (bool isValid, int groupId) ValidateInviteCode(string code);
    }
}
