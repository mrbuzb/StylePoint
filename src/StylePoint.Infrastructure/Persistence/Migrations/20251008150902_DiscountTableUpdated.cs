using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StylePoint.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DiscountTableUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Discounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "UsageLimit",
                table: "Discounts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "UserDiscounts",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    DiscountId = table.Column<long>(type: "bigint", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDiscounts", x => new { x.UserId, x.DiscountId });
                    table.ForeignKey(
                        name: "FK_UserDiscounts_Discounts_DiscountId",
                        column: x => x.DiscountId,
                        principalTable: "Discounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDiscounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDiscounts_DiscountId",
                table: "UserDiscounts",
                column: "DiscountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDiscounts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "UsageLimit",
                table: "Discounts");
        }
    }
}
