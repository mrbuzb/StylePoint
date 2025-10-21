using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StylePoint.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SecretCodeAddedToProduct_Fixed2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecretCode",
                table: "Products",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
        UPDATE Products
        SET SecretCode = CAST(NEWID() AS nvarchar(450))
        WHERE SecretCode = ''
    ");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SecretCode",
                table: "Products",
                column: "SecretCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_SecretCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SecretCode",
                table: "Products");
        }
    }
}
