using StoreX.Api.Shared;

namespace StoreX.Api.Modules.Organization.Entities;

public sealed class Branch : AuditableEntity, ISoftDeletable
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public bool IsActive { get; private set; } = true;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    private Branch() { }

    public Branch(string code, string name, string address, string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Branch code cannot be empty.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Branch name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Branch address cannot be empty.", nameof(address));

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Address = address.Trim();
        PhoneNumber = NormalizePhone(phoneNumber);

        ValidateLengths();
    }

    public void UpdateDetails(string name, string address, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Branch name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Branch address cannot be empty.", nameof(address));

        var normalizedName = name.Trim();
        var normalizedAddress = address.Trim();
        var normalizedPhoneNumber = NormalizePhone(phoneNumber);

        if (normalizedName.Length > 150)
            throw new ArgumentException("Branch name cannot exceed 150 characters.", nameof(name));

        if (normalizedAddress.Length > 300)
            throw new ArgumentException("Branch address cannot exceed 300 characters.", nameof(address));

        if (normalizedPhoneNumber?.Length > 30)
            throw new ArgumentException("Branch phone number cannot exceed 30 characters.", nameof(phoneNumber));

        Name = normalizedName;
        Address = normalizedAddress;
        PhoneNumber = normalizedPhoneNumber;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    private static string? NormalizePhone(string? phoneNumber)
    {
        return string.IsNullOrWhiteSpace(phoneNumber)
            ? null
            : phoneNumber.Trim();
    }

    private void ValidateLengths()
    {
        if (Code.Length > 20)
            throw new ArgumentException("Branch code cannot exceed 20 characters.");

        if (Name.Length > 150)
            throw new ArgumentException("Branch name cannot exceed 150 characters.");

        if (Address.Length > 300)
            throw new ArgumentException("Branch address cannot exceed 300 characters.");

        if (PhoneNumber?.Length > 30)
            throw new ArgumentException("Branch phone number cannot exceed 30 characters.");
    }
}
