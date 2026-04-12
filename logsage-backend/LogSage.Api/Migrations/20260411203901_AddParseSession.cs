using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogSage.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddParseSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParseSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionToken = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InputSample = table.Column<string>(type: "text", nullable: true),
                    InputLineCount = table.Column<int>(type: "integer", nullable: false),
                    InputSizeBytes = table.Column<int>(type: "integer", nullable: false),
                    DetectedFormat = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ParseSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    TotalEntries = table.Column<int>(type: "integer", nullable: false),
                    InfoCount = table.Column<int>(type: "integer", nullable: false),
                    WarningCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorCount = table.Column<int>(type: "integer", nullable: false),
                    DebugCount = table.Column<int>(type: "integer", nullable: false),
                    ParseErrorCount = table.Column<int>(type: "integer", nullable: false),
                    ParseErrorSamples = table.Column<string>(type: "text", nullable: true),
                    DurationMs = table.Column<int>(type: "integer", nullable: false),
                    FeedbackScore = table.Column<int>(type: "integer", nullable: true),
                    FeedbackNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FeedbackAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParseSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParseSessions_SessionToken",
                table: "ParseSessions",
                column: "SessionToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParseSessions");
        }
    }
}
