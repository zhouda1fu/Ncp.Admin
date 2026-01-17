using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.DeptCommands;

/// <summary>
/// 删除部门命令
/// </summary>
public record DeleteDeptCommand(DeptId Id) : ICommand;

/// <summary>
/// 删除部门命令处理器
/// </summary>
public class DeleteDeptCommandHandler(IDeptRepository deptRepository, ApplicationDbContext dbContext) : ICommandHandler<DeleteDeptCommand>
{
    public async Task Handle(DeleteDeptCommand request, CancellationToken cancellationToken)
    {
        var dept = await deptRepository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException($"未找到部门，Id = {request.Id}");

        // 检查是否有子部门
        var hasChildren = await dbContext.Depts
            .AnyAsync(d => d.ParentId == request.Id && !d.IsDeleted, cancellationToken);

        if (hasChildren)
        {
            throw new KnownException("该部门下存在子部门，无法删除");
        }

        dept.SoftDelete();
    }
}
