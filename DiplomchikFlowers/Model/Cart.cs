using System;
using System.Collections.Generic;

namespace DiplomchikFlowers.Model;

public partial class Cart
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Customer Customer { get; set; } = null!;
}
