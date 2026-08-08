using System;

namespace ThriveERP.Application.Common.Interfaces
{
    public interface ILicenseService
    {
        bool IsLicenseValid(string licenseKey);
        (bool IsValid, DateTime? ExpirationDate, string? CustomerName, string? ErrorMessage) ValidateLicense(string licenseKey);
        int GetDaysRemaining(string licenseKey);
    }
}
