using System;
using System.Collections.Generic;

namespace WebAPIWithEF.Models;

public class ProductData:Product
{
    public string TypeName { get; set; } = null!;
}
