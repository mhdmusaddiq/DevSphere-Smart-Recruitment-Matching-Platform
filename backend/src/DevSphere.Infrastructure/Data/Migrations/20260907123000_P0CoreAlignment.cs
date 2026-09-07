using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevSphere.Infrastructure.Data.Migrations
{
    public partial class P0CoreAlignment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CandidateId",
                table: "JobApplications",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "JobApplicationId",
                table: "ContactRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AlterColumn<string>(
                name: "EmployerId",
                table: "ContactRequests",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CandidateId",
                table: "ContactRequests",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ContactRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_CandidateId_VacancyId",
                table: "JobApplications",
                columns: new[] { "CandidateId", "VacancyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequests_CandidateId",
                table: "ContactRequests",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequests_EmployerId",
                table: "ContactRequests",
                column: "EmployerId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequests_JobApplicationId_EmployerId",
                table: "ContactRequests",
                columns: new[] { "JobApplicationId", "EmployerId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactRequests_JobApplications_JobApplicationId",
                table: "ContactRequests",
                column: "JobApplicationId",
                principalTable: "JobApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactRequests_JobApplications_JobApplicationId",
                table: "ContactRequests");

            migrationBuilder.DropIndex(
                name: "IX_ContactRequests_CandidateId",
                table: "ContactRequests");

            migrationBuilder.DropIndex(
                name: "IX_ContactRequests_EmployerId",
                table: "ContactRequests");

            migrationBuilder.DropIndex(
                name: "IX_ContactRequests_JobApplicationId_EmployerId",
                table: "ContactRequests");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_CandidateId_VacancyId",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "JobApplicationId",
                table: "ContactRequests");

            migrationBuilder.AlterColumn<string>(
                name: "CandidateId",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "EmployerId",
                table: "ContactRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "CandidateId",
                table: "ContactRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ContactRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}