using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmsRazor.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSyllabusAndCoursePrereq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Departments");

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "Syllabuses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Syllabuses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveFrom",
                table: "Syllabuses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveTo",
                table: "Syllabuses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Syllabuses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Syllabuses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SyllabusName",
                table: "Syllabuses",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DepartmentNameEng",
                table: "Departments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DepartmentNameVI",
                table: "Departments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Departments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TotalCredit",
                table: "Departments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CoursePrerequisites",
                columns: table => new
                {
                    CoursePrerequisiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrerequisiteCourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursePrerequisites", x => x.CoursePrerequisiteId);
                    table.ForeignKey(
                        name: "FK_CoursePrerequisites_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursePrerequisites_Courses_PrerequisiteCourseId",
                        column: x => x.PrerequisiteCourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Syllabuses_DepartmentId",
                table: "Syllabuses",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_SyllabusID",
                table: "Courses",
                column: "SyllabusID");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePrerequisites_CourseId",
                table: "CoursePrerequisites",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePrerequisites_PrerequisiteCourseId",
                table: "CoursePrerequisites",
                column: "PrerequisiteCourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Syllabuses_SyllabusID",
                table: "Courses",
                column: "SyllabusID",
                principalTable: "Syllabuses",
                principalColumn: "SyllabusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Syllabuses_Departments_DepartmentId",
                table: "Syllabuses",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Syllabuses_SyllabusID",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Syllabuses_Departments_DepartmentId",
                table: "Syllabuses");

            migrationBuilder.DropTable(
                name: "CoursePrerequisites");

            migrationBuilder.DropIndex(
                name: "IX_Syllabuses_DepartmentId",
                table: "Syllabuses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_SyllabusID",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "EffectiveFrom",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "EffectiveTo",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "SyllabusName",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "DepartmentNameEng",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DepartmentNameVI",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "TotalCredit",
                table: "Departments");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Syllabuses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Departments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
