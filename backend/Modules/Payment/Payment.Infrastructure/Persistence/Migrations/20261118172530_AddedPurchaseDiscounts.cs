using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedPurchaseDiscounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CouponDiscount",
                schema: "payment",
                table: "Purchases",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "CouponId",
                schema: "payment",
                table: "Purchases",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CouponDiscount",
                schema: "payment",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "CouponId",
                schema: "payment",
                table: "Purchases");
        }
    }
}
