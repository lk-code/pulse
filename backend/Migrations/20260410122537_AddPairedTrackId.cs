using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPairedTrackId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PairedTrackId",
                table: "Tracks",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PairedTrackId",
                table: "Tracks");
        }
    }
}
