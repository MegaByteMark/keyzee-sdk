using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace keyzee_migrations_sqlserver.Migrations
{
    /// <inheritdoc />
    public partial class MinorUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KeyValuePairs_DeletedOn",
                table: "KeyValuePairs");

            migrationBuilder.DropIndex(
                name: "IX_Apps_DeletedOn",
                table: "Apps");

            migrationBuilder.CreateIndex(
                name: "IX_KeyValuePairs_DeletedOn",
                table: "KeyValuePairs",
                column: "DeletedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Apps_DeletedOn",
                table: "Apps",
                column: "DeletedOn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KeyValuePairs_DeletedOn",
                table: "KeyValuePairs");

            migrationBuilder.DropIndex(
                name: "IX_Apps_DeletedOn",
                table: "Apps");

            migrationBuilder.CreateIndex(
                name: "IX_KeyValuePairs_DeletedOn",
                table: "KeyValuePairs",
                column: "DeletedOn",
                filter: "DeletedOn IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Apps_DeletedOn",
                table: "Apps",
                column: "DeletedOn",
                filter: "DeletedOn IS NOT NULL");
        }
    }
}
