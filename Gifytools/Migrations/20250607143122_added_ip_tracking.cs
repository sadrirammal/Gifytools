using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gifytools.Migrations
{
    /// <inheritdoc />
    public partial class added_ip_tracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "ConversionRequests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "ConversionRequests");
        }
    }
}
