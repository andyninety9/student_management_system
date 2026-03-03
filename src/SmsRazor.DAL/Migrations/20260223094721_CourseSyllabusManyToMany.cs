using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmsRazor.DAL.Migrations
{
    /// <inheritdoc />
    public partial class CourseSyllabusManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Syllabuses_SyllabusID",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_SyllabusID",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "SyllabusID",
                table: "Courses");

            migrationBuilder.CreateTable(
                name: "SyllabusCourses",
                columns: table => new
                {
                    SyllabusCourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    SyllabusId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyllabusCourses", x => x.SyllabusCourseId);
                    table.ForeignKey(
                        name: "FK_SyllabusCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SyllabusCourses_Syllabuses_SyllabusId",
                        column: x => x.SyllabusId,
                        principalTable: "Syllabuses",
                        principalColumn: "SyllabusId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SyllabusCourses_CourseId",
                table: "SyllabusCourses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_SyllabusCourses_SyllabusId",
                table: "SyllabusCourses",
                column: "SyllabusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyllabusCourses");

            migrationBuilder.AddColumn<Guid>(
                name: "SyllabusID",
                table: "Courses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_SyllabusID",
                table: "Courses",
                column: "SyllabusID");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Syllabuses_SyllabusID",
                table: "Courses",
                column: "SyllabusID",
                principalTable: "Syllabuses",
                principalColumn: "SyllabusId");
        }
    }
}
