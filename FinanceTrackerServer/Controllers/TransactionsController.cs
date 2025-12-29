using FinanceTrackerServer.Data;
using FinanceTrackerServer.Models.DTO.Pagination.Requests;
using FinanceTrackerServer.Models.DTO.Stats;
using FinanceTrackerServer.Models.DTO.Transactions;
using FinanceTrackerServer.Services.Interfaces;
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
        private readonly IBalanceService _balanceService;
        private readonly IUserService _userService;

        public TransactionsController(ITransactionService transactionService, IBalanceService balanceService ,IUserService userService)
        {
            _transactionService = transactionService;
            _balanceService = balanceService;
            _userService = userService;
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
                var user = await _userService.GetUserAsync(userId);

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

                var transaction = await _transactionService.Create(dto, userId);
                var balance = await _balanceService.CalculateBalanceForPeriod(userId, transaction.Date.Date);

                return Ok(balance);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] TransactionFilterRequest filter)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var transactions = await _transactionService.GetTransactionsByUser(userId, filter);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                
                var balance = await _balanceService.CalculateBalanceForPeriod(userId, DateTime.Now.Date);

                return Ok(balance);
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

                var transaction = await _transactionService.Update(dto);
                var balance = await _balanceService.CalculateBalanceForPeriod(userId, transaction.Date);

                return Ok(balance);
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
                var transaction = await _transactionService.Get(id);
                var date = transaction.Date;
                await _transactionService.Delete(id);
                var balance = await _balanceService.CalculateBalanceForPeriod(userId, date);
                return Ok(balance);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("stats/user")]
        public async Task<IActionResult> GetUserStats([FromQuery][Bind(Prefix ="")]StatsPeriodRequest? period) 
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var stats = await _transactionService.GetUserStats(userId, period);

            return Ok(stats);
        }

        [HttpGet("stats/group")]
        public async Task<IActionResult> GetGroupStats([FromQuery][Bind(Prefix = "")] StatsPeriodRequest? period)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _userService.GetUserAsync(userId);

            if(user.GroupId == null)
            {
                return BadRequest("This user doesn't have group");
            }

            var stats = await _transactionService.GetGroupStats((int)user.GroupId, period);
           
            return Ok(stats);
        }

        [HttpGet("balance/group")]
        public async Task<IActionResult> GetGroupBalance()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _userService.GetUserAsync(userId);

            if (user.GroupId == null)
            {
                return BadRequest("This user doesn't have group");
            }

            var users = await _userService.GetUsersByGroupAsync((int)user.GroupId);
            decimal balance = 0;
            foreach (var u in users) 
            {
                balance += await _balanceService.CalculateBalanceForPeriod(u.Id, DateTime.Now.Date);
            }

            return Ok(balance);
        }
    }
}
