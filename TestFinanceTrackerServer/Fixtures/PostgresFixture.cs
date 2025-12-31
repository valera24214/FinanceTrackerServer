using FinanceTrackerServer.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace TestFinanceTrackerServer.Fixtures
{
    public class PostgresFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _container;
        public string ConnectionString => _container.GetConnectionString();

        public PostgresFixture()
        {
            _container = new PostgreSqlBuilder()
                .WithDatabase("test_db")
                .WithUsername("test")
                .WithPassword("test")
                .WithImage("postgres:15-alpine")
                .Build();
        }

        public async Task InitializeAsync()
        {
             await _container.StartAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

            using var context = new AppDbContext(options);
            await context.Database.MigrateAsync();
            DbInitializer.Initialize(context);
        }

        public async Task DisposeAsync()
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }
}
