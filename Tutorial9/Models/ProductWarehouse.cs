﻿namespace Tutorial9.Models;

public class ProductWarehouse
{
    public int IdProductWarehouse { get; set; }
    public int IdWarehouse { get; set; }
    public int IdProduct { get; set; }
    public int IdOrder { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}