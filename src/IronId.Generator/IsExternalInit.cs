// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

// This exists only so we can compile `record` and `record struct`
// in .NET Standard 2.0 projects, which don't define IsExternalInit.
internal static class IsExternalInit;