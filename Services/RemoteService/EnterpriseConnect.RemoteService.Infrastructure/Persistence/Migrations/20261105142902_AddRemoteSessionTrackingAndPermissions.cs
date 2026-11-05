using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseConnect.RemoteService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRemoteSessionTrackingAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "connected_at",
                table: "remote_sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "duration_seconds",
                table: "remote_sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "end_reason",
                table: "remote_sessions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ended_at",
                table: "remote_sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_single_use",
                table: "remote_sessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "granted_at",
                table: "access_grants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "revoked_at",
                table: "access_grants",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "connected_at",
                table: "remote_sessions");

            migrationBuilder.DropColumn(
                name: "duration_seconds",
                table: "remote_sessions");

            migrationBuilder.DropColumn(
                name: "end_reason",
                table: "remote_sessions");

            migrationBuilder.DropColumn(
                name: "ended_at",
                table: "remote_sessions");

            migrationBuilder.DropColumn(
                name: "is_single_use",
                table: "remote_sessions");

            migrationBuilder.DropColumn(
                name: "granted_at",
                table: "access_grants");

            migrationBuilder.DropColumn(
                name: "revoked_at",
                table: "access_grants");
        }
    }
}
