using Dapper;
using FinanceTrackerServer.Models.Entities;
using FinanceTrackerServer.Services.Interfaces;
using System.Data;

namespace FinanceTrackerServer.Services
{
    public class BalanceService:IBalanceService
    {
        private readonly IDbConnection _dbConnection;

        public BalanceService(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        private async Task<DateTime?> GetUserStartDate(int userId)
        {
            const string getLastSnapshotDate = @"
                SELECT COALESCE(
                    MAX(b.""Date"") + interval '1 day',
                    (SELECT MIN(""Date"")::date From ""Transactions"" WHERE ""UserId"" = @UserId)
                )
                FROM ""Balances"" b
                WHERE b.""UserId"" = @UserId";

            var startDate = await _dbConnection.QuerySingleOrDefaultAsync<DateTime?>(getLastSnapshotDate, new { UserId = userId });

            return startDate;
        }

        public async Task<decimal> CalculateBalanceForPeriod(int userId, DateTime targetDate)
        {
            const string calculateBalance = @"
                    WITH CalculatedBalance AS (
                    SELECT SUM
                        (   
                            CASE
                                WHEN ""Type"" = 1 THEN ""Amount""
                                WHEN ""Type"" = 0 THEN -""Amount""
                            END
                        ) as DeltaBalance
                    FROM ""Transactions""
                    WHERE ""UserId"" = @UserId AND ""Date""::date = @TargetDate::date
                    ), 

                    PreviousBalance AS (
                    SELECT 
                        ""UserBalance"" AS PrevBalance
                    FROM ""Balances""
                    WHERE ""UserId"" = @UserId 
                      AND ""Date"" = @TargetDate - interval '1 day'
                    )

                    SELECT    
                    COALESCE(pb.PrevBalance, 0) + COALESCE(cb.DeltaBalance, 0) AS FinalBalance
                    FROM CalculatedBalance cb
                    LEFT JOIN PreviousBalance pb ON 1=1
                ";

            var balance = await _dbConnection.QuerySingleAsync<decimal>(calculateBalance, new { UserId = userId, TargetDate = targetDate });

            return balance;
        }

        private async Task SaveBalanceSnapshot(int userId, DateTime targetDate, decimal balance)
        {
            const string upsertBalanceSql = @"
            INSERT INTO ""Balances"" (""UserId"", ""Date"", ""UserBalance"")
            VALUES (@UserId, @TargetDate, @Balance)
            ON CONFLICT (""UserId"", ""Date"") DO UPDATE
            SET ""UserBalance"" = EXCLUDED.""UserBalance""";

            await _dbConnection.ExecuteAsync(
                upsertBalanceSql,
                new
                {
                    UserId = userId,
                    TargetDate = targetDate,
                    Balance = balance
                }
            );
        }

        private async Task ProcessUserBalance(int userId, CancellationToken stoppingToken, DateTime? startDate)
        {
            var targetDate = DateTime.Today.AddDays(-1);
            var currentDay = startDate.Value.Date;

            while (currentDay <= targetDate) 
            { 
                if (stoppingToken.IsCancellationRequested) 
                    break;

                decimal balance = await CalculateBalanceForPeriod(userId, currentDay);

                await SaveBalanceSnapshot(userId, currentDay, balance);

                currentDay = currentDay.AddDays(1);
            }
        }

        public async Task CatchUpAllUsersBalances(CancellationToken stoppingToken = default, DateTime? startDate = null, int? userId = null)
        {
            IEnumerable<int> ids = new List<int>();
            if (userId == null)
            {
                const string getUsersSql = @"SELECT DISTINCT ""UserId"" FROM ""Transactions""";
                ids = await _dbConnection.QueryAsync<int>(getUsersSql);
            }
            else
                ids = ids.Append((int)userId);

            DateTime? personalStartDate;
            foreach (var id in ids) 
            {
                if(stoppingToken.IsCancellationRequested) 
                    break;

                personalStartDate = startDate;
                if(personalStartDate == null)
                    personalStartDate = await GetUserStartDate(id);

                await ProcessUserBalance(id, stoppingToken, personalStartDate);
            }

        }

    }
}
