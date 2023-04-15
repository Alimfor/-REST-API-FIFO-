﻿namespace ProjectHack.Model
{
    public class Supply
    {
        public int Id { get; set; }
        public string BarCode { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime Time { get; set; }
    }
}
