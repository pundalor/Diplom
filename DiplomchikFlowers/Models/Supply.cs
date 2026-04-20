using System;
using System.Collections.Generic;

namespace DiplomchikFlowers.Models;

public partial class Supply
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public DateOnly SupplyDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual ICollection<SupplyItem> SupplyItems { get; set; } = new List<SupplyItem>();
}
