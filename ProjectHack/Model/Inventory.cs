namespace ProjectHack.Model
{
    public static class Inventory
    {
        public static decimal CalculateProfitFifo(List<ProductSale> sales, Dictionary<string, int> stock)
        {
            decimal profit = 0;
            decimal cost = 0;
            var queue = new Queue<ProductSale>();
            foreach (ProductSale sale in sales)
            {
                queue.Enqueue(sale);
                while (queue.Count > 0)
                {
                    ProductSale currentSale = queue.Peek();
                    if (stock[currentSale.Product] >= currentSale.Quantity)
                    {
                        stock[currentSale.Product] -= currentSale.Quantity;
                        cost += currentSale.Quantity * currentSale.Price;
                        profit += currentSale.Quantity * (sale.Price - currentSale.Price);
                        queue.Dequeue();
                    }
                    else
                    {
                        currentSale.Quantity -= stock[currentSale.Product];
                        cost += stock[currentSale.Product] * currentSale.Price;
                        stock[currentSale.Product] = 0;
                        queue.Enqueue(currentSale);
                    }
                }
            }
            return profit;
        }
    }
}
