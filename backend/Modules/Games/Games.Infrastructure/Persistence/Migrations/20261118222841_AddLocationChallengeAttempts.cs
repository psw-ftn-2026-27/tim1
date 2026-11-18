using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationChallengeAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationChallengeAttempts",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Circle_DiameterMeters = table.Column<double>(type: "double precision", nullable: false),
                    Circle_Center_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Circle_Center_Longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationChallengeAttempts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationChallengeAttempts_ChallengeId_UserId",
                schema: "games",
                table: "LocationChallengeAttempts",
                columns: new[] { "ChallengeId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationChallengeAttempts",
                schema: "games");
        }
    }
}
