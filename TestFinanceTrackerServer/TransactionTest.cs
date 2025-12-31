using FinanceTrackerServer.Controllers;
using FinanceTrackerServer.Data;
using FinanceTrackerServer.Models.DTO.Pagination;
using FinanceTrackerServer.Models.DTO.Pagination.Requests;
using FinanceTrackerServer.Models.DTO.Transactions;
using FinanceTrackerServer.Models.Entities;
using FinanceTrackerServer.Services;
using FinanceTrackerServer.Services.Interfaces;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data.Common;
using System.Security.Claims;
using System.Threading.Tasks;
using TestFinanceTrackerServer.Fixtures;

namespace TestFinanceTrackerServer
{
    [Collection("Postgres collection")]
    public class TransactionTest
    {
        private readonly PostgresFixture _postgresFixture;
        private readonly DbContextOptions<AppDbContext> options;

        public TransactionTest(PostgresFixture postgresFixture)
        {
            _postgresFixture = postgresFixture;

            options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_postgresFixture.ConnectionString)
                .Options;
        }

        private async Task<TransactionsController> CreateController(AppDbContext context)
        {
            var connectionBuilder = new NpgsqlConnectionStringBuilder(context.Database.GetDbConnection().ConnectionString)
            { 
                Password = "test"
            };
            var dbConnection = new NpgsqlConnection(connectionBuilder.ConnectionString);

            var transactionService = new TransactionService(context);
            var userService = new UserService(context);

            var balanceService = new BalanceService(dbConnection);
            await balanceService.CatchUpAllUsersBalances();

            var transactionsController = new TransactionsController(transactionService, balanceService, userService);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            transactionsController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return transactionsController;
        }

        private async Task InitializeDb(AppDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Groups\" RESTART IDENTITY CASCADE;");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Users\" RESTART IDENTITY CASCADE;");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Transactions\" RESTART IDENTITY CASCADE;");

            context.Database.EnsureCreated();

            Group group = new Group { Id = 1 };
            context.Groups.Add(group);

            User[] users = new User[]
            {
                new User {Id = 1, GroupId = 1},
                new User {Id = 2, GroupId = 1},
                new User {Id = 3}
            };
            context.Users.AddRange(users);

            Transaction[] transactions = new Transaction[]
            {
                new Transaction{Amount = 1500, Description = "Зп", Date = (new DateTime(2025, 12, 12)).ToUniversalTime(), Type = TransactionType.Income, UserId = 1, CategoryId = 10},
                new Transaction{Amount = 250, Description = "Еда", Date = (new DateTime(2025, 12, 12)).ToUniversalTime(), Type = TransactionType.Expense, UserId = 1, CategoryId = 1},
                new Transaction{Amount = 1200, Description = "Зп", Date = (new DateTime(2025, 12, 18)).ToUniversalTime(), Type = TransactionType.Income, UserId = 2, CategoryId = 10},
                new Transaction{Amount = 75, Description = "Транспорт", Date = (new DateTime(2025, 12, 18)).ToUniversalTime(), Type = TransactionType.Expense, UserId = 2, CategoryId = 2},
                new Transaction{Amount = 1750, Description = "Зп", Date = (new DateTime(2025, 12, 11)).ToUniversalTime(), Type = TransactionType.Income, UserId = 3, CategoryId = 10},
            };
            context.Transactions.AddRange(transactions);

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task TestGetUserTransactions_ReturnsTransactions()
        {
            //Arrange
            await using var context = new AppDbContext(options);

            await InitializeDb(context);
            var transactionsController = await CreateController(context);

            var filterRequest = new TransactionFilterRequest();

            //Act
            var result = await transactionsController.GetUserTransactions(filterRequest);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTransactions = Assert.IsAssignableFrom<PaginatedResponse<TransactionDto>>(okResult.Value);

            Assert.Equal(2, returnedTransactions.TotalCount);
            Assert.Contains(returnedTransactions.Items, (x => x.Amount == 1500 && x.CategoryId == 10 && x.Type == TransactionType.Income));
            Assert.Contains(returnedTransactions.Items, (x => x.Amount == 250 && x.CategoryId == 1 && x.Type == TransactionType.Expense));
        }

        [Fact]
        public async Task TestCreate_ReturnBalance()
        {
            //Arrange
            await using var context = new AppDbContext(options);

            await InitializeDb(context);
            var transactionsController = await CreateController(context);

            var createTransactionDto = new CreateTransactionDto() 
            { 
                Amount = 300,
                Description = "Плойки",
                Date = (new DateTime(2025, 12, 31)).ToUniversalTime(),
                Type = TransactionType.Expense,
                CategoryId = 3
            };

            //Act
            var result = await transactionsController.Create(createTransactionDto);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var balance = Assert.IsAssignableFrom<decimal>(okResult.Value);

            Assert.Equal(950, balance);
        }

        [Fact]
        public async Task TestGetBalance_ReturnBalance()
        {
            //Arrange
            await using var context = new AppDbContext(options);

            await InitializeDb(context);
            var transactionController = await CreateController(context);

            //Act
            var result = await transactionController.GetBalance();

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var balance = Assert.IsAssignableFrom<decimal?>(okResult.Value);
            Assert.Equal(1250, balance);
        }
    }
}
