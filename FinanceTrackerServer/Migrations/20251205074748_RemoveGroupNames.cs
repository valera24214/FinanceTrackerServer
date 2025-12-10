using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTrackerServer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGroupNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Groups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Groups",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
