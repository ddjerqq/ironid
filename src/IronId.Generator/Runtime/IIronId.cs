

// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// Marker interface that all generated Strong ID types implement.
/// Provides methods necessary for universal conversion in ORMs like Entity Framework Core.
/// </summary>
public interface IIronId
{
    /// <summary>
    /// Gets the string representation of this IronId.
    /// </summary>
    string ToString();

    /// <summary>
    /// Gets the underlying Ulid value.
    /// </summary>
    Ulid GetValue();
}