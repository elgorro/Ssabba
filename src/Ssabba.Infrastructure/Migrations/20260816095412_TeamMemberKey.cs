using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssabba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TeamMemberKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MemberKey",
                table: "Teams",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");

            // Fill the key for teams that predate it, in the same shape TeamRoster.Key produces:
            // the members' ids as unhyphenated hex, sorted, joined with hyphens.
            migrationBuilder.Sql("""
                UPDATE "Teams" AS t
                SET "MemberKey" = COALESCE(k.key, '')
                FROM (
                    SELECT "TeamId",
                           string_agg(replace("PlayerId"::text, '-', ''), '-' ORDER BY "PlayerId") AS key
                    FROM "TeamMembers"
                    GROUP BY "TeamId"
                ) AS k
                WHERE k."TeamId" = t."Id";
                """);

            // The same lineup is now one team per community, so any duplicates left over have to be
            // folded together before the unique index can go on: keep the oldest, move its matches
            // and tournament entries across, drop the rest (their members cascade).
            migrationBuilder.Sql("""
                CREATE TEMP TABLE team_merge AS
                SELECT t."Id" AS loser,
                       first_value(t."Id") OVER w AS winner
                FROM "Teams" AS t
                WINDOW w AS (PARTITION BY t."CommunityId", t."MemberKey" ORDER BY t."Id");

                DELETE FROM team_merge WHERE loser = winner;

                -- The survivor keeps its own name where it has one, and stops being a one-off if
                -- any of the rows folded into it was a standing pairing.
                UPDATE "Teams" AS w
                SET "Name" = COALESCE(w."Name", agg.name),
                    "IsAdHoc" = w."IsAdHoc" AND agg.adhoc
                FROM (
                    SELECT mg.winner, min(l."Name") AS name, bool_and(l."IsAdHoc") AS adhoc
                    FROM team_merge AS mg
                    JOIN "Teams" AS l ON l."Id" = mg.loser
                    GROUP BY mg.winner
                ) AS agg
                WHERE w."Id" = agg.winner;

                UPDATE "Matches" AS m SET "HomeTeamId" = mg.winner
                FROM team_merge AS mg WHERE m."HomeTeamId" = mg.loser;

                UPDATE "Matches" AS m SET "AwayTeamId" = mg.winner
                FROM team_merge AS mg WHERE m."AwayTeamId" = mg.loser;

                DELETE FROM "TournamentEntries" AS e
                USING team_merge AS mg
                WHERE e."TeamId" = mg.loser
                  AND EXISTS (
                      SELECT 1 FROM "TournamentEntries" AS w
                      WHERE w."TournamentId" = e."TournamentId" AND w."TeamId" = mg.winner);

                UPDATE "TournamentEntries" AS e SET "TeamId" = mg.winner
                FROM team_merge AS mg WHERE e."TeamId" = mg.loser;

                DELETE FROM "Teams" AS t USING team_merge AS mg WHERE t."Id" = mg.loser;

                DROP TABLE team_merge;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_CommunityId_MemberKey",
                table: "Teams",
                columns: new[] { "CommunityId", "MemberKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teams_CommunityId_MemberKey",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "MemberKey",
                table: "Teams");
        }
    }
}
