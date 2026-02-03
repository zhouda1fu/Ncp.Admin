namespace Ncp.Admin.Infrastructure.Services;

/// <summary>
/// 刷新令牌生成抽象，便于测试与替换实现
/// </summary>
public interface IRefreshTokenGenerator
{
    /// <summary>
    /// 生成新的刷新令牌
    /// </summary>
    string Generate();
}
