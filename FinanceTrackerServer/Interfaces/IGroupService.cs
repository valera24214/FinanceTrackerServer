using FinanceTrackerServer.Models.DTO;
using FinanceTrackerServer.Models.Entities;

namespace FinanceTrackerServer.Interfaces
{
    public interface IGroupService
    {
        Task<GroupDto> Create(Group group);
        Task Delete(int groupId); 
        string GenerateInviteCode(int groupId);
        (bool isValid, int groupId) ValidateInviteCode(string code);
    }
}
