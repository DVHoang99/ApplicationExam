using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppExam.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrigramIndexForCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Enable necessary PostgreSQL extensions (if not already enabled)
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");

            migrationBuilder.Sql(@"
        CREATE OR REPLACE FUNCTION f_unaccent(text)
        RETURNS text AS
        $func$
        SELECT public.unaccent('public.unaccent', $1)
        $func$ LANGUAGE sql IMMUTABLE;
    ");

            // 2. Create GIN index for customerName (using unaccent for accent-insensitive search)
            // Note: Replace "Customers" and "CustomerName" with your actual table and column names
            migrationBuilder.Sql(
                @"CREATE INDEX ""IX_Customers_CustomerName_Trgm"" 
                  ON ""customers"" 
                  USING gin (f_unaccent(""CustomerName"") gin_trgm_ops);"
            );

            // 3. Create GIN index for phoneNumber 
            // (Use GIN if you need to search substrings like '%123%'. If you only search by prefix like '090%', a default B-Tree is sufficient)
            migrationBuilder.Sql(
                @"CREATE INDEX ""IX_Customers_PhoneNumber_Trgm"" 
                  ON ""customers"" 
                  USING gin (""PhoneNumber"" gin_trgm_ops);"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes when rolling back
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Customers_PhoneNumber_Trgm"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Customers_CustomerName_Trgm"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS f_unaccent(text);");
        }
    }
}
