using System.Text.Json;
using IronId.Generated;

namespace IronId.Test;

/// <summary>
/// Tests for the IronIdExtensions helper methods.
/// </summary>
public class ExtensionMethodsTests
{
    [Test]
    public void ConfigureIronIdConverters_AddsJsonConverters()
    {
        var options = new JsonSerializerOptions();
        var initialCount = options.Converters.Count;
        
        options.Converters.ConfigureIronIdConverters();
        
        // Should have added at least one converter (UserId)
        Assert.That(options.Converters, Has.Count.GreaterThan(initialCount));
    }

    [Test]
    public void ConfigureIronIdConverters_WorksWithSerialization()
    {
        var options = new JsonSerializerOptions();
        options.Converters.ConfigureIronIdConverters();
        
        var userId = UserId.New();
        var json = JsonSerializer.Serialize(userId, options);
        var deserialized = JsonSerializer.Deserialize<UserId>(json, options);
        
        Assert.That(deserialized, Is.EqualTo(userId));
    }

    [Test]
    public void IIronId_Interface_IsImplemented()
    {
        var userId = UserId.New();
        
        // UserId should implement IIronId
        Assert.That(userId, Is.InstanceOf<IIronId>());
    }

    [Test]
    public void IIronId_CanBeUsedPolymorphically()
    {
        IIronId ironId = UserId.New();
        
        Assert.That(ironId, Is.Not.Null);
        Assert.That(ironId.ToString(), Does.StartWith("usr_"));
    }

    [Test]
    public void MultipleIronIdTypes_CanBeStoredTogether()
    {
        // If we had multiple IronId types, they'd all implement IIronId
        var ids = new List<IIronId>
        {
            UserId.New(),
            UserId.New(),
            UserId.New(),
        };
        
        Assert.That(ids, Has.Count.EqualTo(3));
        Assert.That(ids.All(id => id is UserId), Is.True);
    }
}

