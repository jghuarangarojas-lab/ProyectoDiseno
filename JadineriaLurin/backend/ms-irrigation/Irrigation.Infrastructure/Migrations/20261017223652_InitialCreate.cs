using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Irrigation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "irrigation_actions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    sector_area = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    trigger_type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    requested_duration_seconds = table.Column<int>(type: "integer", nullable: false),
                    max_pump_duration_seconds = table.Column<int>(type: "integer", nullable: false),
                    actual_duration_seconds = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    cancellation_reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    started_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    finished_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_irrigation_actions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_irrigation_actions_created_at_utc",
                table: "irrigation_actions",
                column: "created_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_irrigation_actions_device_id",
                table: "irrigation_actions",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "ix_irrigation_actions_device_id_status",
                table: "irrigation_actions",
                columns: new[] { "device_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_irrigation_actions_status",
                table: "irrigation_actions",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "irrigation_actions");
        }
    }
}
