namespace ThriveERP.Application.Common.Interfaces;

public interface IBackupService
{
    Task BackupDatabaseAsync(string destinationPath, string? encryptionPassword = null, CancellationToken ct = default);
    Task RestoreDatabaseAsync(string backupPath, string? encryptionPassword = null, CancellationToken ct = default);
}
