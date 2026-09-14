IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903154717_IdentityFoundation'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903154717_IdentityFoundation'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [DisplayName] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903154717_IdentityFoundation'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903154717_IdentityFoundation'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903154717_IdentityFoundation'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903154717_IdentityFoundation'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903154717_IdentityFoundation'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903154717_IdentityFoundation'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903154717_IdentityFoundation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903154717_IdentityFoundation'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903154717_IdentityFoundation'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903154717_IdentityFoundation'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903154717_IdentityFoundation'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903154717_IdentityFoundation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903154717_IdentityFoundation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260903154717_IdentityFoundation', N'8.0.29');
END;
GO

COMMIT;
GO
BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903191435_DomainFoundation'
)
BEGIN
    CREATE TABLE [CandidateProfiles] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [Location] nvarchar(max) NOT NULL,
        [ExperienceMonths] int NOT NULL,
        [Education] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CandidateProfiles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903191435_DomainFoundation'
)
BEGIN
    CREATE TABLE [EmployerProfiles] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [CompanyName] nvarchar(max) NOT NULL,
        [ContactEmail] nvarchar(max) NOT NULL,
        [Website] nvarchar(max) NOT NULL,
        [Location] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_EmployerProfiles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903191435_DomainFoundation'
)
BEGIN
    CREATE TABLE [JobApplications] (
        [Id] uniqueidentifier NOT NULL,
        [CandidateId] nvarchar(max) NOT NULL,
        [VacancyId] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [AppliedAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_JobApplications] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903191435_DomainFoundation'
)
BEGIN
    CREATE TABLE [Vacancies] (
        [Id] uniqueidentifier NOT NULL,
        [EmployerId] nvarchar(max) NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Location] nvarchar(max) NOT NULL,
        [RequiredExperienceMonths] int NOT NULL,
        [IsOpen] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Vacancies] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903191435_DomainFoundation'
)
BEGIN
    CREATE TABLE [Skills] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [CandidateProfileId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Skills] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Skills_CandidateProfiles_CandidateProfileId] FOREIGN KEY ([CandidateProfileId]) REFERENCES [CandidateProfiles] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903191435_DomainFoundation'
)
BEGIN
    CREATE INDEX [IX_Skills_CandidateProfileId] ON [Skills] ([CandidateProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903191435_DomainFoundation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260903191435_DomainFoundation', N'8.0.29');
END;
GO

COMMIT;
GO
BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905081513_AddVacancyRequiredSkills'
)
BEGIN
    ALTER TABLE [Skills] ADD [VacancyId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905081513_AddVacancyRequiredSkills'
)
BEGIN
    CREATE INDEX [IX_Skills_VacancyId] ON [Skills] ([VacancyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905081513_AddVacancyRequiredSkills'
)
BEGIN
    ALTER TABLE [Skills] ADD CONSTRAINT [FK_Skills_Vacancies_VacancyId] FOREIGN KEY ([VacancyId]) REFERENCES [Vacancies] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905081513_AddVacancyRequiredSkills'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905081513_AddVacancyRequiredSkills', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905121313_AddNotifications'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [IsRead] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905121313_AddNotifications'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905121313_AddNotifications', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905132556_AddContactRequests'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobApplications]') AND [c].[name] = N'VacancyId');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [JobApplications] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [JobApplications] ALTER COLUMN [VacancyId] uniqueidentifier NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905132556_AddContactRequests'
)
BEGIN
    CREATE TABLE [ContactRequests] (
        [Id] uniqueidentifier NOT NULL,
        [EmployerId] nvarchar(max) NOT NULL,
        [CandidateId] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ContactRequests] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905132556_AddContactRequests'
)
BEGIN
    CREATE INDEX [IX_JobApplications_VacancyId] ON [JobApplications] ([VacancyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905132556_AddContactRequests'
)
BEGIN
    ALTER TABLE [JobApplications] ADD CONSTRAINT [FK_JobApplications_Vacancies_VacancyId] FOREIGN KEY ([VacancyId]) REFERENCES [Vacancies] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905132556_AddContactRequests'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905132556_AddContactRequests', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobApplications]') AND [c].[name] = N'CandidateId');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [JobApplications] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [JobApplications] ALTER COLUMN [CandidateId] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ContactRequests]') AND [c].[name] = N'Status');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [ContactRequests] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [ContactRequests] ALTER COLUMN [Status] nvarchar(50) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ContactRequests]') AND [c].[name] = N'EmployerId');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [ContactRequests] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [ContactRequests] ALTER COLUMN [EmployerId] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ContactRequests]') AND [c].[name] = N'CandidateId');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [ContactRequests] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [ContactRequests] ALTER COLUMN [CandidateId] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    ALTER TABLE [ContactRequests] ADD [JobApplicationId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE TABLE [ApplicationStatusHistories] (
        [Id] uniqueidentifier NOT NULL,
        [JobApplicationId] uniqueidentifier NOT NULL,
        [PreviousStatus] int NULL,
        [NewStatus] int NOT NULL,
        [ChangedByUserId] nvarchar(450) NOT NULL,
        [ChangedAtUtc] datetime2 NOT NULL,
        [Notes] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ApplicationStatusHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ApplicationStatusHistories_JobApplications_JobApplicationId] FOREIGN KEY ([JobApplicationId]) REFERENCES [JobApplications] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE TABLE [AuditEvents] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NULL,
        [Action] nvarchar(200) NOT NULL,
        [EntityName] nvarchar(200) NOT NULL,
        [EntityId] nvarchar(200) NOT NULL,
        [Details] nvarchar(max) NOT NULL,
        [OccurredAtUtc] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_AuditEvents] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE TABLE [CertificationRecords] (
        [Id] uniqueidentifier NOT NULL,
        [CandidateProfileId] uniqueidentifier NOT NULL,
        [Name] nvarchar(250) NOT NULL,
        [Issuer] nvarchar(250) NOT NULL,
        [IssuedOn] date NOT NULL,
        [ExpiresOn] date NULL,
        [CredentialUrl] nvarchar(1000) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CertificationRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CertificationRecords_CandidateProfiles_CandidateProfileId] FOREIGN KEY ([CandidateProfileId]) REFERENCES [CandidateProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE TABLE [CompanyVerifications] (
        [Id] uniqueidentifier NOT NULL,
        [EmployerProfileId] uniqueidentifier NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [EvidenceStorageKey] nvarchar(500) NOT NULL,
        [SubmittedAtUtc] datetime2 NOT NULL,
        [ReviewedAtUtc] datetime2 NULL,
        [ReviewedByUserId] nvarchar(max) NULL,
        [Notes] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CompanyVerifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CompanyVerifications_EmployerProfiles_EmployerProfileId] FOREIGN KEY ([EmployerProfileId]) REFERENCES [EmployerProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE TABLE [EducationRecords] (
        [Id] uniqueidentifier NOT NULL,
        [CandidateProfileId] uniqueidentifier NOT NULL,
        [Institution] nvarchar(250) NOT NULL,
        [Qualification] nvarchar(200) NOT NULL,
        [FieldOfStudy] nvarchar(200) NOT NULL,
        [StartDate] date NOT NULL,
        [EndDate] date NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_EducationRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EducationRecords_CandidateProfiles_CandidateProfileId] FOREIGN KEY ([CandidateProfileId]) REFERENCES [CandidateProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE TABLE [LanguageCapabilities] (
        [Id] uniqueidentifier NOT NULL,
        [CandidateProfileId] uniqueidentifier NOT NULL,
        [Language] nvarchar(100) NOT NULL,
        [Proficiency] nvarchar(100) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_LanguageCapabilities] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LanguageCapabilities_CandidateProfiles_CandidateProfileId] FOREIGN KEY ([CandidateProfileId]) REFERENCES [CandidateProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE TABLE [MatchResults] (
        [Id] uniqueidentifier NOT NULL,
        [CandidateProfileId] uniqueidentifier NOT NULL,
        [VacancyId] uniqueidentifier NOT NULL,
        [MatchedSkills] nvarchar(max) NOT NULL,
        [MissingSkills] nvarchar(max) NOT NULL,
        [FinalScore] float NOT NULL,
        [CalculatedAtUtc] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_MatchResults] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MatchResults_CandidateProfiles_CandidateProfileId] FOREIGN KEY ([CandidateProfileId]) REFERENCES [CandidateProfiles] ([Id]),
        CONSTRAINT [FK_MatchResults_Vacancies_VacancyId] FOREIGN KEY ([VacancyId]) REFERENCES [Vacancies] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE TABLE [ProjectRecords] (
        [Id] uniqueidentifier NOT NULL,
        [CandidateProfileId] uniqueidentifier NOT NULL,
        [Name] nvarchar(250) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [ProjectUrl] nvarchar(1000) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ProjectRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProjectRecords_CandidateProfiles_CandidateProfileId] FOREIGN KEY ([CandidateProfileId]) REFERENCES [CandidateProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE TABLE [RequiredSkills] (
        [Id] uniqueidentifier NOT NULL,
        [VacancyId] uniqueidentifier NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [MinimumExperienceMonths] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_RequiredSkills] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RequiredSkills_Vacancies_VacancyId] FOREIGN KEY ([VacancyId]) REFERENCES [Vacancies] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE TABLE [Resumes] (
        [Id] uniqueidentifier NOT NULL,
        [CandidateProfileId] uniqueidentifier NOT NULL,
        [CurrentVersionId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Resumes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Resumes_CandidateProfiles_CandidateProfileId] FOREIGN KEY ([CandidateProfileId]) REFERENCES [CandidateProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE TABLE [SystemSettings] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(200) NOT NULL,
        [Value] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [IsSensitive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE TABLE [VacancyRequirements] (
        [Id] uniqueidentifier NOT NULL,
        [VacancyId] uniqueidentifier NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [IsMandatory] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_VacancyRequirements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VacancyRequirements_Vacancies_VacancyId] FOREIGN KEY ([VacancyId]) REFERENCES [Vacancies] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE TABLE [WorkExperiences] (
        [Id] uniqueidentifier NOT NULL,
        [CandidateProfileId] uniqueidentifier NOT NULL,
        [JobTitle] nvarchar(200) NOT NULL,
        [CompanyName] nvarchar(200) NOT NULL,
        [StartDate] date NOT NULL,
        [EndDate] date NULL,
        [Description] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_WorkExperiences] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkExperiences_CandidateProfiles_CandidateProfileId] FOREIGN KEY ([CandidateProfileId]) REFERENCES [CandidateProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE TABLE [CriterionResults] (
        [Id] uniqueidentifier NOT NULL,
        [MatchResultId] uniqueidentifier NOT NULL,
        [CriterionName] nvarchar(100) NOT NULL,
        [WeightPercent] float NOT NULL,
        [Score] float NOT NULL,
        [Details] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CriterionResults] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CriterionResults_MatchResults_MatchResultId] FOREIGN KEY ([MatchResultId]) REFERENCES [MatchResults] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE TABLE [ResumeVersions] (
        [Id] uniqueidentifier NOT NULL,
        [ResumeId] uniqueidentifier NOT NULL,
        [VersionNumber] int NOT NULL,
        [OriginalFileName] nvarchar(260) NOT NULL,
        [StorageKey] nvarchar(500) NOT NULL,
        [ContentType] nvarchar(200) NOT NULL,
        [FileSizeBytes] bigint NOT NULL,
        [IsCurrent] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ResumeVersions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ResumeVersions_Resumes_ResumeId] FOREIGN KEY ([ResumeId]) REFERENCES [Resumes] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE TABLE [ApplicationSnapshots] (
        [Id] uniqueidentifier NOT NULL,
        [JobApplicationId] uniqueidentifier NOT NULL,
        [ResumeVersionId] uniqueidentifier NULL,
        [CandidateSnapshotJson] nvarchar(max) NOT NULL,
        [VacancySnapshotJson] nvarchar(max) NOT NULL,
        [CapturedAtUtc] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ApplicationSnapshots] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ApplicationSnapshots_JobApplications_JobApplicationId] FOREIGN KEY ([JobApplicationId]) REFERENCES [JobApplications] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ApplicationSnapshots_ResumeVersions_ResumeVersionId] FOREIGN KEY ([ResumeVersionId]) REFERENCES [ResumeVersions] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE UNIQUE INDEX [IX_JobApplications_CandidateId_VacancyId] ON [JobApplications] ([CandidateId], [VacancyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE INDEX [IX_ContactRequests_CandidateId] ON [ContactRequests] ([CandidateId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE INDEX [IX_ContactRequests_EmployerId] ON [ContactRequests] ([EmployerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ContactRequests_JobApplicationId_EmployerId] ON [ContactRequests] ([JobApplicationId], [EmployerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE INDEX [IX_ApplicationSnapshots_JobApplicationId] ON [ApplicationSnapshots] ([JobApplicationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE INDEX [IX_ApplicationSnapshots_ResumeVersionId] ON [ApplicationSnapshots] ([ResumeVersionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE INDEX [IX_ApplicationStatusHistories_JobApplicationId] ON [ApplicationStatusHistories] ([JobApplicationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE INDEX [IX_CertificationRecords_CandidateProfileId] ON [CertificationRecords] ([CandidateProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE INDEX [IX_CompanyVerifications_EmployerProfileId] ON [CompanyVerifications] ([EmployerProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE INDEX [IX_CriterionResults_MatchResultId] ON [CriterionResults] ([MatchResultId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE INDEX [IX_EducationRecords_CandidateProfileId] ON [EducationRecords] ([CandidateProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE INDEX [IX_LanguageCapabilities_CandidateProfileId] ON [LanguageCapabilities] ([CandidateProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE INDEX [IX_MatchResults_CandidateProfileId] ON [MatchResults] ([CandidateProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE INDEX [IX_MatchResults_VacancyId] ON [MatchResults] ([VacancyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE INDEX [IX_ProjectRecords_CandidateProfileId] ON [ProjectRecords] ([CandidateProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE INDEX [IX_RequiredSkills_VacancyId] ON [RequiredSkills] ([VacancyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE INDEX [IX_Resumes_CandidateProfileId] ON [Resumes] ([CandidateProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ResumeVersions_ResumeId_VersionNumber] ON [ResumeVersions] ([ResumeId], [VersionNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SystemSettings_Key] ON [SystemSettings] ([Key]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE INDEX [IX_VacancyRequirements_VacancyId] ON [VacancyRequirements] ([VacancyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    CREATE INDEX [IX_WorkExperiences_CandidateProfileId] ON [WorkExperiences] ([CandidateProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    ALTER TABLE [ContactRequests] ADD CONSTRAINT [FK_ContactRequests_JobApplications_JobApplicationId] FOREIGN KEY ([JobApplicationId]) REFERENCES [JobApplications] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907204150_P0CoreAlignment'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907204150_P0CoreAlignment', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    ALTER TABLE [CandidateProfiles] ADD [PreferredWorkMode] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    ALTER TABLE [CandidateProfiles] ADD [PreferredLocation] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    ALTER TABLE [CandidateProfiles] ADD [WillingToRelocate] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    ALTER TABLE [CandidateProfiles] ADD [PreferredEmploymentType] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    ALTER TABLE [CandidateProfiles] ADD [AvailabilityStatus] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    ALTER TABLE [CandidateProfiles] ADD [AvailableFrom] date NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    ALTER TABLE [CandidateProfiles] ADD [NoticePeriodDays] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    CREATE TABLE [SkillConcepts] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [NormalizedName] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SkillConcepts] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    CREATE TABLE [OccupationConcepts] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [NormalizedName] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_OccupationConcepts] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    CREATE TABLE [LicenceRegistrations] (
        [Id] uniqueidentifier NOT NULL,
        [CandidateProfileId] uniqueidentifier NOT NULL,
        [Type] nvarchar(150) NOT NULL,
        [Class] nvarchar(100) NOT NULL,
        [Issuer] nvarchar(250) NOT NULL,
        [Identifier] nvarchar(200) NOT NULL,
        [IssuedOn] date NOT NULL,
        [ExpiresOn] date NULL,
        [Status] nvarchar(50) NOT NULL,
        [VerificationStatus] nvarchar(50) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_LicenceRegistrations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LicenceRegistrations_CandidateProfiles_CandidateProfileId] FOREIGN KEY ([CandidateProfileId]) REFERENCES [CandidateProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    CREATE TABLE [SkillAliases] (
        [Id] uniqueidentifier NOT NULL,
        [SkillConceptId] uniqueidentifier NOT NULL,
        [Alias] nvarchar(200) NOT NULL,
        [NormalizedAlias] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SkillAliases] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SkillAliases_SkillConcepts_SkillConceptId] FOREIGN KEY ([SkillConceptId]) REFERENCES [SkillConcepts] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    ALTER TABLE [Skills] ADD [SkillConceptId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_LicenceRegistrations_CandidateProfileId_Type_Identifier] ON [LicenceRegistrations] ([CandidateProfileId], [Type], [Identifier]) WHERE [CandidateProfileId] IS NOT NULL AND [Type] IS NOT NULL AND [Identifier] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_OccupationConcepts_NormalizedName] ON [OccupationConcepts] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_SkillAliases_NormalizedAlias] ON [SkillAliases] ([NormalizedAlias]) WHERE [NormalizedAlias] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    CREATE INDEX [IX_SkillAliases_SkillConceptId] ON [SkillAliases] ([SkillConceptId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_SkillConcepts_NormalizedName] ON [SkillConcepts] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    CREATE INDEX [IX_Skills_SkillConceptId] ON [Skills] ([SkillConceptId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    ALTER TABLE [Skills] ADD CONSTRAINT [FK_Skills_SkillConcepts_SkillConceptId] FOREIGN KEY ([SkillConceptId]) REFERENCES [SkillConcepts] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908174212_SecondBrainCandidateReadinessTaxonomy'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260908174212_SecondBrainCandidateReadinessTaxonomy', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909133000_ApplicationSnapshotApplyDecisionClosure'
)
BEGIN
    DROP INDEX [IX_ApplicationSnapshots_JobApplicationId] ON [ApplicationSnapshots];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909133000_ApplicationSnapshotApplyDecisionClosure'
)
BEGIN
    ALTER TABLE [ApplicationSnapshots] ADD [MatchingPolicyRevisionId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909133000_ApplicationSnapshotApplyDecisionClosure'
)
BEGIN
    ALTER TABLE [ApplicationSnapshots] ADD [CompatibilityScore] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909133000_ApplicationSnapshotApplyDecisionClosure'
)
BEGIN
    ALTER TABLE [ApplicationSnapshots] ADD [RawCompatibilityScore] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909133000_ApplicationSnapshotApplyDecisionClosure'
)
BEGIN
    ALTER TABLE [ApplicationSnapshots] ADD [DisplayCompatibilityScore] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909133000_ApplicationSnapshotApplyDecisionClosure'
)
BEGIN
    ALTER TABLE [ApplicationSnapshots] ADD [CompatibilityStatus] nvarchar(50) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909133000_ApplicationSnapshotApplyDecisionClosure'
)
BEGIN
    ALTER TABLE [ApplicationSnapshots] ADD [EligibilityStatus] nvarchar(50) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909133000_ApplicationSnapshotApplyDecisionClosure'
)
BEGIN
    ALTER TABLE [ApplicationSnapshots] ADD [IsEligible] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909133000_ApplicationSnapshotApplyDecisionClosure'
)
BEGIN
    ALTER TABLE [ApplicationSnapshots] ADD [ApplyDecision] nvarchar(100) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909133000_ApplicationSnapshotApplyDecisionClosure'
)
BEGIN
    ALTER TABLE [ApplicationSnapshots] ADD [MatchedSkillsJson] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909133000_ApplicationSnapshotApplyDecisionClosure'
)
BEGIN
    ALTER TABLE [ApplicationSnapshots] ADD [GapSkillsJson] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909133000_ApplicationSnapshotApplyDecisionClosure'
)
BEGIN
    ALTER TABLE [ApplicationSnapshots] ADD [EvidenceSummaryJson] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909133000_ApplicationSnapshotApplyDecisionClosure'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ApplicationSnapshots_JobApplicationId] ON [ApplicationSnapshots] ([JobApplicationId]) WHERE [JobApplicationId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909133000_ApplicationSnapshotApplyDecisionClosure'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260909133000_ApplicationSnapshotApplyDecisionClosure', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD [AcceptedValuesJson] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD [AlternativeSetId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD [CanonicalTargetKey] nvarchar(250) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD [DisplayOrder] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD [ExpectedAnswer] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD [FamilyPolicyId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD [Importance] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD [IsRegulatoryGate] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD [IsScored] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD [MatchingPolicyRevisionId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD [Mode] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD [QuestionText] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD [RequiredMonths] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD [RequiredValue] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD [RequirementFamily] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD [RequiresVerification] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD [SkillConceptId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Vacancies]') AND [c].[name] = N'Title');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Vacancies] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Vacancies] ALTER COLUMN [Title] nvarchar(250) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Vacancies]') AND [c].[name] = N'Location');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Vacancies] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Vacancies] ALTER COLUMN [Location] nvarchar(250) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [Vacancies] ADD [ClosingDateUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [Vacancies] ADD [CompanyId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [Vacancies] ADD [EmploymentType] nvarchar(50) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [Vacancies] ADD [LifecycleStatus] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [Vacancies] ADD [MaxExperienceMonths] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [Vacancies] ADD [MinExperienceMonths] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [Vacancies] ADD [PublishedAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [Vacancies] ADD [RequiredEducation] nvarchar(250) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [Vacancies] ADD [SalaryMax] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [Vacancies] ADD [SalaryMin] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [Vacancies] ADD [WorkMode] nvarchar(50) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [RequiredSkills] ADD [Weight] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [CompanyVerifications] ADD [CompanyId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ApplicationSnapshots]') AND [c].[name] = N'ApplyDecision');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [ApplicationSnapshots] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [ApplicationSnapshots] ALTER COLUMN [ApplyDecision] nvarchar(max) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ApplicationSnapshots]') AND [c].[name] = N'CompatibilityScore');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [ApplicationSnapshots] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [ApplicationSnapshots] ALTER COLUMN [CompatibilityScore] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [ApplicationSnapshots] ADD [Coverage] decimal(18,4) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ApplicationSnapshots]') AND [c].[name] = N'DisplayCompatibilityScore');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [ApplicationSnapshots] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [ApplicationSnapshots] ALTER COLUMN [DisplayCompatibilityScore] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [ApplicationSnapshots] ADD [EligibilityReason] nvarchar(250) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [ApplicationSnapshots] ADD [HighTierAggregateScore] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [ApplicationSnapshots] ADD [MatchResultJson] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [ApplicationSnapshots] ADD [MediumTierAggregateScore] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ApplicationSnapshots]') AND [c].[name] = N'RawCompatibilityScore');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [ApplicationSnapshots] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [ApplicationSnapshots] ALTER COLUMN [RawCompatibilityScore] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE TABLE [CompanyProfiles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(250) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Website] nvarchar(1000) NULL,
        [Location] nvarchar(250) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CompanyProfiles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE TABLE [Interviews] (
        [Id] uniqueidentifier NOT NULL,
        [JobApplicationId] uniqueidentifier NOT NULL,
        [EmployerUserId] nvarchar(450) NOT NULL,
        [Status] int NOT NULL,
        [Notes] nvarchar(2000) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Interviews] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Interviews_JobApplications_JobApplicationId] FOREIGN KEY ([JobApplicationId]) REFERENCES [JobApplications] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE TABLE [MatchingPolicyRevisions] (
        [Id] uniqueidentifier NOT NULL,
        [VacancyId] uniqueidentifier NOT NULL,
        [RevisionNumber] int NOT NULL,
        [IsCurrent] bit NOT NULL,
        [IsMateriallyLocked] bit NOT NULL,
        [MateriallyLockedAtUtc] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_MatchingPolicyRevisions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MatchingPolicyRevisions_Vacancies_VacancyId] FOREIGN KEY ([VacancyId]) REFERENCES [Vacancies] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE TABLE [Offers] (
        [Id] uniqueidentifier NOT NULL,
        [JobApplicationId] uniqueidentifier NOT NULL,
        [EmployerUserId] nvarchar(450) NOT NULL,
        [Status] int NOT NULL,
        [OfferedSalary] decimal(18,2) NULL,
        [ExpiresAtUtc] datetime2 NULL,
        [ExtendedAtUtc] datetime2 NULL,
        [Notes] nvarchar(4000) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Offers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Offers_JobApplications_JobApplicationId] FOREIGN KEY ([JobApplicationId]) REFERENCES [JobApplications] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE TABLE [TalentPoolEntries] (
        [Id] uniqueidentifier NOT NULL,
        [JobApplicationId] uniqueidentifier NOT NULL,
        [CandidateUserId] nvarchar(450) NOT NULL,
        [EmployerUserId] nvarchar(450) NOT NULL,
        [HasCandidateConsent] bit NOT NULL,
        [ConsentRecordedAtUtc] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [Notes] nvarchar(2000) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_TalentPoolEntries] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TalentPoolEntries_JobApplications_JobApplicationId] FOREIGN KEY ([JobApplicationId]) REFERENCES [JobApplications] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE TABLE [CompanyMemberships] (
        [Id] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [EmployerUserId] nvarchar(450) NOT NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CompanyMemberships] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CompanyMemberships_CompanyProfiles_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [CompanyProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE TABLE [InterviewSlots] (
        [Id] uniqueidentifier NOT NULL,
        [InterviewId] uniqueidentifier NOT NULL,
        [StartsAtUtc] datetime2 NOT NULL,
        [EndsAtUtc] datetime2 NOT NULL,
        [LocationOrMeetingUrl] nvarchar(1000) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_InterviewSlots] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InterviewSlots_Interviews_InterviewId] FOREIGN KEY ([InterviewId]) REFERENCES [Interviews] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE TABLE [Scorecards] (
        [Id] uniqueidentifier NOT NULL,
        [JobApplicationId] uniqueidentifier NOT NULL,
        [InterviewId] uniqueidentifier NULL,
        [AssessorEmployerUserId] nvarchar(450) NOT NULL,
        [OverallRating] int NOT NULL,
        [Notes] nvarchar(4000) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Scorecards] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Scorecards_Interviews_InterviewId] FOREIGN KEY ([InterviewId]) REFERENCES [Interviews] ([Id]),
        CONSTRAINT [FK_Scorecards_JobApplications_JobApplicationId] FOREIGN KEY ([JobApplicationId]) REFERENCES [JobApplications] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE TABLE [FamilyPolicies] (
        [Id] uniqueidentifier NOT NULL,
        [MatchingPolicyRevisionId] uniqueidentifier NOT NULL,
        [RequirementFamily] int NOT NULL,
        [FamilyImportance] int NOT NULL,
        [IsActive] bit NOT NULL,
        [IsScored] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_FamilyPolicies] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FamilyPolicies_MatchingPolicyRevisions_MatchingPolicyRevisionId] FOREIGN KEY ([MatchingPolicyRevisionId]) REFERENCES [MatchingPolicyRevisions] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE TABLE [AlternativeSets] (
        [Id] uniqueidentifier NOT NULL,
        [MatchingPolicyRevisionId] uniqueidentifier NOT NULL,
        [FamilyPolicyId] uniqueidentifier NOT NULL,
        [SetType] int NOT NULL,
        [MinimumSatisfiedCount] int NULL,
        [Mode] int NOT NULL,
        [Importance] int NOT NULL,
        [IsActive] bit NOT NULL,
        [IsScored] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_AlternativeSets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AlternativeSets_FamilyPolicies_FamilyPolicyId] FOREIGN KEY ([FamilyPolicyId]) REFERENCES [FamilyPolicies] ([Id]),
        CONSTRAINT [FK_AlternativeSets_MatchingPolicyRevisions_MatchingPolicyRevisionId] FOREIGN KEY ([MatchingPolicyRevisionId]) REFERENCES [MatchingPolicyRevisions] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE INDEX [IX_VacancyRequirements_AlternativeSetId] ON [VacancyRequirements] ([AlternativeSetId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE INDEX [IX_VacancyRequirements_FamilyPolicyId] ON [VacancyRequirements] ([FamilyPolicyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE INDEX [IX_VacancyRequirements_MatchingPolicyRevisionId] ON [VacancyRequirements] ([MatchingPolicyRevisionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE INDEX [IX_Vacancies_CompanyId] ON [Vacancies] ([CompanyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE INDEX [IX_CompanyVerifications_CompanyId] ON [CompanyVerifications] ([CompanyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE INDEX [IX_AlternativeSets_FamilyPolicyId] ON [AlternativeSets] ([FamilyPolicyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE INDEX [IX_AlternativeSets_MatchingPolicyRevisionId] ON [AlternativeSets] ([MatchingPolicyRevisionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CompanyMemberships_CompanyId_EmployerUserId] ON [CompanyMemberships] ([CompanyId], [EmployerUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FamilyPolicies_MatchingPolicyRevisionId_RequirementFamily] ON [FamilyPolicies] ([MatchingPolicyRevisionId], [RequirementFamily]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE INDEX [IX_Interviews_JobApplicationId] ON [Interviews] ([JobApplicationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE INDEX [IX_InterviewSlots_InterviewId] ON [InterviewSlots] ([InterviewId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MatchingPolicyRevisions_VacancyId_RevisionNumber] ON [MatchingPolicyRevisions] ([VacancyId], [RevisionNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE INDEX [IX_Offers_JobApplicationId] ON [Offers] ([JobApplicationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE INDEX [IX_Scorecards_InterviewId] ON [Scorecards] ([InterviewId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE INDEX [IX_Scorecards_JobApplicationId] ON [Scorecards] ([JobApplicationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TalentPoolEntries_JobApplicationId_EmployerUserId] ON [TalentPoolEntries] ([JobApplicationId], [EmployerUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [CompanyVerifications] ADD CONSTRAINT [FK_CompanyVerifications_CompanyProfiles_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [CompanyProfiles] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [Vacancies] ADD CONSTRAINT [FK_Vacancies_CompanyProfiles_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [CompanyProfiles] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD CONSTRAINT [FK_VacancyRequirements_AlternativeSets_AlternativeSetId] FOREIGN KEY ([AlternativeSetId]) REFERENCES [AlternativeSets] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD CONSTRAINT [FK_VacancyRequirements_FamilyPolicies_FamilyPolicyId] FOREIGN KEY ([FamilyPolicyId]) REFERENCES [FamilyPolicies] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    ALTER TABLE [VacancyRequirements] ADD CONSTRAINT [FK_VacancyRequirements_MatchingPolicyRevisions_MatchingPolicyRevisionId] FOREIGN KEY ([MatchingPolicyRevisionId]) REFERENCES [MatchingPolicyRevisions] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909213539_BackendFinalClosure'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260909213539_BackendFinalClosure', N'8.0.29');
END;
GO

COMMIT;
GO

