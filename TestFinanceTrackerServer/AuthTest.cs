using FinanceTrackerServer.Controllers;
using FinanceTrackerServer.Data;
using FinanceTrackerServer.Models.DTO.AuthAccounts;
using FinanceTrackerServer.Models.Entities;
using FinanceTrackerServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestFinanceTrackerServer.Fixtures;

namespace TestFinanceTrackerServer;

[Collection("Postgres collection")]
public class AuthTest
{
    private readonly PostgresFixture _postgresFixture;
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly IDistributedCache _cache;

    public AuthTest(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgresFixture.ConnectionString)
            .Options;

        _cache = new MemoryDistributedCache
            (Options.Create
                (new Microsoft.Extensions.Caching.Memory.MemoryDistributedCacheOptions())
            );
    }

    private AuthController CreateController(AppDbContext context)
    {
        var logger = new Logger<AuthAccountsFactory>(new LoggerFactory());
        var authFactory = new AuthAccountsFactory(context);

        var inMemorySettings = new Dictionary<string, string>
        {
            { "Jwt:SecretKey", "super_duper_secret_jwt_protected_unhacked_key"},          
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        var authService = new AuthService(context, _cache, config, authFactory);

        var authController = new AuthController(authService);

        return authController;
    }

    [Fact]
    public async Task Test_RegisterAndLoginByPassword_ReturnToken()
    {
        //Arrange
        await using var context = new AppDbContext(_options);
        var authController = CreateController(context);

        var email = "test";
        var password = "test";
        var passwordDto = new PasswordAccountDto
        {
            Email = email,
            Password = password
        };

        var code = "123456";

        //Act
        await _cache.SetStringAsync(code, email, new DistributedCacheEntryOptions()
        { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) });
        var verifyResult = await authController.VerifyEmail(code);
        var okVerifyResult = Assert.IsType<OkObjectResult>(verifyResult);
        var verifyMessage = okVerifyResult.Value.GetType()
               .GetProperty("Registration_token")?
               .GetValue(okVerifyResult.Value, null)?
               .ToString();

        var regResult = await authController.SetPassword(verifyMessage, password);
        var logResult = await authController.LoginByPassword(passwordDto);

        //Assert
        var okRegResult = Assert.IsType<OkResult>(regResult);

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
