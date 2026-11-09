using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MsServices.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "services");

            migrationBuilder.CreateTable(
                name: "service_projects",
                schema: "services",
                columns: table => new
                {
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    site_address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    stage = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    total_budget = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_projects", x => x.project_id);
                });

            migrationBuilder.CreateTable(
                name: "appointments",
                schema: "services",
                columns: table => new
                {
                    appointment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gardener_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scheduled_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    rescheduled_from_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rescheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_appointments", x => x.appointment_id);
                    table.ForeignKey(
                        name: "fk_appointments_service_projects_project_id",
                        column: x => x.project_id,
                        principalSchema: "services",
                        principalTable: "service_projects",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contract_documents",
                schema: "services",
                columns: table => new
                {
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    generated_pdf_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    digital_signature_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    signed_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    generated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contract_documents", x => x.contract_id);
                    table.ForeignKey(
                        name: "fk_contract_documents_service_projects_project_id",
                        column: x => x.project_id,
                        principalSchema: "services",
                        principalTable: "service_projects",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_installments",
                schema: "services",
                columns: table => new
                {
                    installment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_installments", x => x.installment_id);
                    table.ForeignKey(
                        name: "fk_payment_installments_service_projects_project_id",
                        column: x => x.project_id,
                        principalSchema: "services",
                        principalTable: "service_projects",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_attachments",
                schema: "services",
                columns: table => new
                {
                    attachment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    file_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    file_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    content_type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_attachments", x => x.attachment_id);
                    table.ForeignKey(
                        name: "fk_project_attachments_service_projects_project_id",
                        column: x => x.project_id,
                        principalSchema: "services",
                        principalTable: "service_projects",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_appointments_project_id",
                schema: "services",
                table: "appointments",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_appointments_scheduled_date",
                schema: "services",
                table: "appointments",
                column: "scheduled_date");

            migrationBuilder.CreateIndex(
                name: "ix_appointments_status",
                schema: "services",
                table: "appointments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_contract_documents_project_id",
                schema: "services",
                table: "contract_documents",
                column: "project_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_installments_due_date",
                schema: "services",
                table: "payment_installments",
                column: "due_date");

            migrationBuilder.CreateIndex(
                name: "ix_payment_installments_project_id",
                schema: "services",
                table: "payment_installments",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_installments_status",
                schema: "services",
                table: "payment_installments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_project_attachments_file_type",
                schema: "services",
                table: "project_attachments",
                column: "file_type");

            migrationBuilder.CreateIndex(
                name: "ix_project_attachments_project_id",
                schema: "services",
                table: "project_attachments",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_projects_client_id",
                schema: "services",
                table: "service_projects",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_projects_stage",
                schema: "services",
                table: "service_projects",
                column: "stage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appointments",
                schema: "services");

            migrationBuilder.DropTable(
                name: "contract_documents",
                schema: "services");

            migrationBuilder.DropTable(
                name: "payment_installments",
                schema: "services");

            migrationBuilder.DropTable(
                name: "project_attachments",
                schema: "services");

            migrationBuilder.DropTable(
                name: "service_projects",
                schema: "services");
        }
    }
}
