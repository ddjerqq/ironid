using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace IronIdGenerator;

/// <summary>
/// Represents the minimal info we need about a type that uses [IronId].
/// </summary>
internal readonly record struct EntityIronIdContext(string? Namespace, string TypeName, string Prefix)
{
    /// <summary>
    /// Creates a context from Roslyn's type symbol.
    /// </summary>
    public static EntityIronIdContext FromEntityTypeInfo(INamedTypeSymbol entityType, CancellationToken ct)
    {
        var ns = entityType.ContainingNamespace.IsGlobalNamespace
            ? null
            : entityType.ContainingNamespace.ToDisplayString();

        var prefix = entityType
                         .GetAttributes()
                         .Where(attribute => attribute.AttributeClass?.Name == "IronIdAttribute")
                         .Where(attribute => attribute.ConstructorArguments.Length > 0)
                         .Select(attribute => attribute.ConstructorArguments[0].Value?.ToString() ?? string.Empty)
                         .SingleOrDefault()
                     ?? throw new InvalidOperationException("Could not find prefix value from IronIdAttribute.");

        return new EntityIronIdContext(ns, entityType.Name, prefix);
    }
}

/// <summary>
/// Used to compare contexts so Roslyn doesn't process duplicates.
/// </summary>
internal sealed class PartialClassContextEqualityComparer : EqualityComparer<EntityIronIdContext>
{
    public static IEqualityComparer<EntityIronIdContext> Instance { get; } = new PartialClassContextEqualityComparer();

    public override bool Equals(EntityIronIdContext x, EntityIronIdContext y) =>
        x.Namespace == y.Namespace && x.TypeName == y.TypeName && x.Prefix == y.Prefix;

    public override int GetHashCode(EntityIronIdContext obj) =>
        HashCode.Combine(obj.Namespace, obj.TypeName, obj.Prefix);
}