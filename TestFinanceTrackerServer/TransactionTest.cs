using FinanceTrackerServer.Controllers;
using FinanceTrackerServer.Data;
using FinanceTrackerServer.Models.DTO.Pagination;
using FinanceTrackerServer.Models.DTO.Pagination.Requests;
using FinanceTrackerServer.Models.DTO.Stats;
using FinanceTrackerServer.Models.DTO.Transactions;
using FinanceTrackerServer.Models.Entities;
using FinanceTrackerServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.Claims;
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
                new Transaction{Id = 1, Amount = 1500, Description = "Зп", Date = (new DateTime(2025, 12, 12)).ToUniversalTime(), Type = TransactionType.Income, UserId = 1, CategoryId = 10},
                new Transaction{Id = 2, Amount = 250, Description = "Еда", Date = (new DateTime(2025, 12, 12)).ToUniversalTime(), Type = TransactionType.Expense, UserId = 1, CategoryId = 1},
                new Transaction{Id = 3, Amount = 1200, Description = "Зп", Date = (new DateTime(2025, 12, 18)).ToUniversalTime(), Type = TransactionType.Income, UserId = 2, CategoryId = 10},
                new Transaction{Id = 4, Amount = 75, Description = "Транспорт", Date = (new DateTime(2025, 12, 18)).ToUniversalTime(), Type = TransactionType.Expense, UserId = 2, CategoryId = 2},
                new Transaction{Id = 5, Amount = 1750, Description = "Зп", Date = (new DateTime(2025, 12, 11)).ToUniversalTime(), Type = TransactionType.Income, UserId = 3, CategoryId = 10},
            };

            context.Transactions.AddRange(transactions);

            await context.SaveChangesAsync();

            await context.Database.ExecuteSqlRawAsync(
                "SELECT setval(pg_get_serial_sequence('\"Transactions\"','Id'), " +
                "(SELECT MAX(\"Id\") FROM \"Transactions\"))");
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
        public async Task TestUpdate_ReturnBalance()
        {
            //Arrange
            await using var context = new AppDbContext(options);

            await InitializeDb(context);
            var transactionController = await CreateController(context);

            var updateTransactionDto = new UpdateTransactionDto
            {
                Id = 1,
                Amount = 1650,
                Description = "Зп"
            };

            //Act
            var result = await transactionController.Update(updateTransactionDto);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var balance = Assert.IsAssignableFrom<decimal>(okResult.Value);

            Assert.Equal(1400, balance);
        }

        [Fact]
        public async Task TestDelete_ReturnBalance()
        {
            //Arrange
            await using var context = new AppDbContext(options);

            await InitializeDb(context);
            var transactionController = await CreateController(context);

            //Act
            var result = await transactionController.Delete(2);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var balance = Assert.IsAssignableFrom<decimal>(okResult.Value);

            Assert.Equal(1500, balance);
        }

        [Fact]
        public async Task TestGetUserStats_ReturnStats()
        {
            //Arrange
            await using var context = new AppDbContext(options);

            await InitializeDb(context);
            var transactionController = await CreateController(context);

            var statsRequest = new StatsPeriodRequest();

            //Act 
            var result = await transactionController.GetUserStats(statsRequest);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var stats = Assert.IsAssignableFrom<TransactionStats>(okResult.Value);

            Assert.Equal(1500, stats.TotalIncome);
            Assert.Equal(250, stats.TotalExpense);
            Assert.Equal(2, stats.CategoryStats.Count);
            Assert.Equal(2, stats.TransactionCount);
        }

        [Fact]
        public async Task TestGetGroupStats_ReturnStats()
        {
            //Arrange
            var context = new AppDbContext(options);

            await InitializeDb(context);
            var transactionController = await CreateController(context);

            var statsRequest = new StatsPeriodRequest();

            //Act
            var result = await transactionController.GetGroupStats(statsRequest);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var stats = Assert.IsAssignableFrom<GroupStatsResponse>(okResult.Value);

            Assert.Equal(2700, stats.GroupTotal.TotalIncome);
            Assert.Equal(325, stats.GroupTotal.TotalExpense);
            Assert.Equal(3, stats.GroupTotal.CategoryStats.Count);
            Assert.Equal(4, stats.GroupTotal.TransactionCount);
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
            var balance = Assert.IsAssignableFrom<decimal>(okResult.Value);

            Assert.Equal(1250, balance);
        }

        [Fact]
        public async Task TestGetGroupBalance_ReturnBalance()
        {
            //Arrange
            await using var context = new AppDbContext(options);

            await InitializeDb(context);
            var transactionController = await CreateController(context);

            //Act
            var result = await transactionController.GetGroupBalance();

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var balance = Assert.IsAssignableFrom<decimal>(okResult.Value);

            Assert.Equal(2375, balance);
        }
    }
}
