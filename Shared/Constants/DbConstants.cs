namespace OC_System_Training.Shared.Constants;

/// <summary>
/// Database table names constants to prevent magic strings across repositories.
/// </summary>
public static class DbConstants
{
    public static class Tables
    {
        public const string Users = "Users";
        public const string Departments = "Departments";
        public const string Students = "Students";
        public const string AuditLogs = "AuditLogs";
    }
}
