using System.Text.Json;

namespace IronId.Test;

/// <summary>
/// Tests for System.Text.Json serialization/deserialization of IronId types.
/// </summary>
public class JsonSerializationTests
{
    [Test]
    public void UserId_Serialize_ProducesStringValue()
    {
        var userId = UserId.New();
        var json = JsonSerializer.Serialize(userId);
        
        Assert.That(json, Does.StartWith("\"usr_"));
        Assert.That(json, Does.EndWith("\""));
    }

    [Test]
    public void UserId_Deserialize_FromValidString_ReturnsCorrectId()
    {
        var userId = UserId.New();
        var json = JsonSerializer.Serialize(userId);
        
        var deserialized = JsonSerializer.Deserialize<UserId>(json);
        
        Assert.That(deserialized, Is.EqualTo(userId));
    }

    [Test]
    public void UserId_RoundTrip_PreservesValue()
    {
        var original = UserId.New();
        
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<UserId>(json);
        
        Assert.That(deserialized, Is.EqualTo(original));
        Assert.That(deserialized.ToString(), Is.EqualTo(original.ToString()));
    }

    [Test]
    public void UserId_Deserialize_InvalidFormat_ThrowsJsonException()
    {
        var invalidJson = "\"invalid_format\"";
        
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<UserId>(invalidJson));
    }

    [Test]
    public void UserId_Deserialize_WrongPrefix_ThrowsJsonException()
    {
        var wrongPrefixJson = "\"pst_01k9axfv3hw9ffa2850ks02njz\"";
        
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<UserId>(wrongPrefixJson));
    }

    [Test]
    public void UserId_Deserialize_Null_ThrowsJsonException()
    {
        var nullJson = "null";
        
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<UserId>(nullJson));
    }

    [Test]
    public void UserId_SerializeInObject_Works()
    {
        var user = new User();
        var json = JsonSerializer.Serialize(user);
        
        Assert.That(json, Does.Contain("usr_"));
    }

    [Test]
    public void UserId_DeserializeInObject_Works()
    {
        var user = new User();
        var json = JsonSerializer.Serialize(user);
        
        var deserialized = JsonSerializer.Deserialize<User>(json);
        
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.Id, Is.EqualTo(user.Id));
    }

    [Test]
    public void UserId_SerializeToJson_IsLowercase()
    {
        var userId = UserId.New();
        var json = JsonSerializer.Serialize(userId);
        
        // Remove quotes
        var jsonValue = json.Trim('"');
        
        Assert.That(jsonValue, Is.EqualTo(jsonValue.ToLower()));
    }

    [Test]
    public void UserId_Empty_SerializesCorrectly()
    {
        var empty = UserId.Empty;
        var json = JsonSerializer.Serialize(empty);
        
        Assert.That(json, Is.EqualTo("\"usr_00000000000000000000000000\""));
    }

    [Test]
    public void UserId_Multiple_SerializeToUniqueValues()
    {
        var id1 = UserId.New();
        var id2 = UserId.New();
        
        var json1 = JsonSerializer.Serialize(id1);
        var json2 = JsonSerializer.Serialize(id2);
        
        Assert.That(json1, Is.Not.EqualTo(json2));
    }

    [Test]
    public void UserId_SerializeArray_Works()
    {
        var ids = new[] { UserId.New(), UserId.New(), UserId.New() };
        var json = JsonSerializer.Serialize(ids);
        
        var deserialized = JsonSerializer.Deserialize<UserId[]>(json);
        
        Assert.That(deserialized, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(deserialized!, Has.Length.EqualTo(3));
            Assert.That(deserialized[0], Is.EqualTo(ids[0]));
            Assert.That(deserialized[1], Is.EqualTo(ids[1]));
            Assert.That(deserialized[2], Is.EqualTo(ids[2]));
        });
    }

    [Test]
    public void UserId_SerializeWithCustomOptions_Works()
    {
        var userId = UserId.New();
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        
        var json = JsonSerializer.Serialize(userId, options);
        var deserialized = JsonSerializer.Deserialize<UserId>(json, options);
        
        Assert.That(deserialized, Is.EqualTo(userId));
    }

    [Test]
    public void User_CompleteRoundTrip_Works()
    {
        // Create a user
        var user = new User();
        
        // Serialize to JSON
        var json = JsonSerializer.Serialize(user);
        TestContext.Out.WriteLine($"Serialized User: {json}");
        
        // Verify JSON structure
        Assert.That(json, Does.Contain("\"Id\""));
        Assert.That(json, Does.Contain("\"usr_"));
        
        // Deserialize back
        var deserializedUser = JsonSerializer.Deserialize<User>(json);
        
        // Verify
        Assert.That(deserializedUser, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(deserializedUser!.Id, Is.EqualTo(user.Id));
            Assert.That(deserializedUser.Id.ToString(), Is.EqualTo(user.Id.ToString()));
        });
    }

    [Test]
    public void User_SerializeWithPrettyPrint_ShowsStructure()
    {
        var user = new User();
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        
        var json = JsonSerializer.Serialize(user, options);
        TestContext.Out.WriteLine($"Pretty-printed User JSON:\n{json}");
        
        // Should be able to parse it back
        var deserialized = JsonSerializer.Deserialize<User>(json, options);
        Assert.That(deserialized!.Id, Is.EqualTo(user.Id));
    }

    [Test]
    public void User_ArraySerialization_Works()
    {
        var users = new[] 
        { 
            new User(), 
            new User(), 
            new User(),
        };
        
        var json = JsonSerializer.Serialize(users);
        TestContext.Out.WriteLine($"User array JSON: {json}");
        
        var deserialized = JsonSerializer.Deserialize<User[]>(json);
        
        Assert.That(deserialized, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(deserialized!, Has.Length.EqualTo(3));
            Assert.That(deserialized[0].Id, Is.EqualTo(users[0].Id));
            Assert.That(deserialized[1].Id, Is.EqualTo(users[1].Id));
            Assert.That(deserialized[2].Id, Is.EqualTo(users[2].Id));
        });
    }
}