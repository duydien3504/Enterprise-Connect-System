using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseConnect.AdminService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCyberForensicsAuditLogAndSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "documents",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "file_size_bytes",
                table: "documents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "version",
                table: "documents",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "actor_email",
                table: "audit_logs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "actor_role",
                table: "audit_logs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "correlation_id",
                table: "audit_logs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "endpoint_url",
                table: "audit_logs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "execution_duration_ms",
                table: "audit_logs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "failure_reason",
                table: "audit_logs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "http_method",
                table: "audit_logs",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ip_address",
                table: "audit_logs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "risk_level",
                table: "audit_logs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "service_name",
                table: "audit_logs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "status_code",
                table: "audit_logs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "target_id",
                table: "audit_logs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "timestamp",
                table: "audit_logs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "user_agent",
                table: "audit_logs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "system_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    setting_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    setting_value = table.Column<string>(type: "text", nullable: false),
                    group_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_settings", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_correlation_id",
                table: "audit_logs",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_risk_level",
                table: "audit_logs",
                column: "risk_level");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_service_name",
                table: "audit_logs",
                column: "service_name");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_timestamp",
                table: "audit_logs",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_system_settings_setting_key",
                table: "system_settings",
                column: "setting_key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "system_settings");

            migrationBuilder.DropIndex(
                name: "ix_audit_logs_correlation_id",
                table: "audit_logs");

            migrationBuilder.DropIndex(
                name: "ix_audit_logs_risk_level",
                table: "audit_logs");

            migrationBuilder.DropIndex(
                name: "ix_audit_logs_service_name",
                table: "audit_logs");

            migrationBuilder.DropIndex(
                name: "ix_audit_logs_timestamp",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "description",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "file_size_bytes",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "version",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "actor_email",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "actor_role",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "correlation_id",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "endpoint_url",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "execution_duration_ms",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "failure_reason",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "http_method",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "ip_address",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "risk_level",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "service_name",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "status_code",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "target_id",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "timestamp",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "user_agent",
                table: "audit_logs");
        }
    }
}
