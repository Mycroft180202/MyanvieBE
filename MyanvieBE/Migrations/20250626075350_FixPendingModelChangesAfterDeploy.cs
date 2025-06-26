using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyanvieBE.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingModelChangesAfterDeploy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
