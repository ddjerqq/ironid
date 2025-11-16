namespace IronId.Test;

/// <summary>
/// Tests for UserId generated type.
/// </summary>
public class UserIdTests
{
    [Test]
    public void UserId_New_CreatesNonEmptyId()
    {
        var id = UserId.New();
        
        Assert.That(id, Is.Not.EqualTo(UserId.Empty));
        Assert.That(id.ToString(), Does.StartWith("usr_"));
    }

    [Test]
    public void UserId_Empty_ReturnsEmptyId()
    {
        var empty = UserId.Empty;
        
        Assert.That(empty.ToString(), Is.EqualTo("usr_00000000000000000000000000"));
    }

    [Test]
    public void UserId_Prefix_IsCorrect()
    {
        Assert.That(UserId.Prefix, Is.EqualTo("usr"));
    }

    [Test]
    public void UserId_Parse_ValidString_ReturnsCorrectId()
    {
        var id = UserId.New();
        var idString = id.ToString();
        
        var parsed = UserId.Parse(idString);
        
        Assert.That(parsed, Is.EqualTo(id));
        Assert.That(parsed.ToString(), Is.EqualTo(idString));
    }

    [Test]
    public void UserId_Parse_InvalidString_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => UserId.Parse("invalid"));
    }

    [Test]
    public void UserId_TryParse_ValidString_ReturnsTrue()
    {
        var id = UserId.New();
        var idString = id.ToString();
        
        var success = UserId.TryParse(idString, null, out var parsed);
        Assert.Multiple(() =>
        {
            Assert.That(success, Is.True);
            Assert.That(parsed, Is.EqualTo(id));
        });
    }

    [Test]
    public void UserId_TryParse_InvalidString_ReturnsFalse()
    {
        var success = UserId.TryParse("invalid", null, out var _);
        
        Assert.That(success, Is.False);
    }

    [Test]
    public void UserId_TryParse_WrongPrefix_ReturnsFalse()
    {
        var success = UserId.TryParse("pst_01k9axfv3hw9ffa2850ks02njz", null, out _);
        
        Assert.That(success, Is.False);
    }

    [Test]
    public void UserId_TryParse_NullOrEmpty_ReturnsFalse()
    {
        Assert.Multiple(() =>
        {
            Assert.That(UserId.TryParse(null, null, out _), Is.False);
            Assert.That(UserId.TryParse("", null, out _), Is.False);
            Assert.That(UserId.TryParse("   ", null, out _), Is.False);
        });
    }

    [Test]
    public void UserId_ImplicitConversion_ToString_Works()
    {
        var id = UserId.New();
        string idString = id;
        
        Assert.That(idString, Is.EqualTo(id.ToString()));
    }

    [Test]
    public void UserId_ExplicitConversion_FromString_Works()
    {
        var id = UserId.New();
        var idString = id.ToString();
        
        var converted = (UserId)idString;
        
        Assert.That(converted, Is.EqualTo(id));
    }

    [Test]
    public void UserId_ImplicitConversion_ToUlid_Works()
    {
        var id = UserId.New();
        Ulid ulid = id;
        
        Assert.That(ulid, Is.Not.EqualTo(Ulid.Empty));
    }

    [Test]
    public void UserId_ExplicitConversion_FromUlid_Works()
    {
        var ulid = Ulid.NewUlid();
        var id = (UserId)ulid;
        
        Assert.That(id.Value, Is.EqualTo(ulid));
    }

    [Test]
    public void UserId_CompareTo_SameValue_ReturnsZero()
    {
        var id1 = UserId.New();
        var id2 = UserId.Parse(id1.ToString());
        
        Assert.That(id1.CompareTo(id2), Is.EqualTo(0));
    }

    [Test]
    public void UserId_CompareTo_Null_ReturnsPositive()
    {
        var id = UserId.New();
        
        Assert.That(id.CompareTo(null!), Is.GreaterThan(0));
    }

    [Test]
    public void UserId_CompareTo_WrongType_ThrowsArgumentException()
    {
        var id = UserId.New();
        
        Assert.Throws<ArgumentException>(() => id.CompareTo("wrong type"));
    }

    [Test]
    public void UserId_Equality_SameValue_ReturnsTrue()
    {
        var id1 = UserId.New();
        var id2 = UserId.Parse(id1.ToString());
        Assert.Multiple(() =>
        {
            Assert.That(id1 == id2, Is.True);
            Assert.That(id1.Equals(id2), Is.True);
        });
    }

    [Test]
    public void UserId_Equality_DifferentValue_ReturnsFalse()
    {
        var id1 = UserId.New();
        var id2 = UserId.New();
        Assert.Multiple(() =>
        {
            Assert.That(id1 == id2, Is.False);
            Assert.That(id1.Equals(id2), Is.False);
        });
    }

    [Test]
    public void UserId_ToString_IsLowercase()
    {
        var id = UserId.New();
        var str = id.ToString();
        
        Assert.That(str, Is.EqualTo(str.ToLower()));
    }

    [Test]
    public void UserId_MultipleNew_AreUnique()
    {
        var id1 = UserId.New();
        var id2 = UserId.New();
        var id3 = UserId.New();
        
        Assert.That(id1, Is.Not.EqualTo(id2));
        Assert.Multiple(() =>
        {
            Assert.That(id1, Is.Not.EqualTo(id3));
            Assert.That(id2, Is.Not.EqualTo(id3));
        });
    }
}

/// <summary>
/// Tests for User entity with IronId.
/// </summary>
public class UserEntityTests
{
    [Test]
    public void User_Create_GeneratesValidUserId()
    {
        var user = new User();
        
        Assert.That(user.Id, Is.Not.EqualTo(UserId.Empty));
        Assert.That(user.Id.ToString(), Does.StartWith("usr_"));
    }

    [Test]
    public void User_Constructor_AcceptsUserId()
    {
        var userId = UserId.New();
        var user = new User(userId);
        
        Assert.That(user.Id, Is.EqualTo(userId));
    }

    [Test]
    public void User_TwoUsers_HaveDifferentIds()
    {
        var user1 = new User();
        var user2 = new User();
        
        Assert.That(user1.Id, Is.Not.EqualTo(user2.Id));
    }

    [Test]
    public void User_CanBeReconstructedFromId()
    {
        var user1 = new User();
        var userId = user1.Id;
        
        // Simulate persistence and retrieval
        var userIdString = userId.ToString();
        var retrievedUserId = UserId.Parse(userIdString);
        var user2 = new User(retrievedUserId);
        
        Assert.That(user2.Id, Is.EqualTo(user1.Id));
    }
}