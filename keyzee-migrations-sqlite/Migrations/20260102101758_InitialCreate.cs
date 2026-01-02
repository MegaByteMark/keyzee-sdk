using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeyZee.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Apps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    LastUpdateOn = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LastUpdateBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DeletedOn = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KeyValuePairs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AppId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    EncryptedValue = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    LastUpdateOn = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LastUpdateBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DeletedOn = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyValuePairs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KeyValuePairs_Apps_AppId",
                        column: x => x.AppId,
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Apps_DeletedOn",
                table: "Apps",
                column: "DeletedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Apps_Name",
                table: "Apps",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KeyValuePairs_AppId_Key",
                table: "KeyValuePairs",
                columns: new[] { "AppId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KeyValuePairs_DeletedOn",
                table: "KeyValuePairs",
                column: "DeletedOn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeyValuePairs");

            migrationBuilder.DropTable(
                name: "Apps");
        }
    }
}
