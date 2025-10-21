using FinanceTrackerServer.Data;
using FinanceTrackerServer.Interfaces;
using FinanceTrackerServer.Models.DTO.Pagination.Requests;
using FinanceTrackerServer.Models.DTO.Stats;
using FinanceTrackerServer.Models.DTO.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTrackerServer.Controllers
{
    [Route("api/transactions")]
    [ApiController]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly AppDbContext _context;

        public TransactionsController(ITransactionService transactionService, AppDbContext context)
        {
            _transactionService = transactionService;
            _context = context;
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserTransactions([FromQuery] TransactionFilterRequest filter)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var transactions = await _transactionService.GetTransactionsByUser(userId, filter);

            return Ok(transactions);
        }

        [HttpGet("group")]
        public async Task<IActionResult> GetGroupTransactions([FromQuery] TransactionFilterRequest filter)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var user = await _context.Users.FindAsync(userId);

                if (!user.GroupId.HasValue)
                    throw new NullReferenceException("This user is not in the group");

                var transactions = await _transactionService.GetTransactionsByGroup((int)user.GroupId, filter);

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var user = await _context.Users.FindAsync(userId);

                var transaction = await _transactionService.Create(dto, userId, user?.GroupId);
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var user = await _context.Users.FindAsync(userId);

                var transaction = await _transactionService.Get(id, userId, user?.GroupId);
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateTransactionDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var transaction = await _transactionService.Update(dto, userId);
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                await _transactionService.Delete(id, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("stats/user")]
        public async Task<IActionResult> GetUserStats([FromQuery]StatsPeriodRequest? period) 
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _context.Users.FindAsync(userId);

            var stats = await _transactionService.GetUserStats(userId, period);

            return Ok(stats);
        }

        [HttpGet("stats/group")]
        public async Task<IActionResult> GetGroupStats([FromQuery] StatsPeriodRequest? period)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _context.Users.FindAsync(userId);

            if(user.GroupId == null)
            {
                return BadRequest("This user doesn't have group");
            }

            var stats = await _transactionService.GetGroupStats((int)user.GroupId, period);
           
            return Ok(stats);
        }
    }
}
