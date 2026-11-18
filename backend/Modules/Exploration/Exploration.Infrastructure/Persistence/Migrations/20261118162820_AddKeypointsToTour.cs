using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exploration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddKeypointsToTour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Keypoints",
                schema: "exploration",
                table: "Tours",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Length",
                schema: "exploration",
                table: "Tours",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Keypoints",
                schema: "exploration",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "Length",
                schema: "exploration",
                table: "Tours");
        }
    }
}
