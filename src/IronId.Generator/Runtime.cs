using System.Collections.Generic;
using System.ComponentModel;
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
    private static readonly List<Type> IronIdTypes = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(assembly =>
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Edge case: handle assemblies with missing dependencies safely
                return ex.Types.Where(t => t is not null);
            }
        })
        .Where(type => typeof(IIronId).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
        .ToList();

    /// <summary>
    /// Parses the given string into the appropriate IronId type based on its prefix.
    /// </summary>
    public static IIronId Parse(string s)
    {
        foreach (var idType in IronIdTypes)
        {
            if (TryGetParseMethod(s, idType) is not { } parseMethod)
                continue;

            try
            {
                var result = parseMethod.Invoke(null, [s]);
                if (result is not null)
                    return (IIronId)result;
            }
            catch (TargetInvocationException ex) when (ex.InnerException is FormatException)
            {
                // Ignore format exception and try next type matching the prefix criteria
            }
        }

        throw new FormatException($"No matching IronId type found for value: {s}");
    }

    private static MethodInfo? TryGetParseMethod(string s, Type idType)
    {
        // Optimization: check the generated 'Prefix' static field first before invoking Parse
        var prefixField = idType.GetField("Prefix", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        if (prefixField?.GetValue(null) is string prefix)
        {
            // If the string doesn't start with this ID type's prefix, skip it completely
            if (!s.StartsWith(prefix + "_", StringComparison.OrdinalIgnoreCase))
            {
                return idType.GetMethod("Parse", [typeof(string)]);
            }
        }

        return null;
    }
}