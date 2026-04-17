using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogSage.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFeedbackNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeedbackNote",
                table: "ParseSessions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeedbackNote",
                table: "ParseSessions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
