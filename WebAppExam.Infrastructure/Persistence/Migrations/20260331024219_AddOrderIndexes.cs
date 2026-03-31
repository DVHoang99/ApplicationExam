using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppExam.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
@"CREATE INDEX ""IX_Orders_CreatedAt"" 
          ON ""orders"" 
          (""CreatedAt"");"
);
            // Create GIN index for customerName (using unaccent for accent-insensitive search)
            migrationBuilder.Sql(
                @"CREATE INDEX ""IX_Orders_CustomerName_Trgm"" 
                  ON ""orders"" 
                  USING gin (f_unaccent(""CustomerName"") gin_trgm_ops);"
            );

            migrationBuilder.Sql(
                @"CREATE INDEX ""IX_Orders_PhoneNumber_Trgm"" 
                  ON ""orders"" 
                  USING gin (""PhoneNumber"" gin_trgm_ops);"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"DROP INDEX IF EXISTS ""IX_Orders_PhoneNumber_Trgm"";"
            );

            migrationBuilder.Sql(
                @"DROP INDEX IF EXISTS ""IX_Orders_CustomerName_Trgm"";"
            );

            migrationBuilder.Sql(
                @"DROP INDEX IF EXISTS ""IX_Orders_CreatedAt"";"
            );
        }
    }
}
