using Microsoft.EntityFrameworkCore;

namespace IronId.Test;

/// <summary>
/// Tests for Entity Framework Core integration with IronId types.
/// </summary>
[TestFixture]
public class EfCoreIntegrationTests
{
    private TestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use unique DB for each test
            .Options;

        return new TestDbContext(options);
    }

    [Test]
    public async Task CanSaveAndRetrieveEntityWithIronId()
    {
        // Arrange
        await using var context = CreateDbContext();
        var customerId = CustomerId.New();
        var customer = new CustomerEntity
        {
            Id = customerId,
            Name = "John Doe",
            Email = "john@example.com",
            CreatedAt = DateTime.UtcNow,
        };

        // Act
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Clear context to ensure we're reading from DB
        context.ChangeTracker.Clear();

        var retrievedCustomer = await context.Customers.FindAsync(customerId);

        // Assert
        Assert.That(retrievedCustomer, Is.Not.Null);
        Assert.That(retrievedCustomer!.Id, Is.EqualTo(customerId));
        Assert.That(retrievedCustomer.Name, Is.EqualTo("John Doe"));
        Assert.That(retrievedCustomer.Email, Is.EqualTo("john@example.com"));
    }

    [Test]
    public async Task CanQueryByIronId()
    {
        // Arrange
        await using var context = CreateDbContext();
        var customerId = CustomerId.New();
        var customer = new CustomerEntity
        {
            Id = customerId,
            Name = "Jane Smith",
            Email = "jane@example.com",
            CreatedAt = DateTime.UtcNow,
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var found = await context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId);

        // Assert
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Id, Is.EqualTo(customerId));
        Assert.That(found.Name, Is.EqualTo("Jane Smith"));
    }

    [Test]
    public async Task CanHandleForeignKeyRelationships()
    {
        // Arrange
        await using var context = CreateDbContext();
        var customerId = CustomerId.New();
        var customer = new CustomerEntity
        {
            Id = customerId,
            Name = "Bob Johnson",
            Email = "bob@example.com",
            CreatedAt = DateTime.UtcNow,
        };

        var orderId = OrderId.New();
        var order = new OrderEntity
        {
            Id = orderId,
            CustomerId = customerId,
            Amount = 99.99m,
            OrderDate = DateTime.UtcNow,
        };

        // Act
        context.Customers.Add(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Load order with customer
        var retrievedOrder = await context.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        // Assert
        Assert.That(retrievedOrder, Is.Not.Null);
        Assert.That(retrievedOrder!.Id, Is.EqualTo(orderId));
        Assert.That(retrievedOrder.CustomerId, Is.EqualTo(customerId));
        Assert.That(retrievedOrder.Customer, Is.Not.Null);
        Assert.That(retrievedOrder.Customer.Id, Is.EqualTo(customerId));
        Assert.That(retrievedOrder.Customer.Name, Is.EqualTo("Bob Johnson"));
    }

    [Test]
    public async Task CanSaveMultipleEntitiesWithDifferentIronIdTypes()
    {
        // Arrange
        await using var context = CreateDbContext();
        
        var customer = new CustomerEntity
        {
            Id = CustomerId.New(),
            Name = "Alice Wonder",
            Email = "alice@example.com",
            CreatedAt = DateTime.UtcNow,
        };

        var order = new OrderEntity
        {
            Id = OrderId.New(),
            CustomerId = customer.Id,
            Amount = 150.00m,
            OrderDate = DateTime.UtcNow,
        };

        var product = new ProductEntity
        {
            Id = ProductId.New(),
            Name = "Widget",
            Sku = "WDG-001",
            Price = 29.99m,
        };

        // Act
        context.Customers.Add(customer);
        context.Orders.Add(order);
        context.Products.Add(product);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Assert
        var customerCount = await context.Customers.CountAsync();
        var orderCount = await context.Orders.CountAsync();
        var productCount = await context.Products.CountAsync();

        Assert.That(customerCount, Is.EqualTo(1));
        Assert.That(orderCount, Is.EqualTo(1));
        Assert.That(productCount, Is.EqualTo(1));
    }

    [Test]
    public async Task IronIdIsStoredAsStringInDatabase()
    {
        // Arrange
        await using var context = CreateDbContext();
        var productId = ProductId.New();
        var product = new ProductEntity
        {
            Id = productId,
            Name = "Test Product",
            Sku = "TST-001",
            Price = 19.99m,
        };

        // Act
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Query using the string representation
        var productIdString = productId.ToString();
        
        // EF Core should be able to compare the IronId with its string representation
        var found = await context.Products
            .Where(p => p.Id.ToString() == productIdString)
            .FirstOrDefaultAsync();

        // Assert
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Id.ToString(), Is.EqualTo(productIdString));
        Assert.That(found.Id.ToString(), Does.StartWith("prod_"));
    }

    [Test]
    public async Task CanUpdateEntityWithIronId()
    {
        // Arrange
        await using var context = CreateDbContext();
        var customerId = CustomerId.New();
        var customer = new CustomerEntity
        {
            Id = customerId,
            Name = "Original Name",
            Email = "original@example.com",
            CreatedAt = DateTime.UtcNow,
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var toUpdate = await context.Customers.FindAsync(customerId);
        Assert.That(toUpdate, Is.Not.Null);
        
        toUpdate!.Name = "Updated Name";
        toUpdate.Email = "updated@example.com";
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Assert
        var updated = await context.Customers.FindAsync(customerId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Id, Is.EqualTo(customerId)); // ID unchanged
        Assert.That(updated.Name, Is.EqualTo("Updated Name"));
        Assert.That(updated.Email, Is.EqualTo("updated@example.com"));
    }

    [Test]
    public async Task CanDeleteEntityWithIronId()
    {
        // Arrange
        await using var context = CreateDbContext();
        var customerId = CustomerId.New();
        var customer = new CustomerEntity
        {
            Id = customerId,
            Name = "To Be Deleted",
            Email = "delete@example.com",
            CreatedAt = DateTime.UtcNow,
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var toDelete = await context.Customers.FindAsync(customerId);
        Assert.That(toDelete, Is.Not.Null);
        
        context.Customers.Remove(toDelete!);
        await context.SaveChangesAsync();

        // Assert
        var deleted = await context.Customers.FindAsync(customerId);
        Assert.That(deleted, Is.Null);
    }

    [Test]
    public async Task CanHandleMultipleOrdersForSameCustomer()
    {
        // Arrange
        await using var context = CreateDbContext();
        var customerId = CustomerId.New();
        var customer = new CustomerEntity
        {
            Id = customerId,
            Name = "Multi Order Customer",
            Email = "multi@example.com",
            CreatedAt = DateTime.UtcNow,
        };

        var order1 = new OrderEntity
        {
            Id = OrderId.New(),
            CustomerId = customerId,
            Amount = 50.00m,
            OrderDate = DateTime.UtcNow,
        };

        var order2 = new OrderEntity
        {
            Id = OrderId.New(),
            CustomerId = customerId,
            Amount = 75.00m,
            OrderDate = DateTime.UtcNow.AddDays(1),
        };

        var order3 = new OrderEntity
        {
            Id = OrderId.New(),
            CustomerId = customerId,
            Amount = 100.00m,
            OrderDate = DateTime.UtcNow.AddDays(2),
        };

        // Act
        context.Customers.Add(customer);
        context.Orders.AddRange(order1, order2, order3);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Load customer with orders
        var customerWithOrders = await context.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == customerId);

        // Assert
        Assert.That(customerWithOrders, Is.Not.Null);
        Assert.That(customerWithOrders!.Orders, Has.Count.EqualTo(3));
        Assert.That(customerWithOrders.Orders.Sum(o => o.Amount), Is.EqualTo(225.00m));
        Assert.That(customerWithOrders.Orders.All(o => o.CustomerId == customerId), Is.True);
    }

    [Test]
    public async Task IronIdConverterHandlesEmptyGuid()
    {
        // Arrange
        await using var context = CreateDbContext();
        var emptyCustomerId = new CustomerId(Ulid.Empty);
        var customer = new CustomerEntity
        {
            Id = emptyCustomerId,
            Name = "Empty ID Customer",
            Email = "empty@example.com",
            CreatedAt = DateTime.UtcNow,
        };

        // Act
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var retrieved = await context.Customers.FindAsync(emptyCustomerId);

        // Assert
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.Id, Is.EqualTo(emptyCustomerId));
        Assert.That(retrieved.Id.Value, Is.EqualTo(Ulid.Empty));
    }

    [Test]
    public async Task CanOrderByIronId()
    {
        // Arrange
        await using var context = CreateDbContext();
        
        // Create customers with known IDs
        var customer1 = new CustomerEntity { Id = CustomerId.New(), Name = "Customer 1", Email = "c1@example.com", CreatedAt = DateTime.UtcNow };
        var customer2 = new CustomerEntity { Id = CustomerId.New(), Name = "Customer 2", Email = "c2@example.com", CreatedAt = DateTime.UtcNow };
        var customer3 = new CustomerEntity { Id = CustomerId.New(), Name = "Customer 3", Email = "c3@example.com", CreatedAt = DateTime.UtcNow };

        context.Customers.AddRange(customer3, customer1, customer2); // Add in mixed order
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var ordered = await context.Customers
            .OrderBy(c => c.Id)
            .ToListAsync();

        // Assert
        Assert.That(ordered, Has.Count.EqualTo(3));
        // IronIds are ordered by their string representation (prefix_ulid)
        // Since all have the same prefix, they're ordered by ULID (time-based)
    }

    [Test]
    public async Task CanUseIronIdInComplexQuery()
    {
        // Arrange
        await using var context = CreateDbContext();
        
        var customer1 = new CustomerEntity { Id = CustomerId.New(), Name = "Customer A", Email = "a@example.com", CreatedAt = DateTime.UtcNow };
        var customer2 = new CustomerEntity { Id = CustomerId.New(), Name = "Customer B", Email = "b@example.com", CreatedAt = DateTime.UtcNow };
        
        var order1 = new OrderEntity { Id = OrderId.New(), CustomerId = customer1.Id, Amount = 100m, OrderDate = DateTime.UtcNow };
        var order2 = new OrderEntity { Id = OrderId.New(), CustomerId = customer1.Id, Amount = 200m, OrderDate = DateTime.UtcNow };
        var order3 = new OrderEntity { Id = OrderId.New(), CustomerId = customer2.Id, Amount = 150m, OrderDate = DateTime.UtcNow };

        context.Customers.AddRange(customer1, customer2);
        context.Orders.AddRange(order1, order2, order3);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act - Complex query: Get customers with total order amount > 150
        var customersWithHighOrders = await context.Customers
            .Where(c => c.Orders.Sum(o => o.Amount) > 150m)
            .Select(c => new
            {
                c.Id,
                c.Name,
                TotalAmount = c.Orders.Sum(o => o.Amount),
                OrderCount = c.Orders.Count,
            })
            .ToListAsync();

        // Assert
        Assert.That(customersWithHighOrders, Has.Count.EqualTo(1));
        Assert.That(customersWithHighOrders[0].Id, Is.EqualTo(customer1.Id));
        Assert.That(customersWithHighOrders[0].TotalAmount, Is.EqualTo(300m));
        Assert.That(customersWithHighOrders[0].OrderCount, Is.EqualTo(2));
    }
}

