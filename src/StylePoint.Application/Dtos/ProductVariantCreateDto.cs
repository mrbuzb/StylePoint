﻿namespace StylePoint.Application.Dtos;

public class ProductVariantCreateDto
{
    public string Size { get; set; }
    public string Color { get; set; }
    public int Stock { get; set; }
    public decimal Price { get; set; }
}