using IronId.Generated;

namespace IronId.Test;

/// <summary>
/// Test entity representing a customer.
/// </summary>
[IronId("customer")]
public partial class Customer
{
}

/// <summary>
/// Customer entity for EF Core tests.
/// </summary>
public class CustomerEntity
{
    public CustomerId Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; init; }

    public ICollection<OrderEntity> Orders { get; init; } = new List<OrderEntity>();
}

/// <summary>
/// Test entity representing an order.
/// </summary>
[IronId("ord")]
public class Order;

/// <summary>
/// Order entity for EF Core tests.
/// </summary>
public class OrderEntity
{
    public OrderId Id { get; init; }
    public CustomerId CustomerId { get; init; }
    public decimal Amount { get; init; }
    public DateTime OrderDate { get; init; }

    public CustomerEntity Customer { get; init; } = null!;
}

/// <summary>
/// Test entity representing a product.
/// </summary>
[IronId("prod")]
public class Product;

/// <summary>
/// Product entity for EF Core tests.
/// </summary>
public class ProductEntity
{
    public ProductId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
}