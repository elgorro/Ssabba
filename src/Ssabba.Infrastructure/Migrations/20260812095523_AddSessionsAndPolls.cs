using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssabba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionsAndPolls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Rrule = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    StartTimeLocal = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    CourtId = table.Column<Guid>(type: "uuid", nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    DefaultRuleSetId = table.Column<Guid>(type: "uuid", nullable: true),
                    GenerateAheadDays = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionTemplates_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionTemplates_Courts_CourtId",
                        column: x => x.CourtId,
                        principalTable: "Courts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SessionTemplates_RuleSets_DefaultRuleSetId",
                        column: x => x.DefaultRuleSetId,
                        principalTable: "RuleSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CourtId = table.Column<Guid>(type: "uuid", nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    MinPlayers = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RuleSetId = table.Column<Guid>(type: "uuid", nullable: true),
                    CostPerPlayerMinor = table.Column<long>(type: "bigint", nullable: false),
                    OrganizerMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sessions_CommunityMembers_OrganizerMemberId",
                        column: x => x.OrganizerMemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Courts_CourtId",
                        column: x => x.CourtId,
                        principalTable: "Courts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Sessions_RuleSets_RuleSetId",
                        column: x => x.RuleSetId,
                        principalTable: "RuleSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Sessions_SessionTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "SessionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Polls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Question = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedByMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosesAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsAnonymous = table.Column<bool>(type: "boolean", nullable: false),
                    AllowMultiple = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ResultSessionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Polls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Polls_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Polls_CommunityMembers_CreatedByMemberId",
                        column: x => x.CreatedByMemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Polls_Sessions_ResultSessionId",
                        column: x => x.ResultSessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SessionParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Response = table.Column<int>(type: "integer", nullable: false),
                    RespondedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    WaitlistPosition = table.Column<int>(type: "integer", nullable: true),
                    Attendance = table.Column<int>(type: "integer", nullable: false),
                    IsGuestOfMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionParticipants_CommunityMembers_IsGuestOfMemberId",
                        column: x => x.IsGuestOfMemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SessionParticipants_CommunityMembers_MemberId",
                        column: x => x.MemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionParticipants_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PollOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PollId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CourtId = table.Column<Guid>(type: "uuid", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollOptions_Courts_CourtId",
                        column: x => x.CourtId,
                        principalTable: "Courts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PollOptions_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Votes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PollOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    CastAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votes_CommunityMembers_MemberId",
                        column: x => x.MemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Votes_PollOptions_PollOptionId",
                        column: x => x.PollOptionId,
                        principalTable: "PollOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PollOptions_CourtId",
                table: "PollOptions",
                column: "CourtId");

            migrationBuilder.CreateIndex(
                name: "IX_PollOptions_PollId_SortOrder",
                table: "PollOptions",
                columns: new[] { "PollId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Polls_CommunityId_Status",
                table: "Polls",
                columns: new[] { "CommunityId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Polls_CreatedByMemberId",
                table: "Polls",
                column: "CreatedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Polls_ResultSessionId",
                table: "Polls",
                column: "ResultSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipants_IsGuestOfMemberId",
                table: "SessionParticipants",
                column: "IsGuestOfMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipants_MemberId",
                table: "SessionParticipants",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipants_SessionId_MemberId",
                table: "SessionParticipants",
                columns: new[] { "SessionId", "MemberId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionTemplates_CommunityId_IsActive",
                table: "SessionTemplates",
                columns: new[] { "CommunityId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SessionTemplates_CourtId",
                table: "SessionTemplates",
                column: "CourtId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionTemplates_DefaultRuleSetId",
                table: "SessionTemplates",
                column: "DefaultRuleSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_CommunityId_StartsAt",
                table: "Sessions",
                columns: new[] { "CommunityId", "StartsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_CommunityId_Status",
                table: "Sessions",
                columns: new[] { "CommunityId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_CourtId",
                table: "Sessions",
                column: "CourtId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_OrganizerMemberId",
                table: "Sessions",
                column: "OrganizerMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_RuleSetId",
                table: "Sessions",
                column: "RuleSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_TemplateId_StartsAt",
                table: "Sessions",
                columns: new[] { "TemplateId", "StartsAt" },
                unique: true,
                filter: "\"TemplateId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_MemberId",
                table: "Votes",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_PollOptionId_MemberId",
                table: "Votes",
                columns: new[] { "PollOptionId", "MemberId" },
                unique: true);

            migrationBuilder.Sql("""
                ALTER TABLE "Sessions"
                ADD CONSTRAINT "CK_Sessions_EndsAfterStart"
                CHECK ("EndsAt" > "StartsAt");
                """);

            // A waiting-list position is only meaningful for someone actually on the waiting list.
            migrationBuilder.Sql("""
                ALTER TABLE "SessionParticipants"
                ADD CONSTRAINT "CK_SessionParticipants_WaitlistPosition"
                CHECK (("WaitlistPosition" IS NULL) OR ("Response" = 3));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""ALTER TABLE "Sessions" DROP CONSTRAINT IF EXISTS "CK_Sessions_EndsAfterStart";""");
            migrationBuilder.Sql("""ALTER TABLE "SessionParticipants" DROP CONSTRAINT IF EXISTS "CK_SessionParticipants_WaitlistPosition";""");

            migrationBuilder.DropTable(
                name: "SessionParticipants");

            migrationBuilder.DropTable(
                name: "Votes");

            migrationBuilder.DropTable(
                name: "PollOptions");

            migrationBuilder.DropTable(
                name: "Polls");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "SessionTemplates");
        }
    }
}
