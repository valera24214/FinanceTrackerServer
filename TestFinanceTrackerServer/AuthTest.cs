using FinanceTrackerServer.Controllers;
using FinanceTrackerServer.Data;
using FinanceTrackerServer.Models.DTO.AuthAccounts;
using FinanceTrackerServer.Models.Entities;
using FinanceTrackerServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestFinanceTrackerServer.Fixtures;

namespace TestFinanceTrackerServer;

[Collection("Postgres collection")]
public class AuthTest
{
    private readonly PostgresFixture _postgresFixture;
    private readonly DbContextOptions<AppDbContext> _options;

    public AuthTest(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgresFixture.ConnectionString)
            .Options;
    }

    private AuthController CreateController(AppDbContext context)
    {
        var logger = new Logger<AuthAccountsFactory>(new LoggerFactory());
        var authFactory = new AuthAccountsFactory(context, logger);

        var inMemorySettings = new Dictionary<string, string>
        {
            { "Jwt:SecretKey", "super_duper_secret_jwt_protected_unhacked_key"}
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        var authService = new AuthService(context, config, authFactory);

        var authController = new AuthController(authService);

        return authController;
    }

    [Fact]
    public async Task Test_RegisterAndLoginByPassword_ReturnToken()
    {
        //Arrange
        await using var context = new AppDbContext(_options);
        var authController = CreateController(context);

        var passwordDto = new PasswordAccountDto
        {
            Email = "test",
            Password = "test"
        };

        //Act
        var regResult = await authController.RegisterByPassword(passwordDto);
        var logResult = await authController.Login(passwordDto);

        //Assert
        var okRegResult = Assert.IsType<OkObjectResult>(regResult);
        var regMessage = okRegResult.Value.GetType()
               .GetProperty("message")?
               .GetValue(okRegResult.Value, null)?
               .ToString();
        Assert.Equal("Registration successful", regMessage);

        var okLogResult = Assert.IsType<OkObjectResult>(logResult);
        var token = okLogResult.Value.GetType()
               .GetProperty("token")?
               .GetValue(okLogResult.Value, null)?
               .ToString();
        Assert.NotEmpty(token);
    }


    [Fact]
    public async Task Test_LoginByTelegram_ReturnToken()
    {
        //Arrange
        await using var context = new AppDbContext(_options);
        var authController = CreateController(context);
        
        var tgDto = new TelegramAccountDto 
        { 
            Id = 1, 
            Username = "Test" 
        };

        //Act
        var result = await authController.LoginByTelegram(tgDto);

        //Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var obj = okResult.Value;
        var token = obj.GetType()
               .GetProperty("token")?
               .GetValue(obj, null)?
               .ToString();

        Assert.NotEmpty(token);
    }
}
