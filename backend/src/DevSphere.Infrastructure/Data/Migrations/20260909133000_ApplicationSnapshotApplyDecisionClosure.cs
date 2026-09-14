using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace DevSphere.Infrastructure.Data.Migrations
{
    [DbContext(typeof(DevSphereDbContext))]
    [Migration("20260909133000_ApplicationSnapshotApplyDecisionClosure")]
    public partial class ApplicationSnapshotApplyDecisionClosure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationSnapshots_JobApplicationId",
                table: "ApplicationSnapshots");

            migrationBuilder.AddColumn<Guid>(
                name: "MatchingPolicyRevisionId",
                table: "ApplicationSnapshots",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CompatibilityScore",
                table: "ApplicationSnapshots",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RawCompatibilityScore",
                table: "ApplicationSnapshots",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DisplayCompatibilityScore",
                table: "ApplicationSnapshots",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CompatibilityStatus",
                table: "ApplicationSnapshots",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EligibilityStatus",
                table: "ApplicationSnapshots",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsEligible",
                table: "ApplicationSnapshots",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ApplyDecision",
                table: "ApplicationSnapshots",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MatchedSkillsJson",
                table: "ApplicationSnapshots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GapSkillsJson",
                table: "ApplicationSnapshots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EvidenceSummaryJson",
                table: "ApplicationSnapshots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationSnapshots_JobApplicationId",
                table: "ApplicationSnapshots",
                column: "JobApplicationId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationSnapshots_JobApplicationId",
                table: "ApplicationSnapshots");

            migrationBuilder.DropColumn(
                name: "MatchingPolicyRevisionId",
                table: "ApplicationSnapshots");

            migrationBuilder.DropColumn(
                name: "CompatibilityScore",
                table: "ApplicationSnapshots");

            migrationBuilder.DropColumn(
                name: "RawCompatibilityScore",
                table: "ApplicationSnapshots");

            migrationBuilder.DropColumn(
                name: "DisplayCompatibilityScore",
                table: "ApplicationSnapshots");

            migrationBuilder.DropColumn(
                name: "CompatibilityStatus",
                table: "ApplicationSnapshots");

            migrationBuilder.DropColumn(
                name: "EligibilityStatus",
                table: "ApplicationSnapshots");

            migrationBuilder.DropColumn(
                name: "IsEligible",
                table: "ApplicationSnapshots");

            migrationBuilder.DropColumn(
                name: "ApplyDecision",
                table: "ApplicationSnapshots");

            migrationBuilder.DropColumn(
                name: "MatchedSkillsJson",
                table: "ApplicationSnapshots");

            migrationBuilder.DropColumn(
                name: "GapSkillsJson",
                table: "ApplicationSnapshots");

            migrationBuilder.DropColumn(
                name: "EvidenceSummaryJson",
                table: "ApplicationSnapshots");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationSnapshots_JobApplicationId",
                table: "ApplicationSnapshots",
                column: "JobApplicationId");
        }
    }
}