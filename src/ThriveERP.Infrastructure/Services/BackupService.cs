using System.IO;
using System.Security.Cryptography;
using ThriveERP.Application.Common.Interfaces;

namespace ThriveERP.Infrastructure.Services;

public class BackupService : IBackupService
{
    private readonly string _dbPath;

    public BackupService()
    {
        _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "thrive_erp.db");
    }

    public async Task BackupDatabaseAsync(string destinationPath, string? encryptionPassword = null, CancellationToken ct = default)
    {
        if (!File.Exists(_dbPath))
        {
            throw new FileNotFoundException("The database file does not exist.", _dbPath);
        }

        if (string.IsNullOrEmpty(encryptionPassword))
        {
            // Simple unencrypted copy
            await using var sourceStream = new FileStream(_dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            await using var destStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
            await sourceStream.CopyToAsync(destStream, ct);
        }
        else
        {
            // Encrypted backup (AES-256)
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using var deriveBytes = new Rfc2898DeriveBytes(encryptionPassword, salt, 100000, HashAlgorithmName.SHA256);
            byte[] key = deriveBytes.GetBytes(32);
            byte[] iv = deriveBytes.GetBytes(16);

            await using var destStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
            await destStream.WriteAsync(salt, 0, salt.Length, ct);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            await using var cryptoStream = new CryptoStream(destStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            await using var sourceStream = new FileStream(_dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            await sourceStream.CopyToAsync(cryptoStream, ct);
        }
    }

    public async Task RestoreDatabaseAsync(string backupPath, string? encryptionPassword = null, CancellationToken ct = default)
    {
        if (!File.Exists(backupPath))
        {
            throw new FileNotFoundException("The backup file does not exist.", backupPath);
        }

        if (string.IsNullOrEmpty(encryptionPassword))
        {
            // Simple unencrypted copy
            await using var sourceStream = new FileStream(backupPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await using var destStream = new FileStream(_dbPath, FileMode.Create, FileAccess.Write);
            await sourceStream.CopyToAsync(destStream, ct);
        }
        else
        {
            // Encrypted restore
            await using var sourceStream = new FileStream(backupPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            
            byte[] salt = new byte[16];
            int bytesRead = await sourceStream.ReadAsync(salt, 0, salt.Length, ct);
            if (bytesRead != salt.Length) throw new InvalidOperationException("Invalid backup file format.");

            using var deriveBytes = new Rfc2898DeriveBytes(encryptionPassword, salt, 100000, HashAlgorithmName.SHA256);
            byte[] key = deriveBytes.GetBytes(32);
            byte[] iv = deriveBytes.GetBytes(16);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            await using var cryptoStream = new CryptoStream(sourceStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            await using var destStream = new FileStream(_dbPath, FileMode.Create, FileAccess.Write);
            await cryptoStream.CopyToAsync(destStream, ct);
        }
    }
}
