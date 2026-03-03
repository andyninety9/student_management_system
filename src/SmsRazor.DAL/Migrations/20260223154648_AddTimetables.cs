using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmsRazor.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddTimetables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeacherAssignment",
                columns: table => new
                {
                    TeacherAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    teacherCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherAssignment", x => x.TeacherAssignmentId);
                    table.ForeignKey(
                        name: "FK_TeacherAssignment_TeacherInfo_teacherCode",
                        column: x => x.teacherCode,
                        principalTable: "TeacherInfo",
                        principalColumn: "TeacherCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Term",
                columns: table => new
                {
                    TermId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    isActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Term", x => x.TermId);
                });

            migrationBuilder.CreateTable(
                name: "Section",
                columns: table => new
                {
                    SectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    sectionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TeacherAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Section", x => x.SectionId);
                    table.ForeignKey(
                        name: "FK_Section_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Section_TeacherAssignment_TeacherAssignmentId",
                        column: x => x.TeacherAssignmentId,
                        principalTable: "TeacherAssignment",
                        principalColumn: "TeacherAssignmentId");
                });

            migrationBuilder.CreateTable(
                name: "AcademicCalendar",
                columns: table => new
                {
                    AcademicCalendarId = table.Column<Guid>(type: "uuid", nullable: false),
                    termID = table.Column<Guid>(type: "uuid", nullable: false),
                    sectionID = table.Column<Guid>(type: "uuid", nullable: false),
                    study_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    slot = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicCalendar", x => x.AcademicCalendarId);
                    table.ForeignKey(
                        name: "FK_AcademicCalendar_Section_sectionID",
                        column: x => x.sectionID,
                        principalTable: "Section",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AcademicCalendar_Term_termID",
                        column: x => x.termID,
                        principalTable: "Term",
                        principalColumn: "TermId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcademicCalendar_sectionID",
                table: "AcademicCalendar",
                column: "sectionID");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicCalendar_termID",
                table: "AcademicCalendar",
                column: "termID");

            migrationBuilder.CreateIndex(
                name: "IX_Section_CourseId",
                table: "Section",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Section_TeacherAssignmentId",
                table: "Section",
                column: "TeacherAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAssignment_teacherCode",
                table: "TeacherAssignment",
                column: "teacherCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcademicCalendar");

            migrationBuilder.DropTable(
                name: "Section");

            migrationBuilder.DropTable(
                name: "Term");

            migrationBuilder.DropTable(
                name: "TeacherAssignment");
        }
    }
}
