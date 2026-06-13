using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.DeptCommands;

/// <summary>
/// 创建部门命令
/// </summary>
public record CreateDeptCommand(
    string Name,
    string Remark,
    DeptId? ParentId,
    int Status,
    int SortOrder,
    IReadOnlyList<UserId> ResponsibleUserIds,
    UserId? DefaultResponsibleUserId) : ICommand<DeptId>;

public class CreateDeptCommandValidator : AbstractValidator<CreateDeptCommand>
{
    public CreateDeptCommandValidator(DeptQuery deptQuery)
    {
        RuleFor(d => d.Name).NotEmpty().WithMessage("部门名称不能为空");
        RuleFor(d => d.Name).MustAsync(async (n, ct) => !await deptQuery.DoesDeptExist(n, ct))
            .WithMessage(d => $"该部门已存在，Name={d.Name}");
        RuleFor(d => d.Status).InclusiveBetween(0, 1).WithMessage("状态值必须为0或1");
        RuleFor(d => d.SortOrder).GreaterThanOrEqualTo(0).WithMessage("排序号不能小于0");
        RuleFor(d => d)
            .Must(d =>
            {
                var defaultResponsibleUserId = d.DefaultResponsibleUserId;
                return defaultResponsibleUserId is null
                    || defaultResponsibleUserId == UserId.Unassigned
                    || d.ResponsibleUserIds
                        .Where(id => id != UserId.Unassigned)
                        .Distinct()
                        .Contains(defaultResponsibleUserId);
            })
            .WithMessage("默认负责人必须在部门负责人列表中");
    }
}

/// <summary>
/// 创建部门命令处理器
/// </summary>
public class CreateDeptCommandHandler(
    IDeptRepository deptRepository) : ICommandHandler<CreateDeptCommand, DeptId>
{
    public async Task<DeptId> Handle(CreateDeptCommand request, CancellationToken cancellationToken)
    {
        var parentId = request.ParentId ?? DeptId.Unassigned;
        var dept = new Dept(request.Name, request.Remark, parentId, request.Status, request.SortOrder);

        await deptRepository.AddAsync(dept, cancellationToken);
        dept.ReplaceResponsibleUsers(
            request.ResponsibleUserIds,
            request.DefaultResponsibleUserId);

        return dept.Id;
    }
}
