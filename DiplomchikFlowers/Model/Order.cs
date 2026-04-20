using System;
using System.Collections.Generic;

namespace DiplomchikFlowers.Model;

public partial class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public DateTime? OrderDate { get; set; }

    public int StatusId { get; set; }

    public decimal TotalAmount { get; set; }

    public string? ShippingAddress { get; set; }

    public string? City { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual OrderStatus Status { get; set; } = null!;
}
