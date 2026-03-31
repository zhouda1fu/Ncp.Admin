using FluentValidation;
using MediatR;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Excel;
using Ncp.Admin.Web.Application.Identity.Admin.UserExcel;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;

/// <summary>
/// 从 Excel 解析结果批量创建用户（逐行调用 <see cref="CreateUserCommand"/>）。
/// </summary>
public record ImportUsersCommand(IReadOnlyList<UserImportRowDto> Rows, UserId CreatorId) : ICommand<ImportBatchResultDto>;

public class ImportUsersCommandValidator : AbstractValidator<ImportUsersCommand>
{
    public ImportUsersCommandValidator()
    {
        RuleFor(x => x.Rows)
            .NotNull()
            .WithMessage("导入数据不能为空");
        RuleFor(x => x.Rows!.Count)
            .InclusiveBetween(1, 5000)
            .WithMessage("单次导入行数须在 1～5000 之间");
    }
}

public class ImportUsersCommandHandler(
    IMediator mediator,
    DeptQuery deptQuery,
    PositionQuery positionQuery,
    RoleQuery roleQuery) : ICommandHandler<ImportUsersCommand, ImportBatchResultDto>
{
    public async Task<ImportBatchResultDto> Handle(ImportUsersCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<ImportRowErrorDto>();
        var success = 0;
        foreach (var row in request.Rows)
        {
            try
            {
                var cmd = await BuildCreateCommandAsync(row, request.CreatorId, cancellationToken);
                await mediator.Send(cmd, cancellationToken);
                success++;
            }
            catch (KnownException ex)
            {
                errors.Add(new ImportRowErrorDto(row.RowNumber, ex.Message));
            }
            catch (ValidationException ex)
            {
                var msg = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
                errors.Add(new ImportRowErrorDto(row.RowNumber, msg));
            }
        }

        return new ImportBatchResultDto(success, errors);
    }

    private async Task<CreateUserCommand> BuildCreateCommandAsync(
        UserImportRowDto row,
        UserId creatorId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(row.Name))
        {
            throw new KnownException("用户名不能为空", ErrorCodes.InvalidUser);
        }

        if (string.IsNullOrWhiteSpace(row.Email))
        {
            throw new KnownException("邮箱不能为空", ErrorCodes.InvalidUser);
        }

        if (string.IsNullOrWhiteSpace(row.Password))
        {
            throw new KnownException("初始密码不能为空", ErrorCodes.InvalidUser);
        }

        var email = row.Email.Trim();
        if (email.IndexOf('@') < 0)
        {
            throw new KnownException("邮箱格式不正确", ErrorCodes.InvalidUser);
        }

        if (!UserImportParsing.TryParseInt(row.Status, out var status) || status is not (0 or 1))
        {
            throw new KnownException("状态须为 0（禁用）或 1（启用）", ErrorCodes.InvalidUser);
        }

        if (!UserImportParsing.TryParseDateTimeOffset(row.BirthDate, out var birthDate))
        {
            throw new KnownException("出生日期格式不正确", ErrorCodes.InvalidUser);
        }

        if (!UserImportParsing.TryParseBool(row.IsDeptManager, out var isDeptManager))
        {
            isDeptManager = false;
        }

        if (!UserImportParsing.TryParseBool(row.NotOrderMeal, out var notOrderMeal))
        {
            notOrderMeal = false;
        }

        if (!UserImportParsing.TryParseBool(row.IsResigned, out var isResigned))
        {
            isResigned = false;
        }

        var orderMealSort = 0;
        if (!string.IsNullOrWhiteSpace(row.OrderMealSort) && !UserImportParsing.TryParseInt(row.OrderMealSort, out orderMealSort))
        {
            throw new KnownException("订餐排序须为整数", ErrorCodes.InvalidUser);
        }

        DateTimeOffset resignedTime = default;
        if (isResigned)
        {
            if (!UserImportParsing.TryParseDateTimeOffset(row.ResignedTime, out resignedTime))
            {
                throw new KnownException("已离职时离职时间不能为空且格式须正确", ErrorCodes.InvalidUser);
            }
        }

        DeptId? deptId = null;
        string? deptName = null;
        if (!string.IsNullOrWhiteSpace(row.DeptName))
        {
            var dept = await deptQuery.GetDeptByExactNameAsync(row.DeptName, cancellationToken);
            if (dept == null)
            {
                var exists = await deptQuery.DoesDeptExist(row.DeptName.Trim(), cancellationToken);
                if (exists)
                {
                    throw new KnownException($"部门名称「{row.DeptName.Trim()}」不唯一，请使用唯一部门名称或在系统中消除重名", ErrorCodes.DeptNotFound);
                }

                throw new KnownException($"未找到部门：{row.DeptName.Trim()}", ErrorCodes.DeptNotFound);
            }

            deptId = dept.Id;
            deptName = dept.Name;
        }

        PositionId? positionId = null;
        string? positionName = null;
        if (!string.IsNullOrWhiteSpace(row.PositionName))
        {
            var pos = await positionQuery.GetPositionByNameForImportAsync(row.PositionName, deptId, cancellationToken);
            if (pos == null)
            {
                throw new KnownException(
                    $"未找到岗位或名称不唯一：{row.PositionName.Trim()}（若存在同名岗位请填写部门名称以限定）",
                    ErrorCodes.InvalidUser);
            }

            positionId = pos.Id;
            positionName = pos.Name;
        }

        var roleNameList = UserImportParsing.SplitRoleNames(row.Roles).ToList();
        var (rolesToAssign, missing) = await roleQuery.GetAdminRolesForAssignmentByNamesAsync(roleNameList, cancellationToken);
        if (missing.Count > 0)
        {
            throw new KnownException($"未找到角色：{string.Join('、', missing)}", ErrorCodes.RoleNotFound);
        }

        return new CreateUserCommand(
            row.Name.Trim(),
            email,
            row.Password,
            row.Phone?.Trim() ?? string.Empty,
            string.IsNullOrWhiteSpace(row.RealName) ? row.Name.Trim() : row.RealName.Trim(),
            status,
            row.Gender?.Trim() ?? string.Empty,
            birthDate,
            deptId,
            deptName,
            isDeptManager,
            positionId,
            positionName,
            rolesToAssign,
            creatorId,
            row.IdCardNumber?.Trim() ?? string.Empty,
            row.Address?.Trim() ?? string.Empty,
            row.Education?.Trim() ?? string.Empty,
            row.GraduateSchool?.Trim() ?? string.Empty,
            row.AvatarUrl?.Trim() ?? string.Empty,
            notOrderMeal,
            row.WechatGuid?.Trim() ?? string.Empty,
            isResigned,
            resignedTime);
    }
}
