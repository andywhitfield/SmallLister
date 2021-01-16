using FluentAssertions;
using SmallLister.Security;
using Xunit;

namespace SmallLister.Tests.Security
{
    public class SaltHashHelperTests
    {
        [Fact]
        public void CreateSaltHash()
        {
            var password = "password to hash";
            var (salt, hash) = SaltHashHelper.CreateHash(password);

            salt.Should().NotBeNullOrEmpty();
            hash.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void VerifyHash()
        {
            var password = "password to hash";
            var (salt, hash) = SaltHashHelper.CreateHash(password);

            var verify = SaltHashHelper.CreateHash(password, salt);
            verify.Should().Be(hash);
        }
    }
}