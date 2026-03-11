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
    /// Parses the given string into the appropriate IronId type based on its prefix.
    /// </summary>
    public static IIronId Parse(string s)
    {
        var idTypes = Assembly.GetCallingAssembly().GetTypes()
            .Where(x => typeof(IIronId).IsAssignableFrom(x));
        
        foreach (var idType in idTypes)
        {
            var parseMethod = idType.GetMethod("Parse", [typeof(string)]);
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