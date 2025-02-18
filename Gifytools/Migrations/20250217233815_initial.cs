using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gifytools.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConversionRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoInputPath = table.Column<string>(type: "text", nullable: false),
                    GifOutputPath = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExecutedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SetFps = table.Column<bool>(type: "boolean", nullable: false),
                    Fps = table.Column<int>(type: "integer", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    SetStartTime = table.Column<bool>(type: "boolean", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    SetEndTime = table.Column<bool>(type: "boolean", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    SetSpeed = table.Column<bool>(type: "boolean", nullable: false),
                    SpeedMultiplier = table.Column<double>(type: "double precision", nullable: false),
                    SetCrop = table.Column<bool>(type: "boolean", nullable: false),
                    CropX = table.Column<int>(type: "integer", nullable: false),
                    CropY = table.Column<int>(type: "integer", nullable: false),
                    CropWidth = table.Column<int>(type: "integer", nullable: false),
                    CropHeight = table.Column<int>(type: "integer", nullable: false),
                    SetReverse = table.Column<bool>(type: "boolean", nullable: false),
                    SetWatermark = table.Column<bool>(type: "boolean", nullable: false),
                    WatermarkText = table.Column<string>(type: "text", nullable: true),
                    WatermarkFont = table.Column<string>(type: "text", nullable: true),
                    SetCompression = table.Column<bool>(type: "boolean", nullable: false),
                    CompressionLevel = table.Column<int>(type: "integer", nullable: false),
                    SetReduceFrames = table.Column<bool>(type: "boolean", nullable: false),
                    FrameSkipInterval = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversionRequests", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversionRequests");
        }
    }
}
