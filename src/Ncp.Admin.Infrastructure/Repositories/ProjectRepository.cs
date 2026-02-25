using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 项目仓储接口
/// </summary>
public interface IProjectRepository : IRepository<Project, ProjectId> { }

/// <summary>
/// 项目仓储实现
/// </summary>
public class ProjectRepository(ApplicationDbContext context)
    : RepositoryBase<Project, ProjectId, ApplicationDbContext>(context), IProjectRepository { }
