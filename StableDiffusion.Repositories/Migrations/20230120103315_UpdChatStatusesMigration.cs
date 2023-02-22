using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StableDiffusion.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdChatStatusesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Prompt",
                table: "Settings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Prompt",
                table: "Settings");
        }
    }
}
