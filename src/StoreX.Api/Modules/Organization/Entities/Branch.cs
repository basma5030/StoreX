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

    // Required by Entity Framework Core.
    private Branch()
    {
    }

    public Branch(
        string code,
        string name,
        string address,
        string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException(
                "Branch code cannot be empty.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Branch name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException(
                "Branch address cannot be empty.", nameof(address));

        Code = code.Trim();
        Name = name.Trim();
        Address = address.Trim();
        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber)
            ? null
            : phoneNumber.Trim();
    }
}