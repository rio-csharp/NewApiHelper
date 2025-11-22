using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewApiHelper.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpstreamMultiplier",
                table: "UpstreamGroups");

            migrationBuilder.DropColumn(
                name: "UpstreamName",
                table: "UpstreamGroups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "UpstreamMultiplier",
                table: "UpstreamGroups",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "UpstreamName",
                table: "UpstreamGroups",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
