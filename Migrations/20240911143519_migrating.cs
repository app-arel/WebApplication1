using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class migrating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentOption",
                table: "OrderDetails",
                type: "longtext",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "TransactionResponse",
                table: "OrderDetails",
                type: "longtext",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentOption",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "TransactionResponse",
                table: "OrderDetails");
        }
    }
}
