using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyanvieBE.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTransactionIdToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PaymentTransactionId",
                table: "Orders",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentTransactionId",
                table: "Orders");
        }
    }
}
