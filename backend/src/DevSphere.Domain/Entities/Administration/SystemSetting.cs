namespace DevSphere.Domain.Entities.Administration;

public class SystemSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsSensitive { get; set; }
}
