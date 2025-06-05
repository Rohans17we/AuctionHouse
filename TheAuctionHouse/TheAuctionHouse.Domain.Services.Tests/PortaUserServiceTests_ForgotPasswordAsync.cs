using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Data.EFCore.InMemory;
using TheAuctionHouse.Domain.Entities;

namespace TheAuctionHouse.Domain.Services.Tests;

public class PortaUserServiceTests_ForgotPasswordAsync
{    private IAppUnitOfWork GetSqliteAppUnitOfWork()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<SqliteDbContext>()
            .UseInMemoryDatabase(System.Guid.NewGuid().ToString()) // Using InMemory for testing but with SQLite context
            .Options;
        var context = new SqliteDbContext(options);
        return new SqliteUnitOfWork(context);
    }

    [Fact]
    public async Task ForgotPasswordAsync_ShouldSendEmailForPasswordReset()    {
        //Arrange
        string emailSubject = string.Empty;
        IAppUnitOfWork appUnitOfWork = GetSqliteAppUnitOfWork();
        Mock<IEmailService> emailServiceMock = new Mock<IEmailService>();
        emailServiceMock.Setup(svc => svc.SendEmailAsync("testemail@domain.com", "Password Reset | The Auction House", "", true))
        .Callback<string, string, string, bool>((to, subject, body, isHtml) =>
        {
            emailSubject = subject;
        });

        var portalUser = new PortalUser()
        {
            Id = 1,            Name = "Test User",
            EmailId = "testemail@domain.com",
            HashedPassword = "TestPassword",
            WalletBalance = 0
        };
        await appUnitOfWork.PortalUserRepository.AddAsync(portalUser);
        await appUnitOfWork.SaveChangesAsync();

        PortalUserService portalUserService = new PortalUserService(appUnitOfWork, emailServiceMock.Object);

        //Act
        Result<bool> result = await portalUserService.ForgotPasswordAsync(new ForgotPasswordRequest()
        {
            EmailId = "testemail@domain.com"
        });

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Password Reset | The Auction House", emailSubject);
    }
    [Fact]    public async Task ForgotPasswordAsync_ShouldFailIfEmailAddressIsEmpty()
    {
        //Arrange
        IAppUnitOfWork appUnitOfWork = GetSqliteAppUnitOfWork();
        Mock<IEmailService> emailServiceMock = new Mock<IEmailService>();
        PortalUserService portalUserService = new PortalUserService(appUnitOfWork, emailServiceMock.Object);

        //Act
        Result<bool> result = await portalUserService.ForgotPasswordAsync(new ForgotPasswordRequest()
        {
            EmailId = ""
        });

        //Assert
        Assert.True(!result.IsSuccess);
        Assert.Equal(422, result.Error.ErrorCode);
    }    [Fact]
    public async Task ForgotPasswordAsync_ShouldFailIfEmailAddressIsInvalid()
    {
        //Arrange
        IAppUnitOfWork appUnitOfWork = GetSqliteAppUnitOfWork();
        Mock<IEmailService> emailServiceMock = new Mock<IEmailService>();
        PortalUserService portalUserService = new PortalUserService(appUnitOfWork, emailServiceMock.Object);

        //Act
        Result<bool> result = await portalUserService.ForgotPasswordAsync(new ForgotPasswordRequest()
        {
            EmailId = "rkokhdfcbank.com"
        });

        //Assert
        Assert.True(!result.IsSuccess);
        Assert.Equal(422, result.Error.ErrorCode);
    }    [Fact]
    public async Task ForgotPasswordAsync_ShouldFailIfEmailAddressIsNotRegistered()
    {
        //Arrange
        IAppUnitOfWork appUnitOfWork = GetSqliteAppUnitOfWork();
        Mock<IEmailService> emailServiceMock = new Mock<IEmailService>();
        PortalUserService portalUserService = new PortalUserService(appUnitOfWork, emailServiceMock.Object);

        //Act
        Result<bool> result = await portalUserService.ForgotPasswordAsync(new ForgotPasswordRequest()
        {
            EmailId = "rk@okhdfcbank.com"
        });

        //Assert
        Assert.True(!result.IsSuccess);
        Assert.Equal(404, result.Error.ErrorCode);
    }
}