using FinanceTrackerServer.Models.DTO.Pagination;
using FinanceTrackerServer.Models.DTO.Pagination.Requests;
using FinanceTrackerServer.Models.DTO.Stats;
using FinanceTrackerServer.Models.DTO.Transactions;
using FinanceTrackerServer.Models.Entities;
using FinanceTrackerServer.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FinanceTrackerServer.Interfaces
{
    public interface ITransactionService
    {
        Task<Transaction> Create(CreateTransactionDto dto, int userId, int? userGroupId);
        Task<Transaction> Update(UpdateTransactionDto dto, int userId);
        Task Delete(int id, int userId);

        Task<Transaction> Get(int id, int userId, int? userGroupId);
        Task<PaginatedResponse<Transaction>> GetTransactionsByUser(int userId, TransactionFilterRequest filter);
        Task<PaginatedResponse<Transaction>> GetTransactionsByGroup(int groupId, TransactionFilterRequest filter);

        Task<TransactionStats> GetUserStats(int userId, StatsPeriodRequest? period = null);
        Task<GroupStatsResponse> GetGroupStats(int groupId, StatsPeriodRequest? period = null);
    }
}
