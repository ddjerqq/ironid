# IronId

IronId is a small Roslyn source-generator + runtime helpers that produces strongly-typed ID value types backed by ULIDs. 
It generates zero-boilerplate, safe, JSON- and EF Core-friendly ID types from a single attribute you place on your domain type.

A developer-friendly generator that creates compact, predictable, and type-safe ID wrappers (e.g. `UserId`, `OrderId`) 
with a fixed prefix and ULID payload. 

**Generated IDs provide parsing, formatting, JSON converters, and framework type 
converters so you can treat IDs as first-class, strongly-typed values across your application.**

what it does, example usage, and why you need it. show one attribute usage.

# Example attribute usage:

```csharp
using IronId.Generated;

[IronId("usr")]
public sealed record User(UserId Id);
```

# Usage 

1. Annotate a domain type with the attribute and a short prefix (the prefix must be provided as the positional string argument):

```csharp
using IronId.Generated;

[IronId("ord")]
public partial class Order { }
```

2. Build your project. The source generator emits a `OrderId` (a `readonly record struct`) in the same namespace with the API described below.

3. Use the generated ID type in code:

- Create: `var id = OrderId.New();`
- Empty sentinel: `OrderId.Empty` (wraps `Ulid.Empty`)
- String conversion: `string s = id;` (implicit)
- Parse: `var id = OrderId.Parse("ord_01...");` or `OrderId.TryParse(...)` for safe parsing
- Ulid conversion: `Ulid u = id;` (implicit) and `(OrderId)ulid` (explicit)
- JSON: `JsonSerializer.Serialize(id)` uses the generated `OrderIdJsonConverter` automatically.

4. Optional: register all generated JSON converters into `JsonSerializerOptions`:

```csharp
var options = new JsonSerializerOptions();
options.Converters.ConfigureIronIdConverters();
```

Generated type contract (summary):

- `public readonly record struct OrderId(Ulid Value)` implements `IIronId`, `IComparable`, `IParsable<OrderId>`.
- Constants: `public const string Prefix` and `public static readonly OrderId Empty`.
- Factories: `public static OrderId New()` and default constructor creating a new ULID.
- Formatting: `ToString()` returns `"{prefix}_{ulid}"` in lowercase.
- Parsing: `Parse` throws on invalid input; `TryParse` returns false for non-matching or malformed values.
- Converters: JSON and TypeConverter types are generated and applied via attributes.

# EFcore integration

```csharp
/// <summary>
/// Entity Framework Core value converter for all IronId types.
/// </summary>
/// <example>
/// Put this code in the EF Core DbContext to configure auto conversions for all your IronIds.
/// <code>
/// protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
/// {
///     base.ConfigureConventions(configurationBuilder);
///     
///     configurationBuilder.Properties&lt;IIronId&gt;()
///         .HaveConversion&lt;IronIdValueConverter&gt;();
/// }
/// </code>
/// </example>
public sealed class IronIdValueConverter() : global::Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<IIronId, string>(
    id => id.ToString(),
    s => IronIdExtensions.Parse(s));
```

# Why you would want strongly-typed IDs
Strings are easy to misuse. Passing raw string IDs around makes it trivial to mix up IDs (user vs order vs product),
forget prefixes, or accidentally accept malformed values.
Hand-coded strongly-typed ID wrappers help, but hand-rolling them has recurring problems:

- Boilerplate: Every ID type needs the same constructors, parsing, ToString, conversions and JSON converters.
- Inconsistency: Different implementations tend to diverge in behavior (case, serialization format, parsing rules).
- Error-prone: It's easy to forget to add a JSON converter or a TypeConverter for model binding, which causes silent bugs.
- Maintenance: Adding features (EF Core conversion, parsing rules, canonical casing) means touching many files.

IronId solves this by generating consistent, fully-featured ID types from a single attribute on your domain type. You get:

- Zero-boilerplate generated `XxxId` types wrapping `System.Ulid`.
- Standardized `ToString()` format: `"{prefix}_{ulid}"` (lowercase canonical form).
- `Parse` / `TryParse` and implicit/explicit conversions to/from `string` and `Ulid`.
- Per-type System.Text.Json converters and System.ComponentModel.TypeConverters for model binding.
- A helper `IIronId` interface and runtime helpers for registering converters and parsing by prefix.

## Caveats & potential improvements

- Cross-assembly discovery: `ConfigureIronIdConverters` and `IronIdExtensions.IdTypes` use `typeof(IIronId).Assembly` to find generated types. If you emit types in different assemblies (for example, shared types in a referenced assembly vs. consumer projects), they might not all be discovered by this single-assembly scan. Consider adding overloads that accept an Assembly[] or call this on each target assembly in the application startup.
- AOT / trimming / reflection: The implementation uses reflection and Activator.CreateInstance for converter registration, so trimming or AOT scenarios might need extra preservation attributes or manual registration.
- Comparability: The generated ID implements `IComparable` but not `IComparable<T>`; if callers rely on the strongly-typed generic comparator, implement it as well.
- Null & empty semantics: `Empty` is `Ulid.Empty` and round-trips as the `prefix_000...0` value. Make sure downstream code treats this as empty sentinel appropriately.
- Attribute naming and usage: The generator's syntactic predicate matches attribute by name `IronId`. If you accidentally define `IronIdAttribute` under a different namespace and don't import it, the syntactic pass may still match but the semantic check ensures the attribute resolves to `IronId.Generated.IronIdAttribute`.
