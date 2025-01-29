using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmallLister.Security;

namespace SmallLister.Tests.Security
{
    [TestClass]
    public class SaltHashHelperTests
    {
        [TestMethod]
        public void CreateSaltHash()
        {
            var password = "password to hash";
            var (salt, hash) = SaltHashHelper.CreateHash(password);

            salt.Should().NotBeNullOrEmpty();
            hash.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void VerifyHash()
        {
            var password = "password to hash";
            var (salt, hash) = SaltHashHelper.CreateHash(password);

            var verify = SaltHashHelper.CreateHash(password, salt);
            verify.Should().Be(hash);
        }
    }
}