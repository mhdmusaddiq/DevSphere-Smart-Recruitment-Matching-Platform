using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevSphere.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddContactRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "VacancyId",
                table: "JobApplications",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "ContactRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CandidateId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_VacancyId",
                table: "JobApplications",
                column: "VacancyId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplications_Vacancies_VacancyId",
                table: "JobApplications",
                column: "VacancyId",
                principalTable: "Vacancies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobApplications_Vacancies_VacancyId",
                table: "JobApplications");

            migrationBuilder.DropTable(
                name: "ContactRequests");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_VacancyId",
                table: "JobApplications");

            migrationBuilder.AlterColumn<string>(
                name: "VacancyId",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
