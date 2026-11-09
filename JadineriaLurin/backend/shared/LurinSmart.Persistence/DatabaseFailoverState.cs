using Microsoft.Extensions.Options;

namespace LurinSmart.Persistence;

public sealed class DatabaseFailoverState
{
    private readonly object _sync = new();
    private string _activeConnection;

    public DatabaseFailoverState(IOptions<DatabaseFailoverOptions> options)
    {
        var value = options.Value;

        if (string.IsNullOrWhiteSpace(value.PrimaryConnection))
        {
            throw new InvalidOperationException(
                "DatabaseFailover:PrimaryConnection no está configurada.");
        }

        if (string.IsNullOrWhiteSpace(value.BackupConnection))
        {
            throw new InvalidOperationException(
                "DatabaseFailover:BackupConnection no está configurada.");
        }

        PrimaryConnection = value.PrimaryConnection;
        BackupConnection = value.BackupConnection;
        _activeConnection = PrimaryConnection;
    }

    public string PrimaryConnection { get; }

    public string BackupConnection { get; }

    public bool IsUsingBackup { get; private set; }

    public string ActiveConnection
    {
        get
        {
            lock (_sync)
            {
                return _activeConnection;
            }
        }
    }

    public bool UsePrimary()
    {
        lock (_sync)
        {
            var changed = IsUsingBackup;
            IsUsingBackup = false;
            _activeConnection = PrimaryConnection;
            return changed;
        }
    }

    public bool UseBackup()
    {
        lock (_sync)
        {
            var changed = !IsUsingBackup;
            IsUsingBackup = true;
            _activeConnection = BackupConnection;
            return changed;
        }
    }
}
