using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class ForgotPasswordRequestTests
    {
        [Fact]
        public void ForgotPasswordRequest_DefaultValues_AreSetCorrectly()
        {
            var request = new ForgotPasswordRequest();
            Assert.Equal(string.Empty, request.EmailId);
        }

        [Fact]
        public void ForgotPasswordRequest_CanSetProperties()
        {
            var request = new ForgotPasswordRequest
            {
                EmailId = "user@example.com"
            };
            Assert.Equal("user@example.com", request.EmailId);
        }

        [Theory]
        [InlineData("user@example.com")]
        [InlineData("test.email+tag@domain.co.uk")]
        [InlineData("")]
        [InlineData(null)]
        public void ForgotPasswordRequest_EmailId_AllowsAnyString(string? email)
        {
            var request = new ForgotPasswordRequest
            {
                EmailId = email ?? string.Empty
            };
            Assert.Equal(email ?? string.Empty, request.EmailId);
        }

        [Fact]
        public void ForgotPasswordRequest_ValidEmailFormats_CanBeSet()
        {
            var validEmails = new[]
            {
                "simple@example.com",
                "user.name@domain.com",
                "user+tag@domain.org",
                "firstname.lastname@subdomain.example.com"
            };

            foreach (var email in validEmails)
            {
                var request = new ForgotPasswordRequest { EmailId = email };
                Assert.Equal(email, request.EmailId);
            }
        }

        [Fact]
        public void ForgotPasswordRequest_InvalidEmailFormats_CanStillBeSet()
        {
            // Note: The DTO itself doesn't validate - validation happens at the service layer
            var invalidEmails = new[]
            {
                "plainaddress",
                "@missingdomainname.com",
                "missing@.com",
                "missing.domain.name@.com"
            };

            foreach (var email in invalidEmails)
            {
                var request = new ForgotPasswordRequest { EmailId = email };
                Assert.Equal(email, request.EmailId);
            }
        }
    }
}
