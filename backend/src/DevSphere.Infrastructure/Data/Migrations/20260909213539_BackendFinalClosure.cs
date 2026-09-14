using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevSphere.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackendFinalClosure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AcceptedValuesJson",
                table: "VacancyRequirements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AlternativeSetId",
                table: "VacancyRequirements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CanonicalTargetKey",
                table: "VacancyRequirements",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "VacancyRequirements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ExpectedAnswer",
                table: "VacancyRequirements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FamilyPolicyId",
                table: "VacancyRequirements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Importance",
                table: "VacancyRequirements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "VacancyRequirements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRegulatoryGate",
                table: "VacancyRequirements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsScored",
                table: "VacancyRequirements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "MatchingPolicyRevisionId",
                table: "VacancyRequirements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mode",
                table: "VacancyRequirements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "QuestionText",
                table: "VacancyRequirements",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequiredMonths",
                table: "VacancyRequirements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredValue",
                table: "VacancyRequirements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequirementFamily",
                table: "VacancyRequirements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresVerification",
                table: "VacancyRequirements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "SkillConceptId",
                table: "VacancyRequirements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Vacancies",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Vacancies",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosingDateUtc",
                table: "Vacancies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "Vacancies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmploymentType",
                table: "Vacancies",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LifecycleStatus",
                table: "Vacancies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxExperienceMonths",
                table: "Vacancies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinExperienceMonths",
                table: "Vacancies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAtUtc",
                table: "Vacancies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredEducation",
                table: "Vacancies",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryMax",
                table: "Vacancies",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryMin",
                table: "Vacancies",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkMode",
                table: "Vacancies",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Weight",
                table: "RequiredSkills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "CompanyVerifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApplyDecision",
                table: "ApplicationSnapshots",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<decimal>(
                name: "CompatibilityScore",
                table: "ApplicationSnapshots",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<decimal>(
                name: "Coverage",
                table: "ApplicationSnapshots",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "DisplayCompatibilityScore",
                table: "ApplicationSnapshots",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "EligibilityReason",
                table: "ApplicationSnapshots",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HighTierAggregateScore",
                table: "ApplicationSnapshots",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatchResultJson",
                table: "ApplicationSnapshots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MediumTierAggregateScore",
                table: "ApplicationSnapshots",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "RawCompatibilityScore",
                table: "ApplicationSnapshots",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.CreateTable(
                name: "CompanyProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Interviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interviews_JobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchingPolicyRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VacancyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    IsMateriallyLocked = table.Column<bool>(type: "bit", nullable: false),
                    MateriallyLockedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchingPolicyRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchingPolicyRevisions_Vacancies_VacancyId",
                        column: x => x.VacancyId,
                        principalTable: "Vacancies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Offers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OfferedSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtendedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Offers_JobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TalentPoolEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    EmployerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    HasCandidateConsent = table.Column<bool>(type: "bit", nullable: false),
                    ConsentRecordedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalentPoolEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalentPoolEntries_JobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyMemberships_CompanyProfiles_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InterviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LocationOrMeetingUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewSlots_Interviews_InterviewId",
                        column: x => x.InterviewId,
                        principalTable: "Interviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scorecards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InterviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssessorEmployerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    OverallRating = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scorecards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scorecards_Interviews_InterviewId",
                        column: x => x.InterviewId,
                        principalTable: "Interviews",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Scorecards_JobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchingPolicyRevisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequirementFamily = table.Column<int>(type: "int", nullable: false),
                    FamilyImportance = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsScored = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyPolicies_MatchingPolicyRevisions_MatchingPolicyRevisionId",
                        column: x => x.MatchingPolicyRevisionId,
                        principalTable: "MatchingPolicyRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlternativeSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchingPolicyRevisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyPolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SetType = table.Column<int>(type: "int", nullable: false),
                    MinimumSatisfiedCount = table.Column<int>(type: "int", nullable: true),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    Importance = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsScored = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlternativeSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlternativeSets_FamilyPolicies_FamilyPolicyId",
                        column: x => x.FamilyPolicyId,
                        principalTable: "FamilyPolicies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlternativeSets_MatchingPolicyRevisions_MatchingPolicyRevisionId",
                        column: x => x.MatchingPolicyRevisionId,
                        principalTable: "MatchingPolicyRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VacancyRequirements_AlternativeSetId",
                table: "VacancyRequirements",
                column: "AlternativeSetId");

            migrationBuilder.CreateIndex(
                name: "IX_VacancyRequirements_FamilyPolicyId",
                table: "VacancyRequirements",
                column: "FamilyPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_VacancyRequirements_MatchingPolicyRevisionId",
                table: "VacancyRequirements",
                column: "MatchingPolicyRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacancies_CompanyId",
                table: "Vacancies",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyVerifications_CompanyId",
                table: "CompanyVerifications",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AlternativeSets_FamilyPolicyId",
                table: "AlternativeSets",
                column: "FamilyPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_AlternativeSets_MatchingPolicyRevisionId",
                table: "AlternativeSets",
                column: "MatchingPolicyRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyMemberships_CompanyId_EmployerUserId",
                table: "CompanyMemberships",
                columns: new[] { "CompanyId", "EmployerUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FamilyPolicies_MatchingPolicyRevisionId_RequirementFamily",
                table: "FamilyPolicies",
                columns: new[] { "MatchingPolicyRevisionId", "RequirementFamily" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_JobApplicationId",
                table: "Interviews",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSlots_InterviewId",
                table: "InterviewSlots",
                column: "InterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchingPolicyRevisions_VacancyId_RevisionNumber",
                table: "MatchingPolicyRevisions",
                columns: new[] { "VacancyId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Offers_JobApplicationId",
                table: "Offers",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Scorecards_InterviewId",
                table: "Scorecards",
                column: "InterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_Scorecards_JobApplicationId",
                table: "Scorecards",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_TalentPoolEntries_JobApplicationId_EmployerUserId",
                table: "TalentPoolEntries",
                columns: new[] { "JobApplicationId", "EmployerUserId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyVerifications_CompanyProfiles_CompanyId",
                table: "CompanyVerifications",
                column: "CompanyId",
                principalTable: "CompanyProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vacancies_CompanyProfiles_CompanyId",
                table: "Vacancies",
                column: "CompanyId",
                principalTable: "CompanyProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VacancyRequirements_AlternativeSets_AlternativeSetId",
                table: "VacancyRequirements",
                column: "AlternativeSetId",
                principalTable: "AlternativeSets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VacancyRequirements_FamilyPolicies_FamilyPolicyId",
                table: "VacancyRequirements",
                column: "FamilyPolicyId",
                principalTable: "FamilyPolicies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VacancyRequirements_MatchingPolicyRevisions_MatchingPolicyRevisionId",
                table: "VacancyRequirements",
                column: "MatchingPolicyRevisionId",
                principalTable: "MatchingPolicyRevisions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyVerifications_CompanyProfiles_CompanyId",
                table: "CompanyVerifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Vacancies_CompanyProfiles_CompanyId",
                table: "Vacancies");

            migrationBuilder.DropForeignKey(
                name: "FK_VacancyRequirements_AlternativeSets_AlternativeSetId",
                table: "VacancyRequirements");

            migrationBuilder.DropForeignKey(
                name: "FK_VacancyRequirements_FamilyPolicies_FamilyPolicyId",
                table: "VacancyRequirements");

            migrationBuilder.DropForeignKey(
                name: "FK_VacancyRequirements_MatchingPolicyRevisions_MatchingPolicyRevisionId",
                table: "VacancyRequirements");

            migrationBuilder.DropTable(
                name: "AlternativeSets");

            migrationBuilder.DropTable(
                name: "CompanyMemberships");

            migrationBuilder.DropTable(
                name: "InterviewSlots");

            migrationBuilder.DropTable(
                name: "Offers");

            migrationBuilder.DropTable(
                name: "Scorecards");

            migrationBuilder.DropTable(
                name: "TalentPoolEntries");

            migrationBuilder.DropTable(
                name: "FamilyPolicies");

            migrationBuilder.DropTable(
                name: "CompanyProfiles");

            migrationBuilder.DropTable(
                name: "Interviews");

            migrationBuilder.DropTable(
                name: "MatchingPolicyRevisions");

            migrationBuilder.DropIndex(
                name: "IX_VacancyRequirements_AlternativeSetId",
                table: "VacancyRequirements");

            migrationBuilder.DropIndex(
                name: "IX_VacancyRequirements_FamilyPolicyId",
                table: "VacancyRequirements");

            migrationBuilder.DropIndex(
                name: "IX_VacancyRequirements_MatchingPolicyRevisionId",
                table: "VacancyRequirements");

            migrationBuilder.DropIndex(
                name: "IX_Vacancies_CompanyId",
                table: "Vacancies");

            migrationBuilder.DropIndex(
                name: "IX_CompanyVerifications_CompanyId",
                table: "CompanyVerifications");

            migrationBuilder.DropColumn(
                name: "AcceptedValuesJson",
                table: "VacancyRequirements");

            migrationBuilder.DropColumn(
                name: "AlternativeSetId",
                table: "VacancyRequirements");

            migrationBuilder.DropColumn(
                name: "CanonicalTargetKey",
                table: "VacancyRequirements");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "VacancyRequirements");

            migrationBuilder.DropColumn(
                name: "ExpectedAnswer",
                table: "VacancyRequirements");

            migrationBuilder.DropColumn(
                name: "FamilyPolicyId",
                table: "VacancyRequirements");

            migrationBuilder.DropColumn(
                name: "Importance",
                table: "VacancyRequirements");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "VacancyRequirements");

            migrationBuilder.DropColumn(
                name: "IsRegulatoryGate",
                table: "VacancyRequirements");

            migrationBuilder.DropColumn(
                name: "IsScored",
                table: "VacancyRequirements");

            migrationBuilder.DropColumn(
                name: "MatchingPolicyRevisionId",
                table: "VacancyRequirements");

            migrationBuilder.DropColumn(
                name: "Mode",
                table: "VacancyRequirements");

            migrationBuilder.DropColumn(
                name: "QuestionText",
                table: "VacancyRequirements");

            migrationBuilder.DropColumn(
                name: "RequiredMonths",
                table: "VacancyRequirements");

            migrationBuilder.DropColumn(
                name: "RequiredValue",
                table: "VacancyRequirements");

            migrationBuilder.DropColumn(
                name: "RequirementFamily",
                table: "VacancyRequirements");

            migrationBuilder.DropColumn(
                name: "RequiresVerification",
                table: "VacancyRequirements");

            migrationBuilder.DropColumn(
                name: "SkillConceptId",
                table: "VacancyRequirements");

            migrationBuilder.DropColumn(
                name: "ClosingDateUtc",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "EmploymentType",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "LifecycleStatus",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "MaxExperienceMonths",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "MinExperienceMonths",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "PublishedAtUtc",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "RequiredEducation",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "SalaryMax",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "SalaryMin",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "WorkMode",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "RequiredSkills");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "CompanyVerifications");

            migrationBuilder.DropColumn(
                name: "Coverage",
                table: "ApplicationSnapshots");

            migrationBuilder.DropColumn(
                name: "EligibilityReason",
                table: "ApplicationSnapshots");

            migrationBuilder.DropColumn(
                name: "HighTierAggregateScore",
                table: "ApplicationSnapshots");

            migrationBuilder.DropColumn(
                name: "MatchResultJson",
                table: "ApplicationSnapshots");

            migrationBuilder.DropColumn(
                name: "MediumTierAggregateScore",
                table: "ApplicationSnapshots");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Vacancies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Vacancies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<decimal>(
                name: "CompatibilityScore",
                table: "ApplicationSnapshots",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApplyDecision",
                table: "ApplicationSnapshots",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DisplayCompatibilityScore",
                table: "ApplicationSnapshots",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "RawCompatibilityScore",
                table: "ApplicationSnapshots",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);
        }
    }
}
