using RestaurantPOS.Services;

namespace RestaurantPOS.Tests
{
    public class PasswordSecurityTests
    {
        [Fact]
        public void Hash_AndVerify_PasswordWorks()
        {
            string hash = PasswordSecurity.Hash("123456");
            Assert.StartsWith("PBKDF2$", hash);
            Assert.True(PasswordSecurity.Verify("123456", hash));
            Assert.False(PasswordSecurity.Verify("wrong", hash));
        }

        [Fact]
        public void Verify_LegacyPlainText_RemainsCompatible()
        {
            Assert.True(PasswordSecurity.Verify("123456", "123456"));
            Assert.True(PasswordSecurity.IsLegacy("123456"));
        }
    }
}
