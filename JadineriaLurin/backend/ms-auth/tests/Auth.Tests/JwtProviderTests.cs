using System;
using System.Collections.Generic;
using Auth.Application.Common.Interfaces;
using Auth.Application.Common.Models;
using Auth.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace Auth.Tests;

public class JwtProviderTests
{
    [Fact]
    public void GenerateToken_ReturnsNonEmptyToken()
    {
        var settings = Options.Create(new JwtSettings
        {
            SecretKey = ""this_is_a_very_long_secret_key_for_testing_purposes_123456"",
            Issuer = ""test-issuer"",
            Audience = ""test-audience"",
            ExpiryMinutes = 60
        });
        IJwtProvider provider = new JwtProvider(settings);
        var result = provider.GenerateToken(Guid.NewGuid(), ""test@example.com"", ""User"", new List<string> { ""Usuario"" });
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.True(result.ExpiresAtUtc > DateTime.UtcNow);
    }
}
