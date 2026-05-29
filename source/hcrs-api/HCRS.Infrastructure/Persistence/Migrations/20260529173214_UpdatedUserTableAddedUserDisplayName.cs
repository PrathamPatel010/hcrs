using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HCRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedUserTableAddedUserDisplayName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserDisplayName",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserDisplayName",
                table: "Users");
        }
    }
}
