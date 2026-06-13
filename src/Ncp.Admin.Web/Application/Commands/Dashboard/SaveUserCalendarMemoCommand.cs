using Ncp.Admin.Domain.AggregatesModel.DashboardAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Dashboard;

/// <summary>
/// 保存指定日期的行事历便签（内容为空则删除该日便签）。
/// </summary>
/// <param name="UserId">用户标识。</param>
/// <param name="MemoDate">便签日期。</param>
/// <param name="Content">便签正文。</param>
public record SaveUserCalendarMemoCommand(UserId UserId, DateOnly MemoDate, string Content) : ICommand;

public class SaveUserCalendarMemoCommandValidator : AbstractValidator<SaveUserCalendarMemoCommand>
{
    public SaveUserCalendarMemoCommandValidator()
    {
        RuleFor(x => x.Content).MaximumLength(4000);
    }
}

public class SaveUserCalendarMemoCommandHandler(IUserCalendarMemoRepository memoRepository)
    : ICommandHandler<SaveUserCalendarMemoCommand>
{
    public async Task Handle(SaveUserCalendarMemoCommand request, CancellationToken cancellationToken)
    {
        if (request.MemoDate < TodayInChina())
        {
            throw new KnownException("不能为过去的日期添加便签");
        }

        var content = request.Content.Trim();
        var existing = await memoRepository.GetByUserAndDateAsync(
            request.UserId,
            request.MemoDate,
            cancellationToken);

        if (string.IsNullOrEmpty(content))
        {
            if (existing is not null)
            {
                memoRepository.Remove(existing);
            }

            return;
        }

        if (existing is null)
        {
            await memoRepository.AddAsync(
                new UserCalendarMemo(request.UserId, request.MemoDate, content),
                cancellationToken);
        }
        else
        {
            existing.UpdateContent(content);
        }
    }

    private static DateOnly TodayInChina()
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));
        }
        catch (TimeZoneNotFoundException)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai");
            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));
        }
    }
}
