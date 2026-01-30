using System;
using System.Collections.Generic;

namespace WebAPIWithEF.Models;

public partial class ProductDataClient
{
    public int id { get; set; }

    public string name { get; set; } = null!;

    public decimal price { get; set; }

    public int typeId { get; set; }

    public string? typeName { get; set; }
}
