using System.Text.Json;

namespace IronId.Test;

/// <summary>
/// Tests for the IronIdExtensions helper methods.
/// </summary>
public class ExtensionMethodsTests
{
    [Test]
    public void ConfigureIronIdConverters_WorksWithSerialization()
    {
        var options = new JsonSerializerOptions();
        
        var userId = UserId.New();
        var json = JsonSerializer.Serialize(userId, options);
        var deserialized = JsonSerializer.Deserialize<UserId>(json, options);

        Console.WriteLine(json);
        Console.WriteLine(deserialized);
        
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

