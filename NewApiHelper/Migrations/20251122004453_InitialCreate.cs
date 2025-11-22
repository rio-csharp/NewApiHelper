using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewApiHelper.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UpStreams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    Multiplier = table.Column<double>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpStreams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UpstreamGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    UpstreamId = table.Column<int>(type: "INTEGER", nullable: false),
                    UpstreamName = table.Column<string>(type: "TEXT", nullable: false),
                    UpstreamMultiplier = table.Column<double>(type: "REAL", nullable: false),
                    GroupMultiplier = table.Column<double>(type: "REAL", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpstreamGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UpstreamGroups_UpStreams_UpstreamId",
                        column: x => x.UpstreamId,
                        principalTable: "UpStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UpstreamGroups_UpstreamId",
                table: "UpstreamGroups",
                column: "UpstreamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UpstreamGroups");

            migrationBuilder.DropTable(
                name: "UpStreams");
        }
    }
}
