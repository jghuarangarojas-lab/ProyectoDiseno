using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace LurinSmart.Persistence;

public sealed class DatabaseFailoverMonitor : BackgroundService
{
    private readonly DatabaseFailoverState _state;
    private readonly DatabaseFailoverOptions _options;
    private readonly ILogger<DatabaseFailoverMonitor> _logger;

    public DatabaseFailoverMonitor(
        DatabaseFailoverState state,
        IOptions<DatabaseFailoverOptions> options,
        ILogger<DatabaseFailoverMonitor> logger)
    {
        _state = state;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(3, _options.HealthCheckIntervalSeconds));

        _logger.LogInformation(
            "Monitor de failover de BD iniciado (intervalo {Interval}s).",
            interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var primaryHealthy = await CanConnectAsync(_state.PrimaryConnection, stoppingToken);

                if (primaryHealthy)
                {
                    if (_state.IsUsingBackup)
                    {
                        _logger.LogWarning(
                            "La BD primaria volvió a responder, pero se mantiene activa la BD de respaldo " +
                            "para evitar divergencia de datos. Ejecute la promoción manual para volver a la primaria.");
                    }
                }
                else
                {
                    _logger.LogError("La BD primaria no responde. Verificando la BD de respaldo...");

                    var backupHealthy = await CanConnectAsync(_state.BackupConnection, stoppingToken);

                    if (backupHealthy)
                    {
                        if (_state.UseBackup())
                        {
                            _logger.LogWarning(
                                "Failover activado: las nuevas operaciones se dirigirán a la BD de respaldo.");
                        }
                        else
                        {
                            _logger.LogWarning(
                                "La BD primaria continúa caída; se mantiene el uso de la BD de respaldo.");
                        }
                    }
                    else
                    {
                        _logger.LogCritical(
                            "Ni la BD primaria ni la BD de respaldo responden. No es posible persistir datos.");
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en el monitor de failover de BD.");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task<bool> CanConnectAsync(string connectionString, CancellationToken cancellationToken)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Timeout = Math.Max(1, _options.ConnectTimeoutSeconds),
                CommandTimeout = Math.Max(1, _options.ConnectTimeoutSeconds)
            };

            await using var connection = new NpgsqlConnection(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand("SELECT 1", connection)
            {
                CommandTimeout = Math.Max(1, _options.ConnectTimeoutSeconds)
            };

            await command.ExecuteScalarAsync(cancellationToken);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }
}
