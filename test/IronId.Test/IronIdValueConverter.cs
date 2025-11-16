using IronId.Generated;

namespace IronId.Test;

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