using FinanceTrackerServer.Data;
using FinanceTrackerServer.Interfaces;
using FinanceTrackerServer.Models.DTO.Transactions;
using FinanceTrackerServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTrackerServer.Services
{
    public class TransactionService:ITransactionService
    {
        private readonly AppDbContext _context;

        public TransactionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction> Create(CreateTransactionDto dto, int userId, int? userGroupId)
        {
            var transaction = new Transaction
            {
                Amount = dto.Amount,
                Description = dto.Description,
                Date = dto.Date,
                Type = dto.Type,
                UserId = userId,
                GroupId = userGroupId,
            };

            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task<Transaction> Update(UpdateTransactionDto dto, int userId)
        {
            var transaction = await _context.Transactions.FirstOrDefaultAsync(t=>t.Id == dto.Id);
            if (transaction == null)
                throw new ArgumentNullException("Transaction doesn't found");

            if (transaction.UserId != userId)
                throw new ArgumentOutOfRangeException("Invalid user");

            transaction.Amount = dto.Amount;
            transaction.Description = dto.Description;
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task Delete(int id, int userId)
        {
            var transaction = await _context.Transactions.FirstOrDefaultAsync(t=>t.Id == id);
            if (transaction == null)
                throw new ArgumentNullException("Transaction doesn't found");

            if (transaction.UserId != userId)
                throw new UnauthorizedAccessException("Invalid user");

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();  
        }

        public async Task<Transaction> Get(int id, int userId, int? userGroupId)
        {
            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id);
            if (transaction == null)
                throw new ArgumentException($"Transaction with id {id} not found");

            if (transaction.UserId != userId && transaction.GroupId != userGroupId)
                throw new UnauthorizedAccessException("Access to transaction denied");

            return transaction;
        }

        public async Task<List<Transaction>> GetTransactionsByGroup(int groupId, int? userGroupId)
        {
            if (groupId != userGroupId)
                throw new UnauthorizedAccessException("Invalid user");

            var transaction = await _context.Transactions.Where(t => t.GroupId == groupId).ToListAsync();

            return transaction;
        }

        public async Task<List<Transaction>> GetTransactionsByUser(int userId)
        {
            var transactions = await _context.Transactions.Where(t=>t.UserId == userId).ToListAsync();

            return transactions;
        }
    }
}
