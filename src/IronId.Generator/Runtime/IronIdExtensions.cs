using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// Helper methods for configuring IronId converters.
/// </summary>
public static class IronIdExtensions
{
    /// <summary>
    /// Gets all IronId types in the assembly.
    /// </summary>
    public static readonly IEnumerable<Type> IdTypes = Assembly.GetCallingAssembly().GetTypes().Where(x => typeof(IIronId).IsAssignableFrom(x));

    /// <summary>
    /// Parses the given string into the appropriate IronId type based on its prefix.
    /// </summary>
    public static IIronId Parse(string s)
    {
        foreach (var idType in IdTypes)
        {
            var parseMethod = idType.GetMethod("Parse", [typeof(string), typeof(IFormatProvider)]);
            if (parseMethod is not null)
            {
                try
                {
                    var result = parseMethod.Invoke(null, [s, null]);
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