using FinanceTrackerServer.Data;
using FinanceTrackerServer.Models.DTO.Pagination;
using FinanceTrackerServer.Models.DTO.Pagination.Requests;
using FinanceTrackerServer.Models.DTO.Stats;
using FinanceTrackerServer.Models.DTO.Transactions;
using FinanceTrackerServer.Models.Entities;
using FinanceTrackerServer.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using static FinanceTrackerServer.Models.DTO.Stats.StatsPeriodRequest;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FinanceTrackerServer.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext _context;

        public TransactionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TransactionDto> Create(CreateTransactionDto dto, int userId)
        {
            var transaction = new Transaction
            {
                Amount = dto.Amount,
                Description = dto.Description,
                Date = dto.Date,
                Type = dto.Type,
                UserId = userId,
                CategoryId = dto.CategoryId
            };

            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            var transactionDto = ConvertToDto(transaction);
            return transactionDto;
        }

        public async Task<TransactionDto> Update(UpdateTransactionDto dto)
        {
            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == dto.Id);
            if (transaction == null)
                throw new ArgumentException($"Transaction with id {dto.Id} not found");

            transaction.Amount = dto.Amount;
            transaction.Description = dto.Description;

            await _context.SaveChangesAsync();

            var transactionDto = ConvertToDto(transaction);
            return transactionDto;
        }

        public async Task Delete(int id)
        {
            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id);
            if (transaction == null)
                throw new ArgumentException($"Transaction with id {id} not found");

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<TransactionDto> Get(int id)
        {
            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id);
            if (transaction == null)
                throw new ArgumentException($"Transaction with id {id} not found");

            var transactionDto = ConvertToDto(transaction);
            return transactionDto;
        }

        public async Task<PaginatedResponse<TransactionDto>> GetTransactionsByUser(int userId, TransactionFilterRequest filter)
        {
            var query = _context.Transactions.Where(t => t.UserId == userId);
            query = ApplyFilters(query, filter);
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.Date)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var itemsDto = new List<TransactionDto>();
            foreach (var item in items) 
            {
                itemsDto.Add(ConvertToDto(item));
            }

            return new PaginatedResponse<TransactionDto>
            {
                Items = itemsDto,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PaginatedResponse<TransactionDto>> GetTransactionsByGroup(int groupId, TransactionFilterRequest filter)
        {
            var query = _context.Transactions.Where(t => t.User.GroupId == groupId);
            query = ApplyFilters(query, filter);
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.Date)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var itemsDto = new List<TransactionDto>();
            foreach (var item in items)
            {
                itemsDto.Add(ConvertToDto(item));
            }

            return new PaginatedResponse<TransactionDto>
            {
                Items = itemsDto,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<TransactionStats> GetUserStats(int userId, StatsPeriodRequest? period = null)
        {
            var (startDate, endDate) = CalculatePeriod(period);

            var query = _context.Transactions.Where(t => t.UserId == userId);
            query = ApplyPeriodFilter(query, startDate, endDate);

            var totalIncome = await query.Where(t => t.Type == TransactionType.Income).SumAsync(t => t.Amount);
            var totalExpenses = await query.Where(t=>t.Type == TransactionType.Expense).SumAsync(t => t.Amount);

            var stats = new TransactionStats
            {
                TotalIncome = await query.Where(t => t.Type == TransactionType.Income).SumAsync(t => t.Amount),
                TotalExpense = await query.Where(t => t.Type == TransactionType.Expense).SumAsync(t => t.Amount),
                TransactionCount = await query.CountAsync(),
                LastTransactionDate = await query.MaxAsync(t => (DateTime?)t.Date),
                PeriodStart = startDate,
                PeriodEnd = endDate,
                CategoryStats = await GetCategoryStats(query, totalIncome, totalExpenses)
            };

            return stats;
        }

        public async Task<GroupStatsResponse> GetGroupStats(int groupId, StatsPeriodRequest? period = null)
        {
            var (startDate, endDate) = CalculatePeriod(period);

            var usersInGroup = await _context.Users.Where(u => u.GroupId == groupId).ToListAsync();
            var response = new GroupStatsResponse();

            var groupQuery = _context.Transactions.Where(t => t.User.GroupId == groupId);
            groupQuery = ApplyPeriodFilter(groupQuery, startDate, endDate);

            var groupTotalIncome = await groupQuery.Where(t => t.Type == TransactionType.Income).SumAsync(t => t.Amount);
            var groupTotalExpenses = await groupQuery.Where(t => t.Type == TransactionType.Expense).SumAsync(t => t.Amount);

            response.GroupTotal = new TransactionStats
            {
                TotalIncome = await groupQuery.Where(t => t.Type == TransactionType.Income).SumAsync(t => t.Amount),
                TotalExpense = await groupQuery.Where(t => t.Type == TransactionType.Expense).SumAsync(t => t.Amount),
                TransactionCount = await groupQuery.CountAsync(),
                LastTransactionDate = await groupQuery.MaxAsync(t => (DateTime?)t.Date),
                PeriodStart = startDate,
                PeriodEnd = endDate,
                CategoryStats = await GetCategoryStats(groupQuery, groupTotalIncome, groupTotalExpenses)
            };

            foreach (var user in usersInGroup)
            {
               var userStats = new UserStats
                {
                    UserId = user.Id,
                    Stats = await GetUserStats(user.Id, period)
                };

                response.UsersStats.Add(userStats);
            }

            return response;
        }

        private TransactionDto ConvertToDto(Transaction transaction)
        {
            var transactionDto = new TransactionDto
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                Description = transaction.Description,
                Date = transaction.Date,
                Type = transaction.Type,
                UserId = transaction.UserId,
                CategoryId = transaction.CategoryId
            };

            return transactionDto;
        }

        private (DateTime? startDate, DateTime? endDate) CalculatePeriod(StatsPeriodRequest? period)
        {
            if (period == null)
                return (null, null);

            if (period.StartDate.HasValue || period.EndDate.HasValue)
                return (period.StartDate, period.EndDate);

            var now = DateTime.UtcNow;
            return period.Period switch
            {
                StatsPeriod.Last7Days => (now.AddDays(-7), now),
                StatsPeriod.Last30Days => (now.AddDays(-30), now),
                StatsPeriod.CurrentMonth => (new DateTime(now.Year, now.Month, 1), now),
                StatsPeriod.LastMonth => (
                    new DateTime(now.Year, now.Month, 1).AddMonths(-1),
                    new DateTime(now.Year, now.Month, 1).AddDays(-1)
                ),
                StatsPeriod.CurrentYear => (new DateTime(now.Year, 1, 1), now),
                _ => (null, null)
            };
        }

        private IQueryable<Transaction> ApplyFilters(IQueryable<Transaction> query, TransactionFilterRequest filter)
        {
            if (filter.Type.HasValue)
                query = query.Where(t => t.Type == filter.Type.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(t => t.Date >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(t => t.Date <= filter.EndDate.Value);

            if (filter.MinAmount.HasValue)
                query = query.Where(t => t.Amount >= filter.MinAmount.Value);

            if (filter.MaxAmount.HasValue)
                query = query.Where(t => t.Amount <= filter.MaxAmount.Value);

            if (!string.IsNullOrWhiteSpace(filter.Description))
                query = query.Where(t => t.Description.Contains(filter.Description));

            return query;
        }

        private IQueryable<Transaction> ApplyPeriodFilter(IQueryable<Transaction> query, DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue)
                query = query.Where(t => t.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.Date <= endDate.Value);

            return query;
        }

        private async Task<List<CategoryStats>> GetCategoryStats(IQueryable<Transaction> query, decimal totalIncome, decimal totalExpenses)
        {
            var categoryStats = await query
                .Where(t => t.CategoryId != null)
                .GroupBy(t => new { t.Category!.Id, t.Category.Name, t.Category.Type })
                .Select(g => new CategoryStats
                {
                    CategoryId = g.Key.Id,
                    CategoryName = g.Key.Name,
                    Type = g.Key.Type,
                    TotalAmount = g.Sum(t=>t.Amount),
                    TransactionCount = g.Count()
                })
                .ToListAsync();

            foreach (var stat in categoryStats)
            {
                var totalForType = stat.Type == CategoryType.Income ? totalIncome : totalExpenses;
                stat.Percentage = totalForType > 0 ? (stat.TotalAmount / totalForType) * 100 : 0;
            }

            return categoryStats;
        }
    }

    public class TransactionStats
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense{ get; set; }
  
        public int TransactionCount { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }


        public List<CategoryStats> CategoryStats { get; set; } = new();
    }
}