using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssabba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchDepth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CommunityId",
                table: "Tournaments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "FormatId",
                table: "Tournaments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "RuleSetId",
                table: "Tournaments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SeasonId",
                table: "Tournaments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Tournaments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Tournaments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "VenueId",
                table: "Tournaments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CommunityId",
                table: "Teams",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsAdHoc",
                table: "Teams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "TeamMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "TeamMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BracketSlot",
                table: "Matches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CommunityId",
                table: "Matches",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ConfirmedAt",
                table: "Matches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Matches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FormatId",
                table: "Matches",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "PointsPerSet",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RatingAppliedAt",
                table: "Matches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecordedByMemberId",
                table: "Matches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RuleSetId",
                table: "Matches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SeasonId",
                table: "Matches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                table: "Matches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SetsToWin",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TiebreakPoints",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TournamentRound",
                table: "Matches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WinBy",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MatchAppearances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Side = table.Column<int>(type: "integer", nullable: false),
                    IsSubstitute = table.Column<bool>(type: "boolean", nullable: false),
                    RatingBefore = table.Column<int>(type: "integer", nullable: false),
                    RatingAfter = table.Column<int>(type: "integer", nullable: false),
                    RatingDelta = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchAppearances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchAppearances_CommunityMembers_MemberId",
                        column: x => x.MemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchAppearances_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchAppearances_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchDisputes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    RaisedByMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RaisedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedByMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    Resolution = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchDisputes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchDisputes_CommunityMembers_RaisedByMemberId",
                        column: x => x.RaisedByMemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchDisputes_CommunityMembers_ResolvedByMemberId",
                        column: x => x.ResolvedByMemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MatchDisputes_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerFormatStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    FormatId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Matches = table.Column<int>(type: "integer", nullable: false),
                    Wins = table.Column<int>(type: "integer", nullable: false),
                    Losses = table.Column<int>(type: "integer", nullable: false),
                    SetsWon = table.Column<int>(type: "integer", nullable: false),
                    SetsLost = table.Column<int>(type: "integer", nullable: false),
                    PointsFor = table.Column<int>(type: "integer", nullable: false),
                    PointsAgainst = table.Column<int>(type: "integer", nullable: false),
                    LastPlayedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerFormatStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerFormatStats_CommunityMembers_MemberId",
                        column: x => x.MemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerFormatStats_Formats_FormatId",
                        column: x => x.FormatId,
                        principalTable: "Formats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerFormatStats_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TournamentEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Seed = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RegisteredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FinalRank = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentEntries_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentEntries_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_CommunityId_StartsOn",
                table: "Tournaments",
                columns: new[] { "CommunityId", "StartsOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_FormatId",
                table: "Tournaments",
                column: "FormatId");

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_RuleSetId",
                table: "Tournaments",
                column: "RuleSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_SeasonId",
                table: "Tournaments",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_VenueId",
                table: "Tournaments",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_CommunityId_IsAdHoc",
                table: "Teams",
                columns: new[] { "CommunityId", "IsAdHoc" });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_CommunityId_PlayedAt",
                table: "Matches",
                columns: new[] { "CommunityId", "PlayedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_CommunityId_Status_RatingAppliedAt",
                table: "Matches",
                columns: new[] { "CommunityId", "Status", "RatingAppliedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_FormatId",
                table: "Matches",
                column: "FormatId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_RecordedByMemberId",
                table: "Matches",
                column: "RecordedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_RuleSetId",
                table: "Matches",
                column: "RuleSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_SeasonId",
                table: "Matches",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_SessionId",
                table: "Matches",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchAppearances_MatchId_PlayerId",
                table: "MatchAppearances",
                columns: new[] { "MatchId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchAppearances_MemberId_MatchId",
                table: "MatchAppearances",
                columns: new[] { "MemberId", "MatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_MatchAppearances_PlayerId",
                table: "MatchAppearances",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchDisputes_MatchId_Status",
                table: "MatchDisputes",
                columns: new[] { "MatchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MatchDisputes_RaisedByMemberId",
                table: "MatchDisputes",
                column: "RaisedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchDisputes_ResolvedByMemberId",
                table: "MatchDisputes",
                column: "ResolvedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerFormatStats_FormatId",
                table: "PlayerFormatStats",
                column: "FormatId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerFormatStats_MemberId_FormatId_AllTime",
                table: "PlayerFormatStats",
                columns: new[] { "MemberId", "FormatId" },
                unique: true,
                filter: "\"SeasonId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerFormatStats_MemberId_FormatId_SeasonId",
                table: "PlayerFormatStats",
                columns: new[] { "MemberId", "FormatId", "SeasonId" },
                unique: true,
                filter: "\"SeasonId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerFormatStats_SeasonId",
                table: "PlayerFormatStats",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentEntries_TeamId",
                table: "TournamentEntries",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentEntries_TournamentId_TeamId",
                table: "TournamentEntries",
                columns: new[] { "TournamentId", "TeamId" },
                unique: true);

            // Existing rows predate communities and formats. Attach them to the single community the
            // first migration created, and read each match's format from how many actually played.
            // Done before the foreign keys land, so the placeholder zeros never have to satisfy one.
            migrationBuilder.Sql("""
                UPDATE "Teams" SET "CommunityId" = (SELECT "Id" FROM "Communities" ORDER BY "CreatedAt" LIMIT 1)
                WHERE "CommunityId" = '00000000-0000-0000-0000-000000000000';
                """);

            migrationBuilder.Sql("""
                UPDATE "Tournaments"
                SET "CommunityId" = (SELECT "Id" FROM "Communities" ORDER BY "CreatedAt" LIMIT 1),
                    "FormatId" = (SELECT "Id" FROM "Formats" WHERE "PlayersPerSide" = 2)
                WHERE "CommunityId" = '00000000-0000-0000-0000-000000000000';
                """);

            migrationBuilder.Sql("""
                UPDATE "Matches" m
                SET "CommunityId" = (SELECT "Id" FROM "Communities" ORDER BY "CreatedAt" LIMIT 1),
                    "FormatId" = f."Id",
                    "Status" = 2,
                    "ConfirmedAt" = m."PlayedAt",
                    "SetsToWin" = f."DefaultSetsToWin",
                    "PointsPerSet" = f."DefaultPointsPerSet",
                    "WinBy" = f."DefaultWinBy",
                    "TiebreakPoints" = f."DefaultTiebreakPoints"
                FROM "Formats" f
                WHERE m."CommunityId" = '00000000-0000-0000-0000-000000000000'
                  AND f."PlayersPerSide" = greatest(
                      (SELECT count(*) FROM "TeamMembers" WHERE "TeamId" = m."HomeTeamId"),
                      (SELECT count(*) FROM "TeamMembers" WHERE "TeamId" = m."AwayTeamId"),
                      2);
                """);

            // Reconstruct who played from the lineups. Ratings are left flat: the deltas that would
            // have produced them were never recorded, and inventing them would be a lie.
            migrationBuilder.Sql("""
                INSERT INTO "MatchAppearances"
                    ("Id", "MatchId", "PlayerId", "MemberId", "Side", "IsSubstitute",
                     "RatingBefore", "RatingAfter", "RatingDelta")
                SELECT uuidv7(), m."Id", tm."PlayerId", cm."Id",
                       CASE WHEN m."HomeTeamId" = tm."TeamId" THEN 0 ELSE 1 END,
                       false, cm."Rating", cm."Rating", 0
                FROM "Matches" m
                JOIN "TeamMembers" tm
                  ON tm."TeamId" IN (m."HomeTeamId", m."AwayTeamId")
                JOIN "CommunityMembers" cm
                  ON cm."PlayerId" = tm."PlayerId" AND cm."CommunityId" = m."CommunityId"
                ON CONFLICT DO NOTHING;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Communities_CommunityId",
                table: "Matches",
                column: "CommunityId",
                principalTable: "Communities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_CommunityMembers_RecordedByMemberId",
                table: "Matches",
                column: "RecordedByMemberId",
                principalTable: "CommunityMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Formats_FormatId",
                table: "Matches",
                column: "FormatId",
                principalTable: "Formats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_RuleSets_RuleSetId",
                table: "Matches",
                column: "RuleSetId",
                principalTable: "RuleSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Seasons_SeasonId",
                table: "Matches",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Sessions_SessionId",
                table: "Matches",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Communities_CommunityId",
                table: "Teams",
                column: "CommunityId",
                principalTable: "Communities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tournaments_Communities_CommunityId",
                table: "Tournaments",
                column: "CommunityId",
                principalTable: "Communities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tournaments_Formats_FormatId",
                table: "Tournaments",
                column: "FormatId",
                principalTable: "Formats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tournaments_RuleSets_RuleSetId",
                table: "Tournaments",
                column: "RuleSetId",
                principalTable: "RuleSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tournaments_Seasons_SeasonId",
                table: "Tournaments",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tournaments_Venues_VenueId",
                table: "Tournaments",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Communities_CommunityId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_CommunityMembers_RecordedByMemberId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Formats_FormatId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_RuleSets_RuleSetId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Seasons_SeasonId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Sessions_SessionId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Communities_CommunityId",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_Tournaments_Communities_CommunityId",
                table: "Tournaments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tournaments_Formats_FormatId",
                table: "Tournaments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tournaments_RuleSets_RuleSetId",
                table: "Tournaments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tournaments_Seasons_SeasonId",
                table: "Tournaments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tournaments_Venues_VenueId",
                table: "Tournaments");

            migrationBuilder.DropTable(
                name: "MatchAppearances");

            migrationBuilder.DropTable(
                name: "MatchDisputes");

            migrationBuilder.DropTable(
                name: "PlayerFormatStats");

            migrationBuilder.DropTable(
                name: "TournamentEntries");

            migrationBuilder.DropIndex(
                name: "IX_Tournaments_CommunityId_StartsOn",
                table: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Tournaments_FormatId",
                table: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Tournaments_RuleSetId",
                table: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Tournaments_SeasonId",
                table: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Tournaments_VenueId",
                table: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Teams_CommunityId_IsAdHoc",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Matches_CommunityId_PlayedAt",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_CommunityId_Status_RatingAppliedAt",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_FormatId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_RecordedByMemberId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_RuleSetId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_SeasonId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_SessionId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "CommunityId",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "FormatId",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "RuleSetId",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "VenueId",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "CommunityId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "IsAdHoc",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "BracketSlot",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "CommunityId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "FormatId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "PointsPerSet",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "RatingAppliedAt",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "RecordedByMemberId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "RuleSetId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "SetsToWin",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TiebreakPoints",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TournamentRound",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "WinBy",
                table: "Matches");
        }
    }
}
