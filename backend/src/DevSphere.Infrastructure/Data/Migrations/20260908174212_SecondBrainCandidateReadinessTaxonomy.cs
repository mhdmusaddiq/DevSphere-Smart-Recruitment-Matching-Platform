using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace DevSphere.Infrastructure.Data.Migrations
{
    [DbContext(typeof(DevSphereDbContext))]
    [Migration("20260908174212_SecondBrainCandidateReadinessTaxonomy")]
    public partial class SecondBrainCandidateReadinessTaxonomy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredWorkMode",
                table: "CandidateProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreferredLocation",
                table: "CandidateProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "WillingToRelocate",
                table: "CandidateProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreferredEmploymentType",
                table: "CandidateProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AvailabilityStatus",
                table: "CandidateProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "AvailableFrom",
                table: "CandidateProfiles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoticePeriodDays",
                table: "CandidateProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SkillConcepts",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),
                    Name = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false),
                    NormalizedName = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false),
                    IsActive = table.Column<bool>(
                        type: "bit",
                        nullable: false),
                    CreatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),
                    UpdatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_SkillConcepts",
                        x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OccupationConcepts",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),
                    Name = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false),
                    NormalizedName = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false),
                    IsActive = table.Column<bool>(
                        type: "bit",
                        nullable: false),
                    CreatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),
                    UpdatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_OccupationConcepts",
                        x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LicenceRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),
                    CandidateProfileId = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),
                    Type = table.Column<string>(
                        type: "nvarchar(150)",
                        maxLength: 150,
                        nullable: false),
                    Class = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),
                    Issuer = table.Column<string>(
                        type: "nvarchar(250)",
                        maxLength: 250,
                        nullable: false),
                    Identifier = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false),
                    IssuedOn = table.Column<DateOnly>(
                        type: "date",
                        nullable: false),
                    ExpiresOn = table.Column<DateOnly>(
                        type: "date",
                        nullable: true),
                    Status = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false),
                    VerificationStatus = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false),
                    CreatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),
                    UpdatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_LicenceRegistrations",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_LicenceRegistrations_CandidateProfiles_CandidateProfileId",
                        column: x => x.CandidateProfileId,
                        principalTable: "CandidateProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkillAliases",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),
                    SkillConceptId = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),
                    Alias = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false),
                    NormalizedAlias = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false),
                    IsActive = table.Column<bool>(
                        type: "bit",
                        nullable: false),
                    CreatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),
                    UpdatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_SkillAliases",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_SkillAliases_SkillConcepts_SkillConceptId",
                        column: x => x.SkillConceptId,
                        principalTable: "SkillConcepts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "SkillConceptId",
                table: "Skills",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LicenceRegistrations_CandidateProfileId_Type_Identifier",
                table: "LicenceRegistrations",
                columns: new[]
                {
                    "CandidateProfileId",
                    "Type",
                    "Identifier"
                },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OccupationConcepts_NormalizedName",
                table: "OccupationConcepts",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkillAliases_NormalizedAlias",
                table: "SkillAliases",
                column: "NormalizedAlias",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkillAliases_SkillConceptId",
                table: "SkillAliases",
                column: "SkillConceptId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillConcepts_NormalizedName",
                table: "SkillConcepts",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skills_SkillConceptId",
                table: "Skills",
                column: "SkillConceptId");

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_SkillConcepts_SkillConceptId",
                table: "Skills",
                column: "SkillConceptId",
                principalTable: "SkillConcepts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_SkillConcepts_SkillConceptId",
                table: "Skills");

            migrationBuilder.DropTable(
                name: "LicenceRegistrations");

            migrationBuilder.DropTable(
                name: "OccupationConcepts");

            migrationBuilder.DropTable(
                name: "SkillAliases");

            migrationBuilder.DropTable(
                name: "SkillConcepts");

            migrationBuilder.DropIndex(
                name: "IX_Skills_SkillConceptId",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "SkillConceptId",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "AvailabilityStatus",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "AvailableFrom",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "NoticePeriodDays",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "PreferredEmploymentType",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "PreferredLocation",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "PreferredWorkMode",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "WillingToRelocate",
                table: "CandidateProfiles");
        }
    }
}
