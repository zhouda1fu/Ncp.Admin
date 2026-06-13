namespace Ncp.Admin.Web.Application.Services.Notification;

/// <summary>
/// 微信公众号通知配置。
/// </summary>
public class WeChatOfficialAccountOptions
{
    public const string SectionName = "WeChatOfficialAccount";

    public bool Enabled { get; set; }

    /// <summary>
    /// 微信公众号 AppId，用于获取 access_token、生成绑定二维码和发送模板消息。
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// 微信公众号 AppSecret，生产环境应通过环境变量或部署配置注入。
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// 通知模板 ID，对应公众号后台的模板消息配置。
    /// </summary>
    public string NoticeTemplateId { get; set; } = "y2ndrZ3vC4w286HRsa_3oC3OxmXHEW2h4TWm3qrq03k";

    /// <summary>
    /// 前端站点根地址，用于模板消息点击跳转。
    /// </summary>
    public string FrontendBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// 模板消息标题最大长度，避免超过微信 thing 类型字段限制。
    /// </summary>
    public int TitleMaxLength { get; set; } = 20;

    /// <summary>
    /// 微信公众号服务器配置 Token，用于验证微信回调签名。
    /// </summary>
    public string CallbackToken { get; set; } = string.Empty;

    /// <summary>
    /// 微信公众号消息加解密密钥。当前扫码绑定回调只处理明文模式，安全模式需后续补充解密逻辑。
    /// </summary>
    public string EncodingAesKey { get; set; } = string.Empty;

    /// <summary>
    /// 临时绑定二维码有效期，单位秒。微信限制范围为 60 到 2592000 秒。
    /// </summary>
    public int BindQrExpireSeconds { get; set; } = 300;
}
