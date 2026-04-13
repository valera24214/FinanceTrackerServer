using FinanceTrackerServer.Data;
using Microsoft.EntityFrameworkCore;
using TestFinanceTrackerServer.Fixtures;

namespace TestFinanceTrackerServer;

[Collection("Postgres collection")]
public class CategoryTest
{
    private readonly PostgresFixture _postgresFixture;
    private readonly DbContextOptions<AppDbContext> _options;

    public CategoryTest()
    {
        
    }
}
