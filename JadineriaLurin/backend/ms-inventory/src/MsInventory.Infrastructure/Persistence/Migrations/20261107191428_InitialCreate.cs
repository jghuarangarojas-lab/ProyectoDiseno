using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MsInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.CreateTable(
                name: "product_plants",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    common_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    stock_available = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    stock_minimum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_plants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventory_entries",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    supplier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    entry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_entries_product_plants_product_id",
                        column: x => x.product_id,
                        principalSchema: "inventory",
                        principalTable: "product_plants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "merma_records",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity_lost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    evidence_photo_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    manager_review_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merma_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_merma_records_product_plants_product_id",
                        column: x => x.product_id,
                        principalSchema: "inventory",
                        principalTable: "product_plants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_entries_entry_date",
                schema: "inventory",
                table: "inventory_entries",
                column: "entry_date");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_entries_product_id",
                schema: "inventory",
                table: "inventory_entries",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_entries_supplier",
                schema: "inventory",
                table: "inventory_entries",
                column: "supplier");

            migrationBuilder.CreateIndex(
                name: "ix_merma_records_created_at",
                schema: "inventory",
                table: "merma_records",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_merma_records_manager_review_status",
                schema: "inventory",
                table: "merma_records",
                column: "manager_review_status");

            migrationBuilder.CreateIndex(
                name: "ix_merma_records_product_id",
                schema: "inventory",
                table: "merma_records",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_plants_category",
                schema: "inventory",
                table: "product_plants",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_product_plants_common_name",
                schema: "inventory",
                table: "product_plants",
                column: "common_name");

            migrationBuilder.CreateIndex(
                name: "ix_product_plants_sku",
                schema: "inventory",
                table: "product_plants",
                column: "sku",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventory_entries",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "merma_records",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "product_plants",
                schema: "inventory");
        }
    }
}
