using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLocationChallengeAttemptExperiencePoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwardedExperiencePoints",
                schema: "games",
                table: "LocationChallengeAttempts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AwardedExperiencePoints",
                schema: "games",
                table: "LocationChallengeAttempts",
                type: "integer",
                nullable: true);
        }
    }
}
