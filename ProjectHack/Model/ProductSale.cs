﻿namespace ProjectHack.Model
{
    public class ProductSale
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime Time { get; set; }
        public int BarCode { get; set; }
    }
}
