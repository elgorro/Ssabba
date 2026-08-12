using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssabba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Players_SubjectId",
                table: "Players");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Players",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Locale",
                table: "Players",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredTimeZone",
                table: "Players",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Players",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            // Slugs are unique, so derive them from the display name and suffix the collisions.
            migrationBuilder.Sql("""
                WITH slugged AS (
                    SELECT "Id",
                           coalesce(nullif(trim(BOTH '-' FROM regexp_replace(
                               translate(
                                   replace(replace(replace(replace(lower("DisplayName"),
                                       'ß', 'ss'), 'ü', 'ue'), 'ö', 'oe'), 'ä', 'ae'),
                                   'àáâãåèéêëìíîïòóôõùúûñç',
                                   'aaaaaeeeeiiiioooouuunc'),
                               '[^a-z0-9]+', '-', 'g')), ''), 'player') AS base,
                           "CreatedAt"
                    FROM "Players"
                ),
                numbered AS (
                    SELECT "Id", base,
                           row_number() OVER (PARTITION BY base ORDER BY "CreatedAt", "Id") AS rn
                    FROM slugged
                )
                UPDATE "Players" p
                SET "Slug" = n.base || CASE WHEN n.rn = 1 THEN '' ELSE '-' || n.rn::text END
                FROM numbered n
                WHERE p."Id" = n."Id";
                """);

            migrationBuilder.CreateTable(
                name: "Communities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TimeZone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    PublicKeyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Communities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Label = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerContacts_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerProfiles",
                columns: table => new
                {
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeightCm = table.Column<int>(type: "integer", nullable: true),
                    PreferredPositions = table.Column<int>(type: "integer", nullable: false),
                    SelfRatedLevel = table.Column<int>(type: "integer", nullable: true),
                    PlayingSince = table.Column<int>(type: "integer", nullable: true),
                    Bio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsLeftHanded = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerProfiles", x => x.PlayerId);
                    table.ForeignKey(
                        name: "FK_PlayerProfiles_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommunityLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceCommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetCommunityUri = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    TargetPublicKeyId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SharedSecretHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityLinks_Communities_SourceCommunityId",
                        column: x => x.SourceCommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommunityMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nickname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    RatingDeviation = table.Column<int>(type: "integer", nullable: false),
                    MatchesPlayed = table.Column<int>(type: "integer", nullable: false),
                    ReliabilityScore = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityMembers_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunityMembers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seasons_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommunityInvites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    InvitedByMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AcceptedByPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityInvites_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunityInvites_CommunityMembers_InvitedByMemberId",
                        column: x => x.InvitedByMemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunityInvites_Players_AcceptedByPlayerId",
                        column: x => x.AcceptedByPlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "JoinRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DecidedByMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JoinRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JoinRequests_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JoinRequests_CommunityMembers_DecidedByMemberId",
                        column: x => x.DecidedByMemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_JoinRequests_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Players_Slug",
                table: "Players",
                column: "Slug",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Players_SubjectId",
                table: "Players",
                column: "SubjectId",
                unique: true,
                filter: "\"SubjectId\" IS NOT NULL AND \"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Communities_PublicKeyId",
                table: "Communities",
                column: "PublicKeyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Communities_Slug",
                table: "Communities",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvites_AcceptedByPlayerId",
                table: "CommunityInvites",
                column: "AcceptedByPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvites_CommunityId_ExpiresAt",
                table: "CommunityInvites",
                columns: new[] { "CommunityId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvites_InvitedByMemberId",
                table: "CommunityInvites",
                column: "InvitedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvites_TokenHash",
                table: "CommunityInvites",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunityLinks_SourceCommunityId_TargetCommunityUri",
                table: "CommunityLinks",
                columns: new[] { "SourceCommunityId", "TargetCommunityUri" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMembers_CommunityId_PlayerId",
                table: "CommunityMembers",
                columns: new[] { "CommunityId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMembers_CommunityId_Rating",
                table: "CommunityMembers",
                columns: new[] { "CommunityId", "Rating" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMembers_PlayerId",
                table: "CommunityMembers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_JoinRequests_CommunityId_PlayerId_Pending",
                table: "JoinRequests",
                columns: new[] { "CommunityId", "PlayerId" },
                unique: true,
                filter: "\"Status\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_JoinRequests_DecidedByMemberId",
                table: "JoinRequests",
                column: "DecidedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_JoinRequests_PlayerId",
                table: "JoinRequests",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerContacts_PlayerId_Kind_Value",
                table: "PlayerContacts",
                columns: new[] { "PlayerId", "Kind", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_CommunityId_Current",
                table: "Seasons",
                column: "CommunityId",
                unique: true,
                filter: "\"IsCurrent\"");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_CommunityId_StartsOn",
                table: "Seasons",
                columns: new[] { "CommunityId", "StartsOn" });

            // Ratings move from the player to their standing in a community. Existing installs have
            // exactly one implicit group, so give it a home and enrol everyone at their current rating.
            migrationBuilder.Sql("""
                INSERT INTO "Communities"
                    ("Id", "Name", "Slug", "Description", "TimeZone", "Currency", "Visibility", "PublicKeyId", "CreatedAt")
                SELECT uuidv7(), 'Ssabba', 'ssabba', NULL, 'UTC', 'EUR', 0, uuidv7(), now()
                WHERE EXISTS (SELECT 1 FROM "Players");
                """);

            migrationBuilder.Sql("""
                INSERT INTO "CommunityMembers"
                    ("Id", "CommunityId", "PlayerId", "Nickname", "Role", "Status",
                     "Rating", "RatingDeviation", "MatchesPlayed", "ReliabilityScore", "JoinedAt", "LeftAt")
                SELECT uuidv7(), c."Id", p."Id", NULL, 1, 1,
                       p."Rating", 350, 0, 100, p."CreatedAt", NULL
                FROM "Players" p
                CROSS JOIN (SELECT "Id" FROM "Communities" WHERE "Slug" = 'ssabba') c;
                """);

            migrationBuilder.Sql("""
                UPDATE "CommunityMembers" cm
                SET "MatchesPlayed" = played.n
                FROM (
                    SELECT tm."PlayerId", count(DISTINCT m."Id") AS n
                    FROM "TeamMembers" tm
                    JOIN "Matches" m ON m."HomeTeamId" = tm."TeamId" OR m."AwayTeamId" = tm."TeamId"
                    GROUP BY tm."PlayerId"
                ) played
                WHERE cm."PlayerId" = played."PlayerId";
                """);

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Players");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 1000);

            // Carry ratings back before the memberships holding them are dropped. A player in several
            // communities has several ratings and only one column, so keep the best-attested one.
            migrationBuilder.Sql("""
                UPDATE "Players" p
                SET "Rating" = best."Rating"
                FROM (
                    SELECT DISTINCT ON ("PlayerId") "PlayerId", "Rating"
                    FROM "CommunityMembers"
                    ORDER BY "PlayerId", "MatchesPlayed" DESC, "JoinedAt"
                ) best
                WHERE p."Id" = best."PlayerId";
                """);

            migrationBuilder.DropTable(
                name: "CommunityInvites");

            migrationBuilder.DropTable(
                name: "CommunityLinks");

            migrationBuilder.DropTable(
                name: "JoinRequests");

            migrationBuilder.DropTable(
                name: "PlayerContacts");

            migrationBuilder.DropTable(
                name: "PlayerProfiles");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropTable(
                name: "CommunityMembers");

            migrationBuilder.DropTable(
                name: "Communities");

            migrationBuilder.DropIndex(
                name: "IX_Players_Slug",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Players_SubjectId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Locale",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "PreferredTimeZone",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Players");

            migrationBuilder.CreateIndex(
                name: "IX_Players_SubjectId",
                table: "Players",
                column: "SubjectId",
                unique: true,
                filter: "\"SubjectId\" IS NOT NULL");
        }
    }
}
