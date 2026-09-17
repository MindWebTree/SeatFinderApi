namespace CounsellingApp.Domain.Constants;

/// <summary>
/// Fixed, well-known GUIDs for the built-in roles, seeded by database/01_schema.sql.
/// Keeping these as constants avoids a lookup query every time the app needs to
/// assign or check a role.
/// </summary>
public static class RoleIds
{
    public static readonly Guid Admin = new("cb6c499a-ac3e-11f1-a93e-40b034f2b31c");
    public static readonly Guid Student = new("cb6c6402-ac3e-11f1-a93e-40b034f2b31c");
}
