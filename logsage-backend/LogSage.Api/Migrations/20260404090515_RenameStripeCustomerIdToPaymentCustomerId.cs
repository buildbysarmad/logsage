using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogSage.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameStripeCustomerIdToPaymentCustomerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix incorrect Phase6 migration: PaymentProvider column contains old StripeCustomerId data
            // Copy it to PaymentCustomerId where it belongs, then clear PaymentProvider
            migrationBuilder.Sql(@"
                UPDATE ""Users""
                SET ""PaymentCustomerId"" = ""PaymentProvider""
                WHERE ""PaymentProvider"" IS NOT NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Users""
                SET ""PaymentProvider"" = NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse the fix: copy PaymentCustomerId back to PaymentProvider
            migrationBuilder.Sql(@"
                UPDATE ""Users""
                SET ""PaymentProvider"" = ""PaymentCustomerId""
                WHERE ""PaymentCustomerId"" IS NOT NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Users""
                SET ""PaymentCustomerId"" = NULL;
            ");
        }
    }
}
