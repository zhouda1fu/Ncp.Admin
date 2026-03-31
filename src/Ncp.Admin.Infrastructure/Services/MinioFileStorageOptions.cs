namespace Ncp.Admin.Infrastructure.Services;

/// <summary>
/// MinIO 文件存储配置
/// </summary>
public class MinioFileStorageOptions
{
    public const string SectionName = "FileStorage:MinIO";

    /// <summary>
    /// MinIO 服务端点（如 localhost:9000，不要带 http/https 前缀）
    /// </summary>
    public string Endpoint { get; set; } = "localhost:9000";

    /// <summary>
    /// 访问密钥
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// 秘密密钥
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// 存储桶名称
    /// </summary>
    public string Bucket { get; set; } = "ncp-admin";

    /// <summary>
    /// 是否使用 SSL
    /// </summary>
    public bool UseSSL { get; set; }
}
