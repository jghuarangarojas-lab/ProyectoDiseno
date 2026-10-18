using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Iot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "device_alerts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    issue_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    is_resolved = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    resolved_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolution_note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device_alerts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "iot_devices",
                columns: table => new
                {
                    device_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    mac_address = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    current_sector_area = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    battery_percentage = table.Column<int>(type: "integer", nullable: false),
                    last_seen_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_irrigation_requested_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iot_devices", x => x.device_code);
                });

            migrationBuilder.CreateTable(
                name: "telemetry_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    recorded_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    humidity_percentage = table.Column<double>(type: "double precision", nullable: false),
                    temperature_celsius = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_telemetry_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "botanical_thresholds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    min_humidity_percentage = table.Column<int>(type: "integer", nullable: false),
                    max_humidity_percentage = table.Column<int>(type: "integer", nullable: false),
                    min_temperature_celsius = table.Column<double>(type: "double precision", nullable: false),
                    max_temperature_celsius = table.Column<double>(type: "double precision", nullable: false),
                    auto_irrigation_seconds = table.Column<int>(type: "integer", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_botanical_thresholds", x => x.id);
                    table.ForeignKey(
                        name: "FK_botanical_thresholds_iot_devices_device_code",
                        column: x => x.device_code,
                        principalTable: "iot_devices",
                        principalColumn: "device_code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_botanical_thresholds_device_code",
                table: "botanical_thresholds",
                column: "device_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_device_alerts_created_at_utc",
                table: "device_alerts",
                column: "created_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_device_alerts_device_code_is_resolved",
                table: "device_alerts",
                columns: new[] { "device_code", "is_resolved" });

            migrationBuilder.CreateIndex(
                name: "ix_iot_devices_current_sector_area",
                table: "iot_devices",
                column: "current_sector_area");

            migrationBuilder.CreateIndex(
                name: "ix_iot_devices_mac_address",
                table: "iot_devices",
                column: "mac_address");

            migrationBuilder.CreateIndex(
                name: "ix_iot_devices_status",
                table: "iot_devices",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_telemetry_records_device_code_recorded_at_utc",
                table: "telemetry_records",
                columns: new[] { "device_code", "recorded_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "botanical_thresholds");

            migrationBuilder.DropTable(
                name: "device_alerts");

            migrationBuilder.DropTable(
                name: "telemetry_records");

            migrationBuilder.DropTable(
                name: "iot_devices");
        }
    }
}
