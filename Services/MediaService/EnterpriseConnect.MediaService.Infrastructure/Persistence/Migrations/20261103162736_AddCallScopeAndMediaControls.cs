using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseConnect.MediaService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCallScopeAndMediaControls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "connected_at",
                table: "call_sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "duration_seconds",
                table: "call_sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "end_reason",
                table: "call_sessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "live_kit_room_name",
                table: "call_sessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "scope",
                table: "call_sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "join_time",
                table: "call_participants",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<bool>(
                name: "is_camera_on",
                table: "call_participants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_muted",
                table: "call_participants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_screen_sharing",
                table: "call_participants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "call_participants",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "connected_at",
                table: "call_sessions");

            migrationBuilder.DropColumn(
                name: "duration_seconds",
                table: "call_sessions");

            migrationBuilder.DropColumn(
                name: "end_reason",
                table: "call_sessions");

            migrationBuilder.DropColumn(
                name: "live_kit_room_name",
                table: "call_sessions");

            migrationBuilder.DropColumn(
                name: "scope",
                table: "call_sessions");

            migrationBuilder.DropColumn(
                name: "is_camera_on",
                table: "call_participants");

            migrationBuilder.DropColumn(
                name: "is_muted",
                table: "call_participants");

            migrationBuilder.DropColumn(
                name: "is_screen_sharing",
                table: "call_participants");

            migrationBuilder.DropColumn(
                name: "status",
                table: "call_participants");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "join_time",
                table: "call_participants",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
