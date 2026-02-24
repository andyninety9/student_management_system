using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmsRazor.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: table => new
                {
                    AttendanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AcademicCalendarId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendances", x => x.AttendanceId);
                    table.ForeignKey(
                        name: "FK_Attendances_AcademicCalendar_AcademicCalendarId",
                        column: x => x.AcademicCalendarId,
                        principalTable: "AcademicCalendar",
                        principalColumn: "AcademicCalendarId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendances_StudentInfo_StudentCode",
                        column: x => x.StudentCode,
                        principalTable: "StudentInfo",
                        principalColumn: "StudentCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_AcademicCalendarId",
                table: "Attendances",
                column: "AcademicCalendarId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_StudentCode_AcademicCalendarId",
                table: "Attendances",
                columns: new[] { "StudentCode", "AcademicCalendarId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendances");
        }
    }
}
