using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;

/// <summary>
/// 更新用户命令
/// </summary>
public record UpdateUserCommand(
    UserId UserId,
    string Name,
    string Email,
    string Phone,
    string RealName,
    int Status,
    string Gender,
    int Age,
    DateTimeOffset BirthDate,
    DeptId DeptId,
    string DeptName,
    bool IsDeptManager,
    PositionId? PositionId,
    string? PositionName,
    string PasswordHash,
    string IdCardNumber,
    string Address,
    string Education,
    string GraduateSchool,
    string AvatarUrl,
    bool NotOrderMeal,
    int OrderMealSort,
    string WechatGuid,
    bool IsResigned,
    DateTimeOffset ResignedTime,
    UserId ModifierId) : ICommand<UserId>;

/// <summary>
/// 更新用户命令验证器
/// </summary>
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
        RuleFor(x => x.Name).NotEmpty().WithMessage("用户名不能为空");
        When(u => u.IsResigned, () =>
        {
            RuleFor(u => u.ResignedTime).NotNull().WithMessage("离职时间不能为空");
        });
    }
}

/// <summary>
/// 更新用户命令处理器
/// </summary>
public class UpdateUserCommandHandler(IUserRepository userRepository, IMemoryCache memoryCache) : ICommandHandler<UpdateUserCommand, UserId>
{
    public async Task<UserId> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken) ??
                   throw new KnownException($"未找到用户，UserId = {request.UserId}", ErrorCodes.UserNotFound);

        user.UpdateUserInfo(
            request.Name,
            request.Phone,
            request.RealName,
            request.Status,
            request.Email,
            request.Gender,
            request.BirthDate,
            request.IdCardNumber ?? string.Empty,
            request.Address ?? string.Empty,
            request.Education ?? string.Empty,
            request.GraduateSchool ?? string.Empty,
            request.AvatarUrl ?? string.Empty,
            request.NotOrderMeal,
            request.OrderMealSort,
            request.WechatGuid ?? string.Empty,
            request.IsResigned,
            request.ResignedTime,
            request.ModifierId);

        // 如果提供了新密码，则更新密码
        if (!string.IsNullOrEmpty(request.PasswordHash))
        {
            user.UpdatePassword(request.PasswordHash);
        }

        // 分配部门
        if (request.DeptId != new DeptId(0) && !string.IsNullOrEmpty(request.DeptName))
        {
            var dept = new UserDept(user.Id, request.DeptId, request.DeptName, request.IsDeptManager);
            user.AssignDept(dept);
        }

        // 分配岗位（null 或空表示清除岗位）
        if (request.PositionId != null && !string.IsNullOrEmpty(request.PositionName))
        {
            var position = new UserPosition(user.Id, request.PositionId!, request.PositionName);
            user.AssignPosition(position);
        }
        else
        {
            user.AssignPosition(null);
        }

        memoryCache.Remove(UserQuery.GetUserCacheKey(request.UserId));
        return user.Id;
    }
}
