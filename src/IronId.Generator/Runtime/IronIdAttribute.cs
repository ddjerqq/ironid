// ReSharper disable once CheckNamespace
namespace System;

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