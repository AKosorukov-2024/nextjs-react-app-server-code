using System;
using System.Collections.Generic;

namespace WebAPIWithEF.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int ProductTypeId { get; set; }

    public virtual ProductType? ProductType { get; set; }
}
