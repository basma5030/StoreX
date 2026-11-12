
namespace StoreX.Api.Shared;

public sealed record Error
{
    public string Code { get; }

    public string Description { get; }

    public static readonly Error None =
        new(string.Empty, string.Empty, allowEmpty: true);

    public Error(string code, string description)
        : this(code, description, allowEmpty: false)
    {
    }

    private Error(
        string code,
        string description,
        bool allowEmpty)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(description);

        if (!allowEmpty && string.IsNullOrWhiteSpace(code))
            throw new ArgumentException(
                "Error code cannot be empty.",
                nameof(code));

        if (!allowEmpty && string.IsNullOrWhiteSpace(description))
            throw new ArgumentException(
                "Error description cannot be empty.",
                nameof(description));

        Code = code;
        Description = description;
    }
}
