using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;
using System;
using System.Collections.Generic;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class UserAuditLogResponseTests
    {
        [Fact]
        public void UserAuditLogResponse_DefaultValues_AreSetCorrectly()
        {
            var response = new UserAuditLogResponse();
            Assert.Equal(0, response.UserId);
            Assert.Equal(string.Empty, response.UserName);
            Assert.Equal(string.Empty, response.Email);
            Assert.Equal(default(DateTime), response.LastLoginDate);
            Assert.NotNull(response.ActivityLog);
            Assert.Empty(response.ActivityLog);
            Assert.False(response.IsBlocked);
        }

        [Fact]
        public void UserAuditLogResponse_CanSetProperties()
        {
            var now = new DateTime(2025, 5, 27, 10, 0, 0);
            var log = new List<string> { "Login", "PasswordChanged" };
            var response = new UserAuditLogResponse
            {
                UserId = 7,
                UserName = "admin",
                Email = "admin@email.com",
                LastLoginDate = now,
                ActivityLog = log,
                IsBlocked = true
            };
            Assert.Equal(7, response.UserId);
            Assert.Equal("admin", response.UserName);
            Assert.Equal("admin@email.com", response.Email);
            Assert.Equal(now, response.LastLoginDate);
            Assert.Equal(log, response.ActivityLog);
            Assert.True(response.IsBlocked);
        }
    }
}
