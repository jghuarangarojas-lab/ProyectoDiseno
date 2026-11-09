using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MsDelivery.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "delivery");

            migrationBuilder.CreateTable(
                name: "drivers",
                schema: "delivery",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    vehicle_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    max_capacity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_drivers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "delivery_orders",
                schema: "delivery",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sale_id = table.Column<Guid>(type: "uuid", nullable: false),
                    address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    delivery_date = table.Column<DateOnly>(type: "date", nullable: false),
                    delivery_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    required_capacity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    proof_photo_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    digital_signature_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_delivery_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_delivery_orders_drivers_driver_id",
                        column: x => x.driver_id,
                        principalSchema: "delivery",
                        principalTable: "drivers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_delivery_orders_delivery_date",
                schema: "delivery",
                table: "delivery_orders",
                column: "delivery_date");

            migrationBuilder.CreateIndex(
                name: "ix_delivery_orders_driver_id",
                schema: "delivery",
                table: "delivery_orders",
                column: "driver_id");

            migrationBuilder.CreateIndex(
                name: "ix_delivery_orders_sale_id",
                schema: "delivery",
                table: "delivery_orders",
                column: "sale_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_delivery_orders_status",
                schema: "delivery",
                table: "delivery_orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_drivers_is_active",
                schema: "delivery",
                table: "drivers",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_drivers_name",
                schema: "delivery",
                table: "drivers",
                column: "name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "delivery_orders",
                schema: "delivery");

            migrationBuilder.DropTable(
                name: "drivers",
                schema: "delivery");
        }
    }
}
