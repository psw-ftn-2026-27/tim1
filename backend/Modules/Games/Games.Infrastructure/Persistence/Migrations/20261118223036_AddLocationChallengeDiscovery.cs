using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationChallengeDiscovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AwardedExperiencePoints",
                schema: "games",
                table: "LocationChallengeAttempts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid[]>(
                name: "DiscoveredPointIds",
                schema: "games",
                table: "LocationChallengeAttempts",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "SolvedAt",
                schema: "games",
                table: "LocationChallengeAttempts",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwardedExperiencePoints",
                schema: "games",
                table: "LocationChallengeAttempts");

            migrationBuilder.DropColumn(
                name: "DiscoveredPointIds",
                schema: "games",
                table: "LocationChallengeAttempts");

            migrationBuilder.DropColumn(
                name: "SolvedAt",
                schema: "games",
                table: "LocationChallengeAttempts");
        }
    }
}
