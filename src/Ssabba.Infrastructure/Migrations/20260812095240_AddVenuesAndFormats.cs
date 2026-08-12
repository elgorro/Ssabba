using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ssabba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVenuesAndFormats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Matches",
                newName: "LocationNote");

            migrationBuilder.AddColumn<Guid>(
                name: "CourtId",
                table: "Matches",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Formats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<int>(type: "integer", nullable: false),
                    PlayersPerSide = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DefaultSetsToWin = table.Column<int>(type: "integer", nullable: false),
                    DefaultPointsPerSet = table.Column<int>(type: "integer", nullable: false),
                    DefaultWinBy = table.Column<int>(type: "integer", nullable: false),
                    DefaultTiebreakPoints = table.Column<int>(type: "integer", nullable: false),
                    RatingWeightPercent = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Formats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Venues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Address = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    OwnerCommunityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OpeningHours = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Venues_Communities_OwnerCommunityId",
                        column: x => x.OwnerCommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RuleSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    FormatId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SetsToWin = table.Column<int>(type: "integer", nullable: false),
                    PointsPerSet = table.Column<int>(type: "integer", nullable: false),
                    WinBy = table.Column<int>(type: "integer", nullable: false),
                    TiebreakPoints = table.Column<int>(type: "integer", nullable: false),
                    SwitchEveryPoints = table.Column<int>(type: "integer", nullable: true),
                    LetServeAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RuleSets_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RuleSets_Formats_FormatId",
                        column: x => x.FormatId,
                        principalTable: "Formats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Courts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Surface = table.Column<int>(type: "integer", nullable: false),
                    NetHeightCm = table.Column<int>(type: "integer", nullable: true),
                    HasLighting = table.Column<bool>(type: "boolean", nullable: false),
                    MaxTeamSize = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Courts_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourtReservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourtId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    HeldByCommunityId = table.Column<Guid>(type: "uuid", nullable: true),
                    HeldByMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CostMinor = table.Column<long>(type: "bigint", nullable: true),
                    Currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourtReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourtReservations_Communities_HeldByCommunityId",
                        column: x => x.HeldByCommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CourtReservations_CommunityMembers_HeldByMemberId",
                        column: x => x.HeldByMemberId,
                        principalTable: "CommunityMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CourtReservations_Courts_CourtId",
                        column: x => x.CourtId,
                        principalTable: "Courts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Formats",
                columns: new[] { "Id", "Code", "DefaultPointsPerSet", "DefaultSetsToWin", "DefaultTiebreakPoints", "DefaultWinBy", "Name", "PlayersPerSide", "RatingWeightPercent" },
                values: new object[,]
                {
                    { new Guid("0195f000-0000-7000-8000-000000000002"), 2, 21, 2, 15, 2, "2v2", 2, 100 },
                    { new Guid("0195f000-0000-7000-8000-000000000003"), 3, 21, 2, 15, 2, "3v3", 3, 85 },
                    { new Guid("0195f000-0000-7000-8000-000000000004"), 4, 21, 2, 15, 2, "4v4", 4, 70 },
                    { new Guid("0195f000-0000-7000-8000-000000000005"), 5, 25, 2, 15, 2, "5v5", 5, 60 },
                    { new Guid("0195f000-0000-7000-8000-000000000006"), 6, 25, 2, 15, 2, "6v6", 6, 50 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_CourtId",
                table: "Matches",
                column: "CourtId");

            migrationBuilder.CreateIndex(
                name: "IX_CourtReservations_CourtId_StartsAt",
                table: "CourtReservations",
                columns: new[] { "CourtId", "StartsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CourtReservations_HeldByCommunityId",
                table: "CourtReservations",
                column: "HeldByCommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_CourtReservations_HeldByMemberId",
                table: "CourtReservations",
                column: "HeldByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Courts_VenueId_Name",
                table: "Courts",
                columns: new[] { "VenueId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Formats_Code",
                table: "Formats",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RuleSets_CommunityId_FormatId_Default",
                table: "RuleSets",
                columns: new[] { "CommunityId", "FormatId" },
                unique: true,
                filter: "\"IsDefault\"");

            migrationBuilder.CreateIndex(
                name: "IX_RuleSets_FormatId",
                table: "RuleSets",
                column: "FormatId");

            migrationBuilder.CreateIndex(
                name: "IX_Venues_OwnerCommunityId",
                table: "Venues",
                column: "OwnerCommunityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Courts_CourtId",
                table: "Matches",
                column: "CourtId",
                principalTable: "Courts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // A slot cannot be held twice. Application-level checks race, so the database decides.
            // btree_gist is what lets a plain equality column join a range in one GiST index.
            migrationBuilder.Sql("""CREATE EXTENSION IF NOT EXISTS btree_gist;""");

            migrationBuilder.Sql("""
                ALTER TABLE "CourtReservations"
                ADD CONSTRAINT "CK_CourtReservations_EndsAfterStart"
                CHECK ("EndsAt" > "StartsAt");
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "CourtReservations"
                ADD CONSTRAINT "EX_CourtReservations_NoOverlap"
                EXCLUDE USING gist (
                    "CourtId" WITH =,
                    tstzrange("StartsAt", "EndsAt", '[)') WITH &&
                ) WHERE ("Status" = 0);
                """);

            // Give every community that already exists a default rule set per format, so a match can
            // always resolve one without the UI having to ask first.
            migrationBuilder.Sql("""
                INSERT INTO "RuleSets"
                    ("Id", "CommunityId", "FormatId", "Name", "SetsToWin", "PointsPerSet",
                     "WinBy", "TiebreakPoints", "SwitchEveryPoints", "LetServeAllowed", "IsDefault", "Notes")
                SELECT uuidv7(), c."Id", f."Id", f."Name" || ' house rules',
                       f."DefaultSetsToWin", f."DefaultPointsPerSet", f."DefaultWinBy",
                       f."DefaultTiebreakPoints", NULL, true, true, NULL
                FROM "Communities" c
                CROSS JOIN "Formats" f;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "CourtReservations" DROP CONSTRAINT IF EXISTS "EX_CourtReservations_NoOverlap";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "CourtReservations" DROP CONSTRAINT IF EXISTS "CK_CourtReservations_EndsAfterStart";
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Courts_CourtId",
                table: "Matches");

            migrationBuilder.DropTable(
                name: "CourtReservations");

            migrationBuilder.DropTable(
                name: "RuleSets");

            migrationBuilder.DropTable(
                name: "Courts");

            migrationBuilder.DropTable(
                name: "Formats");

            migrationBuilder.DropTable(
                name: "Venues");

            migrationBuilder.DropIndex(
                name: "IX_Matches_CourtId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "CourtId",
                table: "Matches");

            migrationBuilder.RenameColumn(
                name: "LocationNote",
                table: "Matches",
                newName: "Location");
        }
    }
}
