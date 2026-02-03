using System.Security.Cryptography;

namespace Ncp.Admin.Infrastructure.Services;

/// <summary>
/// 基于加密随机数的刷新令牌生成器
/// </summary>
public class DefaultRefreshTokenGenerator : IRefreshTokenGenerator
{
    public string Generate()
    {
        var randomNumber = new byte[32];
        using var rnd = RandomNumberGenerator.Create();
        rnd.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
