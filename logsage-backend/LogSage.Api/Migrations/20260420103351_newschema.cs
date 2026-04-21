using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogSage.Api.Migrations
{
    /// <inheritdoc />
    public partial class newschema : Migration
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
                    InputSample = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InputLineCount = table.Column<int>(type: "integer", nullable: false),
                    InputSizeBytes = table.Column<int>(type: "integer", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
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
                    Outcome = table.Column<int>(type: "integer", nullable: false),
                    FeedbackScore = table.Column<int>(type: "integer", nullable: true),
                    FeedbackAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParseSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsageTracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    SessionCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageTracking", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Plan = table.Column<string>(type: "text", nullable: false, defaultValue: "free"),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    PaymentCustomerId = table.Column<string>(type: "text", nullable: true),
                    PaymentProvider = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DetectedFormat = table.Column<string>(type: "text", nullable: true),
                    TotalLines = table.Column<int>(type: "integer", nullable: false),
                    ErrorCount = table.Column<int>(type: "integer", nullable: false),
                    WarningCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    ExternalSubscriptionId = table.Column<string>(type: "text", nullable: false),
                    ExternalCustomerId = table.Column<string>(type: "text", nullable: false),
                    PriceId = table.Column<string>(type: "text", nullable: false),
                    Plan = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CurrentPeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentPeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CanceledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErrorGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupKey = table.Column<string>(type: "text", nullable: false),
                    RepresentativeMessage = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<string>(type: "text", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    ExceptionType = table.Column<string>(type: "text", nullable: true),
                    FirstSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AiSeverity = table.Column<string>(type: "text", nullable: true),
                    AiRootCause = table.Column<string>(type: "text", nullable: true),
                    AiSuggestedFix = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErrorGroups_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErrorGroups_SessionId",
                table: "ErrorGroups",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ParseSessions_SessionToken",
                table: "ParseSessions",
                column: "SessionToken");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UserId",
                table: "Sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ExternalSubscriptionId",
                table: "Subscriptions",
                column: "ExternalSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId_Provider",
                table: "Subscriptions",
                columns: new[] { "UserId", "Provider" });

            migrationBuilder.CreateIndex(
                name: "IX_UsageTracking_Identifier_Date",
                table: "UsageTracking",
                columns: new[] { "Identifier", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErrorGroups");

            migrationBuilder.DropTable(
                name: "ParseSessions");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "UsageTracking");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
