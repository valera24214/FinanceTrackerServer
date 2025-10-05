using FinanceTrackerServer.Models.DTO.Transactions;
using FinanceTrackerServer.Models.Entities;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FinanceTrackerServer.Interfaces
{
    public interface ITransactionService
    {
        Task<Transaction> Get(int id, int userId, int? userGroupId);
        Task<List<Transaction>> GetTransactionsByGroup(int groupId, int? userGroupId);
        Task<List<Transaction>> GetTransactionsByUser(int userId);

        Task<Transaction> Create(CreateTransactionDto dto, int userId, int? userGroupId);
        Task<Transaction> Update(UpdateTransactionDto createDto, int userId);
        Task Delete(int id, int userId);
    }
}
