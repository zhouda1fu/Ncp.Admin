using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Services.Notification;

/// <summary>
/// 将站内通知同步发送到微信公众号模板消息。
/// </summary>
public class WeChatNotificationSender(
    ApplicationDbContext dbContext,
    IWeChatOfficialAccountClient weChatClient,
    INotificationLinkResolver linkResolver,
    IOptions<WeChatOfficialAccountOptions> options,
    ILogger<WeChatNotificationSender> logger) : INotificationChannel
{
    public async Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        var opt = options.Value;
        if (!opt.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(opt.NoticeTemplateId))
        {
            logger.LogWarning("微信公众号推送已启用，但模板消息ID为空。");
            return;
        }

        var openId = await dbContext.Users.AsNoTracking()
            .Where(u => u.Id == new UserId(message.ReceiverId) && !u.IsDeleted)
            .Select(u => u.WechatGuid)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(openId))
        {
            return;
        }

        var result = await weChatClient.SendNoticeTemplateAsync(
            openId,
            message.Title,
            message.SenderName ?? string.Empty,
            message.CreatedAt,
            await linkResolver.ResolveAsync(message, cancellationToken),
            cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "微信模板消息发送失败，接收人ID：{ReceiverId}，通知ID：{NotificationId}，错误码：{ErrCode}，错误信息：{ErrMsg}",
                message.ReceiverId,
                message.NotificationId?.Id,
                result.ErrCode,
                result.ErrMsg);
        }
    }
}
