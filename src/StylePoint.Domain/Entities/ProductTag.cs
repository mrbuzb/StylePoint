﻿namespace StylePoint.Domain.Entities;

public class ProductTag
{
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public long TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}