using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTrackerServer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInviteCodeFromGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InviteCode",
                table: "Groups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InviteCode",
                table: "Groups",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
