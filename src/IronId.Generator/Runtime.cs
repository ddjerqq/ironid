using System.Linq;
using System.Reflection;

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

/// <summary>
/// Marks a class as an IronId type with the specified prefix.
/// </summary>
/// <param name="prefix">Required prefix for the IronId type.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IronIdAttribute(string prefix) : Attribute
{
    /// <summary>
    /// Required prefix for the IronId type.
    /// </summary>
    public string Prefix { get; set; } = prefix;
}

/// <summary>
/// Helper methods for configuring IronId converters.
/// </summary>
public static class IronIdExtensions
{
    /// <summary>
    /// Parses the given string into the appropriate IronId type based on its prefix.
    /// </summary>
    public static IIronId Parse(string s)
    {
        var idTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => typeof(IIronId).IsAssignableFrom(x));
        
        foreach (var idType in idTypes)
        {
            var parseMethod = idType.GetMethod("Parse", [typeof(string)]);
            // IIronId for example, does not have the Parse method. so we quietly skip.
            // There is no way that an ID type will not have the Parse method generated,
            // so if it doesn't it's safe to skip.
            if (parseMethod is not null)
            {
                try
                {
                    var result = parseMethod.Invoke(null, [s]);
                    if (result is not null)
                        return (IIronId)result;
                }
                catch (TargetInvocationException)
                {
                    // Ignore and try next
                }
            }
        }

        throw new FormatException($"No matching IronId type found for value: {s}");
    }
}