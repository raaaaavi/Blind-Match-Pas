namespace BlindMatchPAS.Web.Authorization;

public static class RoleNames
{
    public const string Student = "Student";
    public const string Supervisor = "Supervisor";
    public const string ModuleLeader = "ModuleLeader";
    public const string SystemAdmin = "SystemAdmin";

    public static readonly string[] All = [Student, Supervisor, ModuleLeader, SystemAdmin];
    public const string AdminRoles = ModuleLeader + "," + SystemAdmin;
}
