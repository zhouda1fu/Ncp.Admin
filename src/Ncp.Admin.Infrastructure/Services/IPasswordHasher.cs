namespace Ncp.Admin.Infrastructure.Services;

/// <summary>
/// 密码哈希与校验抽象，便于测试与替换实现
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// 对明文密码进行哈希
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// 校验明文密码与已存储的哈希是否匹配
    /// </summary>
    bool Verify(string password, string hashedPassword);
}
