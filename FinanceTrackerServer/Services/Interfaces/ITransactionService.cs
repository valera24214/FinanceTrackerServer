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
        Task<(TransactionDto, decimal)> Create(CreateTransactionDto dto);
        Task<(TransactionDto, decimal)> Update(UpdateTransactionDto dto);
        Task<decimal> Delete(int id);

        Task<TransactionDto> Get(int id);
        Task<decimal> GetUserBalance();
        Task<PaginatedResponse<TransactionDto>> GetTransactionsByUser(TransactionFilterRequest filter);
        Task<PaginatedResponse<TransactionDto>> GetTransactionsByGroup(TransactionFilterRequest filter);

        Task<TransactionStats> GetUserStats(StatsPeriodRequest? period = null);
        Task<GroupStatsResponse> GetGroupStats(StatsPeriodRequest? period = null);
    }
}
