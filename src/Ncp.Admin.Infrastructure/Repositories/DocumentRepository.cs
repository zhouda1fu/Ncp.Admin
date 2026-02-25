using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 文档仓储接口
/// </summary>
public interface IDocumentRepository : IRepository<Document, DocumentId> { }

/// <summary>
/// 文档仓储实现
/// </summary>
public class DocumentRepository(ApplicationDbContext context)
    : RepositoryBase<Document, DocumentId, ApplicationDbContext>(context), IDocumentRepository { }
