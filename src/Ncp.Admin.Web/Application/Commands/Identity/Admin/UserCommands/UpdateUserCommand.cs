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
    bool AttendanceRequired,
    string WechatGuid,
    bool IsResigned,
    DateTimeOffset ResignedTime,
    bool? SetAsDeptResponsibleUser,
    bool? SetAsDefaultDeptResponsibleUser,
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
        var originalUserName = user.Name;

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
            request.ModifierId,
            request.AttendanceRequired);

        // 如果提供了新密码，则更新密码
        if (!string.IsNullOrEmpty(request.PasswordHash))
        {
            user.UpdatePassword(request.PasswordHash);
        }

        // 分配部门
        if (request.DeptId != DeptId.Unassigned && !string.IsNullOrEmpty(request.DeptName))
        {
            user.AssignDept(request.DeptId, request.DeptName);
        }

        // 分配岗位（null 或空表示清除岗位）
        if (request.PositionId != null && !string.IsNullOrEmpty(request.PositionName))
        {
            user.AssignPosition(request.PositionId, request.PositionName);
        }
        else
        {
            user.ClearPosition();
        }

        if (request.SetAsDeptResponsibleUser.HasValue || request.SetAsDefaultDeptResponsibleUser.HasValue)
        {
            var setAsDeptResponsibleUser =
                request.SetAsDeptResponsibleUser == true || request.SetAsDefaultDeptResponsibleUser == true;
            if (!setAsDeptResponsibleUser)
            {
                user.RequestDeptResponsibleUserClear();
            }
            else
            {
                if (request.DeptId == DeptId.Unassigned)
                {
                    throw new KnownException("部门负责人必须选择部门", ErrorCodes.DeptNotFound);
                }

                // 编辑页提交的是“当前所属部门负责人”状态。先清理旧部门关系，再把当前部门交给部门聚合追加。
                user.RequestDeptResponsibleUserClear();
                user.RequestDeptResponsibleUserAssignment(
                    request.DeptId,
                    request.SetAsDefaultDeptResponsibleUser == true);
            }
        }

        memoryCache.Remove(UserQuery.GetUserCacheKey(request.UserId));
        return user.Id;
    }
}
