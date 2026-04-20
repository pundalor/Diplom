using System;
using System.Collections.Generic;

namespace DiplomchikFlowers.Model;

public partial class Notification
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}
