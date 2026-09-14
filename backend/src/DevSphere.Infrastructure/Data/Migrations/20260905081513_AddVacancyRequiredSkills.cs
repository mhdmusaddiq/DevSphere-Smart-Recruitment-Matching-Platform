using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevSphere.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVacancyRequiredSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VacancyId",
                table: "Skills",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skills_VacancyId",
                table: "Skills",
                column: "VacancyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_Vacancies_VacancyId",
                table: "Skills",
                column: "VacancyId",
                principalTable: "Vacancies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Vacancies_VacancyId",
                table: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_Skills_VacancyId",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "VacancyId",
                table: "Skills");
        }
    }
}
