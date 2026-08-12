using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssabba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accounts_CommunityMembers_MemberId",
                        column: x => x.MemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: true),
                    IpHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditEvents_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AuditEvents_Players_ActorPlayerId",
                        column: x => x.ActorPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ConsentRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Granted = table.Column<bool>(type: "boolean", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PolicyVersion = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsentRecords_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsentRecords_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DuesPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    AmountMinor = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    Period = table.Column<int>(type: "integer", nullable: false),
                    AppliesToRole = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuesPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuesPlans_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DuesPlans_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    AssetTag = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    PurchasedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    PurchasePriceMinor = table.Column<long>(type: "bigint", nullable: true),
                    Currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: true),
                    Condition = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    HomeVenueId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentItems_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentItems_Venues_HomeVenueId",
                        column: x => x.HomeVenueId,
                        principalTable: "Venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MediaAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: true),
                    UploadedByMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Bytes = table.Column<long>(type: "bigint", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaAssets_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MediaAssets_CommunityMembers_UploadedByMemberId",
                        column: x => x.UploadedByMemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "NotificationOutbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    RecipientPlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: true),
                    ScheduledFor = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AbandonedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationOutbox", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationOutbox_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationOutbox_Players_RecipientPlayerId",
                        column: x => x.RecipientPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Channels = table.Column<int>(type: "integer", nullable: false),
                    LeadTimeMinutes = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationPreferences_CommunityMembers_MemberId",
                        column: x => x.MemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeatherObservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ObservedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FetchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TemperatureC = table.Column<double>(type: "double precision", nullable: true),
                    FeelsLikeC = table.Column<double>(type: "double precision", nullable: true),
                    WindKph = table.Column<double>(type: "double precision", nullable: true),
                    WindGustKph = table.Column<double>(type: "double precision", nullable: true),
                    PrecipitationMm = table.Column<double>(type: "double precision", nullable: true),
                    CloudPercent = table.Column<int>(type: "integer", nullable: true),
                    HumidityPercent = table.Column<int>(type: "integer", nullable: true),
                    ConditionCode = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ConditionText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherObservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeatherObservations_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentLoans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckedOutAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DueBackAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReturnedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConditionOut = table.Column<int>(type: "integer", nullable: false),
                    ConditionIn = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentLoans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentLoans_CommunityMembers_MemberId",
                        column: x => x.MemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipmentLoans_EquipmentItems_EquipmentItemId",
                        column: x => x.EquipmentItemId,
                        principalTable: "EquipmentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentLoans_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DataRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResultMediaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataRequests_MediaAssets_ResultMediaId",
                        column: x => x.ResultMediaId,
                        principalTable: "MediaAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DataRequests_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundingSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    ContactPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContactDetails = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AmountMinor = table.Column<long>(type: "bigint", nullable: true),
                    Currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: true),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: true),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LogoMediaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundingSources_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundingSources_MediaAssets_LogoMediaId",
                        column: x => x.LogoMediaId,
                        principalTable: "MediaAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FundingSources_Players_ContactPlayerId",
                        column: x => x.ContactPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MediaSubjects",
                columns: table => new
                {
                    MediaAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaggedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TaggedByMemberId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaSubjects", x => new { x.MediaAssetId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_MediaSubjects_CommunityMembers_TaggedByMemberId",
                        column: x => x.TaggedByMemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MediaSubjects_MediaAssets_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "MediaAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MediaSubjects_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DuesAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DuesPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueOn = table.Column<DateOnly>(type: "date", nullable: false),
                    AmountMinor = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaidLedgerEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WaivedReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuesAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuesAssignments_CommunityMembers_MemberId",
                        column: x => x.MemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DuesAssignments_DuesPlans_DuesPlanId",
                        column: x => x.DuesPlanId,
                        principalTable: "DuesPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LedgerEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DebitAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreditAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    AmountMinor = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    EquipmentItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    FundingSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceiptMediaId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReversesEntryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LedgerEntries_Accounts_CreditAccountId",
                        column: x => x.CreditAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LedgerEntries_Accounts_DebitAccountId",
                        column: x => x.DebitAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LedgerEntries_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LedgerEntries_CommunityMembers_CreatedByMemberId",
                        column: x => x.CreatedByMemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LedgerEntries_EquipmentItems_EquipmentItemId",
                        column: x => x.EquipmentItemId,
                        principalTable: "EquipmentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LedgerEntries_FundingSources_FundingSourceId",
                        column: x => x.FundingSourceId,
                        principalTable: "FundingSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LedgerEntries_LedgerEntries_ReversesEntryId",
                        column: x => x.ReversesEntryId,
                        principalTable: "LedgerEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LedgerEntries_MediaAssets_ReceiptMediaId",
                        column: x => x.ReceiptMediaId,
                        principalTable: "MediaAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LedgerEntries_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    EquipmentItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    CourtId = table.Column<Guid>(type: "uuid", nullable: true),
                    RaisedByMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RaisedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CostLedgerEntryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_CommunityMembers_AssignedToMemberId",
                        column: x => x.AssignedToMemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_CommunityMembers_RaisedByMemberId",
                        column: x => x.RaisedByMemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Courts_CourtId",
                        column: x => x.CourtId,
                        principalTable: "Courts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_EquipmentItems_EquipmentItemId",
                        column: x => x.EquipmentItemId,
                        principalTable: "EquipmentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_LedgerEntries_CostLedgerEntryId",
                        column: x => x.CostLedgerEntryId,
                        principalTable: "LedgerEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CommunityId_Kind",
                table: "Accounts",
                columns: new[] { "CommunityId", "Kind" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_MemberId",
                table: "Accounts",
                column: "MemberId",
                unique: true,
                filter: "\"MemberId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_ActorPlayerId",
                table: "AuditEvents",
                column: "ActorPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_CommunityId_OccurredAt",
                table: "AuditEvents",
                columns: new[] { "CommunityId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_EntityType_EntityId",
                table: "AuditEvents",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsentRecords_CommunityId",
                table: "ConsentRecords",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentRecords_PlayerId_Kind_RecordedAt",
                table: "ConsentRecords",
                columns: new[] { "PlayerId", "Kind", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DataRequests_PlayerId_Status",
                table: "DataRequests",
                columns: new[] { "PlayerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DataRequests_ResultMediaId",
                table: "DataRequests",
                column: "ResultMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_DuesAssignments_DuesPlanId_MemberId_DueOn",
                table: "DuesAssignments",
                columns: new[] { "DuesPlanId", "MemberId", "DueOn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DuesAssignments_MemberId_Status",
                table: "DuesAssignments",
                columns: new[] { "MemberId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DuesAssignments_PaidLedgerEntryId",
                table: "DuesAssignments",
                column: "PaidLedgerEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_DuesPlans_CommunityId_IsActive",
                table: "DuesPlans",
                columns: new[] { "CommunityId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DuesPlans_SeasonId",
                table: "DuesPlans",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentItems_CommunityId_AssetTag",
                table: "EquipmentItems",
                columns: new[] { "CommunityId", "AssetTag" },
                unique: true,
                filter: "\"AssetTag\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentItems_CommunityId_Status",
                table: "EquipmentItems",
                columns: new[] { "CommunityId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentItems_HomeVenueId",
                table: "EquipmentItems",
                column: "HomeVenueId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentLoans_EquipmentItemId_Outstanding",
                table: "EquipmentLoans",
                column: "EquipmentItemId",
                unique: true,
                filter: "\"ReturnedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentLoans_MemberId_ReturnedAt",
                table: "EquipmentLoans",
                columns: new[] { "MemberId", "ReturnedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentLoans_SessionId",
                table: "EquipmentLoans",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingSources_CommunityId_Status",
                table: "FundingSources",
                columns: new[] { "CommunityId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FundingSources_ContactPlayerId",
                table: "FundingSources",
                column: "ContactPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingSources_LogoMediaId",
                table: "FundingSources",
                column: "LogoMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_CommunityId_Category",
                table: "LedgerEntries",
                columns: new[] { "CommunityId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_CommunityId_OccurredAt",
                table: "LedgerEntries",
                columns: new[] { "CommunityId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_CreatedByMemberId",
                table: "LedgerEntries",
                column: "CreatedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_CreditAccountId",
                table: "LedgerEntries",
                column: "CreditAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_DebitAccountId",
                table: "LedgerEntries",
                column: "DebitAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_EquipmentItemId",
                table: "LedgerEntries",
                column: "EquipmentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_FundingSourceId",
                table: "LedgerEntries",
                column: "FundingSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_ReceiptMediaId",
                table: "LedgerEntries",
                column: "ReceiptMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_ReversesEntryId",
                table: "LedgerEntries",
                column: "ReversesEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_ServiceRequestId",
                table: "LedgerEntries",
                column: "ServiceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_SessionId",
                table: "LedgerEntries",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_CommunityId_CreatedAt",
                table: "MediaAssets",
                columns: new[] { "CommunityId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_Sha256",
                table: "MediaAssets",
                column: "Sha256",
                filter: "\"Sha256\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_UploadedByMemberId",
                table: "MediaAssets",
                column: "UploadedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaSubjects_PlayerId",
                table: "MediaSubjects",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaSubjects_TaggedByMemberId",
                table: "MediaSubjects",
                column: "TaggedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationOutbox_CommunityId",
                table: "NotificationOutbox",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationOutbox_Pending",
                table: "NotificationOutbox",
                column: "ScheduledFor",
                filter: "\"SentAt\" IS NULL AND \"AbandonedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationOutbox_RecipientPlayerId",
                table: "NotificationOutbox",
                column: "RecipientPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_MemberId_Kind",
                table: "NotificationPreferences",
                columns: new[] { "MemberId", "Kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_AssignedToMemberId",
                table: "ServiceRequests",
                column: "AssignedToMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_CommunityId_Status_Priority",
                table: "ServiceRequests",
                columns: new[] { "CommunityId", "Status", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_CostLedgerEntryId",
                table: "ServiceRequests",
                column: "CostLedgerEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_CourtId",
                table: "ServiceRequests",
                column: "CourtId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_EquipmentItemId",
                table: "ServiceRequests",
                column: "EquipmentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_RaisedByMemberId",
                table: "ServiceRequests",
                column: "RaisedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherObservations_SessionId",
                table: "WeatherObservations",
                column: "SessionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DuesAssignments_LedgerEntries_PaidLedgerEntryId",
                table: "DuesAssignments",
                column: "PaidLedgerEntryId",
                principalTable: "LedgerEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerEntries_ServiceRequests_ServiceRequestId",
                table: "LedgerEntries",
                column: "ServiceRequestId",
                principalTable: "ServiceRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Money moves between two different accounts, and only ever forwards.
            migrationBuilder.Sql("""
                ALTER TABLE "LedgerEntries"
                ADD CONSTRAINT "CK_LedgerEntries_AmountPositive" CHECK ("AmountMinor" > 0),
                ADD CONSTRAINT "CK_LedgerEntries_DistinctAccounts" CHECK ("DebitAccountId" <> "CreditAccountId");
                """);

            // A member balance belongs to a member; nothing else does.
            migrationBuilder.Sql("""
                ALTER TABLE "Accounts"
                ADD CONSTRAINT "CK_Accounts_MemberBalanceHasMember"
                CHECK (("Kind" = 2) = ("MemberId" IS NOT NULL));
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "EquipmentLoans"
                ADD CONSTRAINT "CK_EquipmentLoans_ReturnedAfterCheckout"
                CHECK ("ReturnedAt" IS NULL OR "ReturnedAt" >= "CheckedOutAt");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FundingSources_MediaAssets_LogoMediaId",
                table: "FundingSources");

            migrationBuilder.DropForeignKey(
                name: "FK_LedgerEntries_MediaAssets_ReceiptMediaId",
                table: "LedgerEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_LedgerEntries_CostLedgerEntryId",
                table: "ServiceRequests");

            migrationBuilder.DropTable(
                name: "AuditEvents");

            migrationBuilder.DropTable(
                name: "ConsentRecords");

            migrationBuilder.DropTable(
                name: "DataRequests");

            migrationBuilder.DropTable(
                name: "DuesAssignments");

            migrationBuilder.DropTable(
                name: "EquipmentLoans");

            migrationBuilder.DropTable(
                name: "MediaSubjects");

            migrationBuilder.DropTable(
                name: "NotificationOutbox");

            migrationBuilder.DropTable(
                name: "NotificationPreferences");

            migrationBuilder.DropTable(
                name: "WeatherObservations");

            migrationBuilder.DropTable(
                name: "DuesPlans");

            migrationBuilder.DropTable(
                name: "MediaAssets");

            migrationBuilder.DropTable(
                name: "LedgerEntries");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "FundingSources");

            migrationBuilder.DropTable(
                name: "ServiceRequests");

            migrationBuilder.DropTable(
                name: "EquipmentItems");
        }
    }
}
