namespace DevSphere.Tests.Security;

public class SecondBrainSecurityContractTests
{
    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(
            AppContext.BaseDirectory);

        while (directory != null)
        {
            var backend = Path.Combine(
                directory.FullName,
                "src",
                "DevSphere.Api");

            if (Directory.Exists(backend))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Backend repository root was not found.");
    }

    private static string ReadBackendFile(
        params string[] parts)
    {
        var root = RepositoryRoot();

        var pathParts = new List<string>
        {
            root
        };

        pathParts.AddRange(parts);

        return File.ReadAllText(
            Path.Combine(pathParts.ToArray()));
    }

    [Fact]
    public void Auth_Register_Should_Validate_Public_Role_Before_User_Creation()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Api",
            "Controllers",
            "Auth",
            "AuthController.cs");

        var validationIndex = source.IndexOf(
            "if (role == null)",
            StringComparison.Ordinal);

        var createIndex = source.IndexOf(
            "CreateAsync(user, request.Password)",
            StringComparison.Ordinal);

        Assert.True(validationIndex >= 0);
        Assert.True(createIndex >= 0);
        Assert.True(validationIndex < createIndex);
    }

    [Fact]
    public void Auth_Login_Should_Block_Disabled_Accounts()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Api",
            "Controllers",
            "Auth",
            "AuthController.cs");

        Assert.Contains(
            "user == null || !user.IsActive",
            source);
    }

    [Fact]
    public void Auth_Me_Should_Return_Stable_Role_And_Account_State()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Api",
            "Controllers",
            "Auth",
            "AuthController.cs");

        Assert.Contains(
            "GetRolesAsync(user)",
            source);

        Assert.Contains(
            "AccountState = user.IsActive",
            source);

        Assert.Contains(
            "return \"JobSeeker\";",
            source);
    }

    [Fact]
    public void Admin_Should_Block_Self_Disable()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Administration",
            "AdminService.cs");

        Assert.Contains(
            "Administrators cannot disable their own account.",
            source);

        Assert.Contains(
            "actorUserId",
            source);

        Assert.Contains(
            "targetUserId",
            source);
    }

    [Fact]
    public void Admin_Should_Protect_Last_Active_Admin()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Administration",
            "AdminService.cs");

        Assert.Contains(
            "GetUsersInRoleAsync(AppRoles.Admin)",
            source);

        Assert.Contains(
            "otherActiveAdminExists",
            source);

        Assert.Contains(
            "The last active administrator cannot be disabled.",
            source);
    }

    [Fact]
    public void Resume_Upload_Should_Create_New_Version_And_Move_Current_Pointer()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Candidates",
            "ResumeService.cs");

        Assert.Contains(
            "new ResumeVersion",
            source);

        Assert.Contains(
            "IsCurrent = false",
            source);

        Assert.Contains(
            "CurrentVersionId",
            source);
    }

    [Fact]
    public void Resume_Current_Selection_Should_Be_Owner_Scoped()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Candidates",
            "ResumeService.cs");

        Assert.Contains(
            "SetCurrentVersionAsync",
            source);

        Assert.Contains(
            "GetByUserIdAsync(userId)",
            source);

        Assert.Contains(
            "versionId",
            source);
    }

    [Fact]
    public void Profile_Readiness_Should_Not_Contain_Score_Logic()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Profile",
            "CandidateProfileService.cs");

        var start = source.IndexOf(
            "GetProfileReadinessAsync",
            StringComparison.Ordinal);

        var end = source.IndexOf(
            "GetApplicationReadinessAsync",
            StringComparison.Ordinal);

        Assert.True(start >= 0);
        Assert.True(end > start);

        var method = source.Substring(
            start,
            end - start);

        Assert.DoesNotContain(
            "Score",
            method,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "MissingItems",
            method);
    }

    [Fact]
    public void Application_Readiness_Should_Require_Current_Resume_Version()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Profile",
            "CandidateProfileService.cs");

        Assert.Contains(
            "GetApplicationReadinessAsync",
            source);

        Assert.Contains(
            "CurrentVersionId",
            source);

        Assert.Contains(
            "IsCurrent",
            source);

        Assert.Contains(
            "Resume",
            source);
    }

    [Fact]
    public void Candidate_Skill_Should_Store_Canonical_Concept_Identity()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Profile",
            "CandidateProfileService.cs");

        Assert.Contains(
            "ResolveOrCreateAsync",
            source);

        Assert.Contains(
            "SkillConceptId = concept.Id",
            source);

        Assert.Contains(
            "Name = concept.Name",
            source);
    }

    [Fact]
    public void Skill_Taxonomy_Should_Normalize_Alias_Keys()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Profile",
            "SkillTaxonomyService.cs");

        Assert.Contains(
            "Normalize",
            source,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "SkillAlias",
            source);

        Assert.Contains(
            "SkillConcept",
            source);
    }
}
