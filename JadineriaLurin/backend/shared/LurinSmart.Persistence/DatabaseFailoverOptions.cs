namespace LurinSmart.Persistence;

public sealed class DatabaseFailoverOptions
{
    public const string SectionName = "DatabaseFailover";

    public string PrimaryConnection { get; set; } = string.Empty;
    public string BackupConnection { get; set; } = string.Empty;
    public int HealthCheckIntervalSeconds { get; set; } = 15;
    public int ConnectTimeoutSeconds { get; set; } = 3;
}
