using IronId.Generated;

namespace IronId.Test;

/// <summary>
/// Represents a user in the system.
/// </summary>
[IronId("usr")]
public sealed record User(UserId Id)
{
    public User() : this(UserId.New())
    {
    }
}