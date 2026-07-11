using CloudConfigurationHub.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace CloudConfigurationHub.Tests.Infrastructure;

public sealed class AesGcmSecretProtectorTests {
    [Fact]
    public void Protect_returns_ciphertext_that_roundtrips_without_plaintext_leakage() {
        var protector = new AesGcmSecretProtector(Options.Create(new ConfigurationValueProtectionOptions {
            MasterKey = "test-master-key-for-sensitive-configuration-values"
        }));

        var protectedValue = protector.Protect("plain-password");
        var roundtrippedValue = protector.Unprotect(protectedValue);

        Assert.NotEqual("plain-password", protectedValue);
        Assert.DoesNotContain("plain-password", protectedValue, StringComparison.Ordinal);
        Assert.True(protector.IsProtected(protectedValue));
        Assert.Equal("plain-password", roundtrippedValue);
    }
}
