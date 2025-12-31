using FinanceTrackerServer.Models.DTO.Pagination;
using FinanceTrackerServer.Models.DTO.Pagination.Requests;
using FinanceTrackerServer.Models.DTO.Stats;
using FinanceTrackerServer.Models.DTO.Transactions;
using FinanceTrackerServer.Models.Entities;
using FinanceTrackerServer.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FinanceTrackerServer.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionDto> Create(CreateTransactionDto dto, int userId);
        Task<TransactionDto> Update(UpdateTransactionDto dto);
        Task Delete(int id);

        Task<TransactionDto> Get(int id);
        Task<PaginatedResponse<TransactionDto>> GetTransactionsByUser(int userId, TransactionFilterRequest filter);
        Task<PaginatedResponse<TransactionDto>> GetTransactionsByGroup(int groupId, TransactionFilterRequest filter);

        Task<TransactionStats> GetUserStats(int userId, StatsPeriodRequest? period = null);
        Task<GroupStatsResponse> GetGroupStats(int groupId, StatsPeriodRequest? period = null);
    }
}
