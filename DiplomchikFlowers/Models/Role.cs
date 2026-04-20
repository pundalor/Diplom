using System;
using System.Collections.Generic;

namespace DiplomchikFlowers.Models;

public partial class Role
{
    public int Roleid { get; set; }

    public string? Rolename { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
