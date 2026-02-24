using Ncp.Admin.Domain.AggregatesModel.MeetingAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 会议室仓储接口
/// </summary>
public interface IMeetingRoomRepository : IRepository<MeetingRoom, MeetingRoomId> { }

/// <summary>
/// 会议室仓储实现
/// </summary>
public class MeetingRoomRepository(ApplicationDbContext context)
    : RepositoryBase<MeetingRoom, MeetingRoomId, ApplicationDbContext>(context), IMeetingRoomRepository { }
