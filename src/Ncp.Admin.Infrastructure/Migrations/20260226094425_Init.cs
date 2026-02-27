using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ncp.Admin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "announcement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    PublisherId = table.Column<long>(type: "bigint", nullable: false),
                    PublisherName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PublishAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_announcement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "announcement_read_record",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnnouncementId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_announcement_read_record", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "asset",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PurchaseDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "asset_allocation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AllocatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReturnedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_allocation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "attendance_record",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CheckInAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CheckOutAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_record", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CAPLock",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Instance = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LastLockTime = table.Column<DateTime>(type: "TIMESTAMP", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAPLock", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "CAPPublishedMessage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    Retries = table.Column<int>(type: "integer", nullable: true),
                    Added = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: true),
                    StatusName = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAPPublishedMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CAPReceivedMessage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Name = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Group = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    Retries = table.Column<int>(type: "integer", nullable: true),
                    Added = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: true),
                    StatusName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAPReceivedMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "chat_group",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_group", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "chat_message",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<long>(type: "bigint", nullable: false),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ReplyToMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_message", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "contact_group",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contact_group", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "contract",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PartyA = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PartyB = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FileStorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contract", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true),
                    DeptId = table.Column<long>(type: "bigint", nullable: true),
                    CustomerSource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ShortName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Nature = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProvinceCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CityCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DistrictCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CoverRegion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RegisterAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MainContactName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MainContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    WechatStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Remark = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsKeyAccount = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    CombineFlag = table.Column<bool>(type: "boolean", nullable: false),
                    IsInSea = table.Column<bool>(type: "boolean", nullable: false),
                    ReleasedToSeaAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dept",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dept", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "document",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "expense_claim",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<long>(type: "bigint", nullable: false),
                    ApplicantName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_claim", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "industry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_industry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "leave_balance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    LeaveType = table.Column<int>(type: "integer", nullable: false),
                    TotalDays = table.Column<decimal>(type: "numeric", nullable: false),
                    UsedDays = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_balance", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "leave_request",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<long>(type: "bigint", nullable: false),
                    ApplicantName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LeaveType = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Days = table.Column<decimal>(type: "numeric", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_request", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "meeting_booking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MeetingRoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookerId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meeting_booking", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "meeting_room",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    Equipment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meeting_room", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    SenderId = table.Column<long>(type: "bigint", nullable: true),
                    SenderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReceiverId = table.Column<long>(type: "bigint", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    BusinessId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BusinessType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "position",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DeptId = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_position", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DataScope = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "schedule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    WorkDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    ShiftName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "share_link",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_share_link", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "task",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AssigneeId = table.Column<long>(type: "bigint", nullable: true),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RealName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastLoginTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    BirthDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlateNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_booking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookerId = table.Column<long>(type: "bigint", nullable: false),
                    Purpose = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_booking", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_definition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DefinitionJson = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_definition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_instance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowDefinitionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BusinessKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BusinessType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    InitiatorId = table.Column<long>(type: "bigint", nullable: false),
                    InitiatorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InitiatorDeptId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentNodeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Variables = table.Column<string>(type: "text", nullable: false),
                    Remark = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_instance", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "chat_group_member",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ChatGroupId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_group_member", x => x.Id);
                    table.ForeignKey(
                        name: "FK_chat_group_member_chat_group_ChatGroupId",
                        column: x => x.ChatGroupId,
                        principalTable: "chat_group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContactType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: true),
                    Birthday = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Position = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Mobile = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_contact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_contact_customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_industry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    IndustryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_industry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_industry_customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_version",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    FileStorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_version", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_version_document_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "expense_item",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    InvoiceUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExpenseClaimId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_expense_item_expense_claim_ExpenseClaimId",
                        column: x => x.ExpenseClaimId,
                        principalTable: "expense_claim",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permission",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PermissionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PermissionDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permission", x => new { x.RoleId, x.PermissionCode });
                    table.ForeignKey(
                        name: "FK_role_permission_role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_comment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AuthorId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_comment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_task_comment_task_TaskId",
                        column: x => x.TaskId,
                        principalTable: "task",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_dept",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    DeptId = table.Column<long>(type: "bigint", nullable: false),
                    DeptName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_dept", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_user_dept_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "user_refresh_token",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_refresh_token", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_refresh_token_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_role",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_role", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_user_role_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_node",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NodeType = table.Column<int>(type: "integer", nullable: false),
                    AssigneeType = table.Column<int>(type: "integer", nullable: false),
                    AssigneeValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApprovalMode = table.Column<int>(type: "integer", nullable: false),
                    ConditionExpression = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TrueNextNodeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FalseNextNodeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_node", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_node_workflow_definition_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "workflow_definition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_task",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TaskType = table.Column<int>(type: "integer", nullable: false),
                    AssigneeType = table.Column<int>(type: "integer", nullable: false),
                    AssigneeId = table.Column<long>(type: "bigint", nullable: true),
                    AssigneeRoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssigneeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_task", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_task_workflow_instance_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "workflow_instance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_announcement_CreatedAt",
                table: "announcement",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_announcement_PublishAt",
                table: "announcement",
                column: "PublishAt");

            migrationBuilder.CreateIndex(
                name: "IX_announcement_PublisherId",
                table: "announcement",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_announcement_Status",
                table: "announcement",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_announcement_read_record_AnnouncementId_UserId",
                table: "announcement_read_record",
                columns: new[] { "AnnouncementId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_announcement_read_record_UserId",
                table: "announcement_read_record",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_asset_Code",
                table: "asset",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_asset_Status",
                table: "asset",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_asset_allocation_AssetId",
                table: "asset_allocation",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_asset_allocation_UserId",
                table: "asset_allocation",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_record_CheckInAt",
                table: "attendance_record",
                column: "CheckInAt");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_record_UserId",
                table: "attendance_record",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_record_UserId_CheckInAt",
                table: "attendance_record",
                columns: new[] { "UserId", "CheckInAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ExpiresAt_StatusName",
                table: "CAPPublishedMessage",
                columns: new[] { "ExpiresAt", "StatusName" });

            migrationBuilder.CreateIndex(
                name: "IX_Version_ExpiresAt_StatusName",
                table: "CAPPublishedMessage",
                columns: new[] { "Version", "ExpiresAt", "StatusName" });

            migrationBuilder.CreateIndex(
                name: "IX_ExpiresAt_StatusName1",
                table: "CAPReceivedMessage",
                columns: new[] { "ExpiresAt", "StatusName" });

            migrationBuilder.CreateIndex(
                name: "IX_Version_ExpiresAt_StatusName1",
                table: "CAPReceivedMessage",
                columns: new[] { "Version", "ExpiresAt", "StatusName" });

            migrationBuilder.CreateIndex(
                name: "IX_chat_group_CreatorId",
                table: "chat_group",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_group_Type",
                table: "chat_group",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_chat_group_member_ChatGroupId",
                table: "chat_group_member",
                column: "ChatGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_group_member_UserId",
                table: "chat_group_member",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_message_ChatGroupId",
                table: "chat_message",
                column: "ChatGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_message_CreatedAt",
                table: "chat_message",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_chat_message_SenderId",
                table: "chat_message",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_contact_CreatorId",
                table: "contact",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_contact_GroupId",
                table: "contact",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_contact_group_CreatorId",
                table: "contact_group",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_contract_Code",
                table: "contract",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_contract_EndDate",
                table: "contract",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_contract_Status",
                table: "contract",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_customer_CreatedAt",
                table: "customer",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_customer_FullName",
                table: "customer",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_customer_IsInSea",
                table: "customer",
                column: "IsInSea");

            migrationBuilder.CreateIndex(
                name: "IX_customer_OwnerId",
                table: "customer",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_contact_CustomerId",
                table: "customer_contact",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_industry_CustomerId_IndustryId",
                table: "customer_industry",
                columns: new[] { "CustomerId", "IndustryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dept_IsDeleted",
                table: "dept",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_dept_ParentId",
                table: "dept",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_dept_Status",
                table: "dept",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_document_CreatorId",
                table: "document",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_document_version_DocumentId",
                table: "document_version",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_expense_claim_ApplicantId",
                table: "expense_claim",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_expense_claim_CreatedAt",
                table: "expense_claim",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_expense_claim_Status",
                table: "expense_claim",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_expense_item_ExpenseClaimId",
                table: "expense_item",
                column: "ExpenseClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_industry_ParentId",
                table: "industry",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_leave_balance_UserId_Year_LeaveType",
                table: "leave_balance",
                columns: new[] { "UserId", "Year", "LeaveType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_leave_request_ApplicantId",
                table: "leave_request",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_leave_request_CreatedAt",
                table: "leave_request",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_leave_request_Status",
                table: "leave_request",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_leave_request_WorkflowInstanceId",
                table: "leave_request",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_meeting_booking_BookerId",
                table: "meeting_booking",
                column: "BookerId");

            migrationBuilder.CreateIndex(
                name: "IX_meeting_booking_MeetingRoomId",
                table: "meeting_booking",
                column: "MeetingRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_meeting_booking_StartAt",
                table: "meeting_booking",
                column: "StartAt");

            migrationBuilder.CreateIndex(
                name: "IX_meeting_room_Status",
                table: "meeting_room",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_notification_IsDeleted",
                table: "notification",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_notification_IsRead",
                table: "notification",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_notification_ReceiverId",
                table: "notification",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_notification_ReceiverId_IsRead_IsDeleted",
                table: "notification",
                columns: new[] { "ReceiverId", "IsRead", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_Type",
                table: "notification",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_position_Code",
                table: "position",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_position_DeptId",
                table: "position",
                column: "DeptId");

            migrationBuilder.CreateIndex(
                name: "IX_position_IsDeleted",
                table: "position",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_position_Status",
                table: "position",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_project_CreatorId",
                table: "project",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_project_Status",
                table: "project",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_role_Name",
                table: "role",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_schedule_UserId",
                table: "schedule",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_UserId_WorkDate",
                table: "schedule",
                columns: new[] { "UserId", "WorkDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_schedule_WorkDate",
                table: "schedule",
                column: "WorkDate");

            migrationBuilder.CreateIndex(
                name: "IX_share_link_DocumentId",
                table: "share_link",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_share_link_Token",
                table: "share_link",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_task_AssigneeId",
                table: "task",
                column: "AssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_task_ProjectId",
                table: "task",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_task_Status",
                table: "task",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_task_comment_TaskId",
                table: "task_comment",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_user_Email",
                table: "user",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_user_Name",
                table: "user",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_user_dept_DeptId",
                table: "user_dept",
                column: "DeptId");

            migrationBuilder.CreateIndex(
                name: "IX_user_dept_UserId",
                table: "user_dept",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_refresh_token_UserId",
                table: "user_refresh_token",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_PlateNumber",
                table: "vehicle",
                column: "PlateNumber");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_Status",
                table: "vehicle",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_booking_BookerId",
                table: "vehicle_booking",
                column: "BookerId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_booking_StartAt",
                table: "vehicle_booking",
                column: "StartAt");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_booking_VehicleId",
                table: "vehicle_booking",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_definition_Category",
                table: "workflow_definition",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_definition_IsDeleted",
                table: "workflow_definition",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_definition_Name",
                table: "workflow_definition",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_definition_Status",
                table: "workflow_definition",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_BusinessKey",
                table: "workflow_instance",
                column: "BusinessKey");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_BusinessType",
                table: "workflow_instance",
                column: "BusinessType");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_InitiatorDeptId",
                table: "workflow_instance",
                column: "InitiatorDeptId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_InitiatorId",
                table: "workflow_instance",
                column: "InitiatorId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_Status",
                table: "workflow_instance",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_WorkflowDefinitionId",
                table: "workflow_instance",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_node_WorkflowDefinitionId",
                table: "workflow_node",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_node_WorkflowDefinitionId_SortOrder",
                table: "workflow_node",
                columns: new[] { "WorkflowDefinitionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_task_AssigneeId",
                table: "workflow_task",
                column: "AssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_task_AssigneeId_Status",
                table: "workflow_task",
                columns: new[] { "AssigneeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_task_AssigneeRoleId",
                table: "workflow_task",
                column: "AssigneeRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_task_Status",
                table: "workflow_task",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_task_WorkflowInstanceId",
                table: "workflow_task",
                column: "WorkflowInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "announcement");

            migrationBuilder.DropTable(
                name: "announcement_read_record");

            migrationBuilder.DropTable(
                name: "asset");

            migrationBuilder.DropTable(
                name: "asset_allocation");

            migrationBuilder.DropTable(
                name: "attendance_record");

            migrationBuilder.DropTable(
                name: "CAPLock");

            migrationBuilder.DropTable(
                name: "CAPPublishedMessage");

            migrationBuilder.DropTable(
                name: "CAPReceivedMessage");

            migrationBuilder.DropTable(
                name: "chat_group_member");

            migrationBuilder.DropTable(
                name: "chat_message");

            migrationBuilder.DropTable(
                name: "contact");

            migrationBuilder.DropTable(
                name: "contact_group");

            migrationBuilder.DropTable(
                name: "contract");

            migrationBuilder.DropTable(
                name: "customer_contact");

            migrationBuilder.DropTable(
                name: "customer_industry");

            migrationBuilder.DropTable(
                name: "dept");

            migrationBuilder.DropTable(
                name: "document_version");

            migrationBuilder.DropTable(
                name: "expense_item");

            migrationBuilder.DropTable(
                name: "industry");

            migrationBuilder.DropTable(
                name: "leave_balance");

            migrationBuilder.DropTable(
                name: "leave_request");

            migrationBuilder.DropTable(
                name: "meeting_booking");

            migrationBuilder.DropTable(
                name: "meeting_room");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "position");

            migrationBuilder.DropTable(
                name: "project");

            migrationBuilder.DropTable(
                name: "role_permission");

            migrationBuilder.DropTable(
                name: "schedule");

            migrationBuilder.DropTable(
                name: "share_link");

            migrationBuilder.DropTable(
                name: "task_comment");

            migrationBuilder.DropTable(
                name: "user_dept");

            migrationBuilder.DropTable(
                name: "user_refresh_token");

            migrationBuilder.DropTable(
                name: "user_role");

            migrationBuilder.DropTable(
                name: "vehicle");

            migrationBuilder.DropTable(
                name: "vehicle_booking");

            migrationBuilder.DropTable(
                name: "workflow_node");

            migrationBuilder.DropTable(
                name: "workflow_task");

            migrationBuilder.DropTable(
                name: "chat_group");

            migrationBuilder.DropTable(
                name: "customer");

            migrationBuilder.DropTable(
                name: "document");

            migrationBuilder.DropTable(
                name: "expense_claim");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "task");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "workflow_definition");

            migrationBuilder.DropTable(
                name: "workflow_instance");
        }
    }
}
