using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Collections;
using System.Data.SqlClient;

namespace ProjectHack.Model
{
    public static class Inventory
    {
        public static async Task<decimal> CalculateProfitFifoAsync(long barcode, DateTime startDate, DateTime endDate)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("dbsetting.json")
                .Build();
            decimal profit = 0;
            int AllQuantity = 0;
            var stock = new Dictionary<long, Queue<(int quantity, decimal cost)>>();

            using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                // get data supply
                string supplyQuery = "SELECT * FROM supply WHERE barcode = @barcode AND quantity > 0";
                var supplyData = await connection.QueryAsync(supplyQuery, new { barcode, startDate, endDate });
                foreach (var row in supplyData)
                {
                    int quantity = row.quantity;
                    decimal cost = row.price;

                    // Adding supplies to the stock dictionary
                    if (stock.ContainsKey(barcode))
                    {
                        stock[barcode].Enqueue((quantity, cost));
                    }
                    else
                    {
                        var queue = new Queue<(int quantity, decimal cost)>();
                        queue.Enqueue((quantity, cost));
                        stock.Add(barcode, queue);
                    }
                }

                // get data sale
                string saleQuery = "SELECT * FROM sale WHERE sale_time >= @startDate AND sale_time <= @endDate ORDER BY sale_time ASC";
                var saleData = await connection.QueryAsync(saleQuery, new { barcode, startDate, endDate });
                foreach (var row in saleData)
                {
                    int quantity = row.quantity;
                    decimal price = row.price;
                    AllQuantity += quantity;

                    // We calculate the profit for the goods sold
                    while (true)
                    {
                        if(quantity <= 0 || !stock.ContainsKey(barcode) || stock[barcode].Count <= 0)
                        {
                            break;
                        }
                        var stockItem = stock[barcode].Peek();
                        int soldQuantity = Math.Min(quantity, stockItem.quantity);
                        quantity -= soldQuantity;

                        decimal cost = stockItem.cost;
                        stockItem.quantity -= soldQuantity;

                        // If the quantity of goods in the warehouse for this delivery has become equal to 0,
                        // then we delete it from the dictionary
                        if (stockItem.quantity == 0)
                        {
                            stock[barcode].Dequeue();
                        }

                        decimal profitFromSale = (price - cost) * soldQuantity;
                        profit += profitFromSale;
                        
                    }
                }
            }
            await Console.Out.WriteLineAsync(AllQuantity + "");
            return profit;
        }
        public static async Task<IEnumerable<T>> GetAllDataTableAsync<T>(string tableName)
        {
            var config = new ConfigurationBuilder()
            .AddJsonFile("dbsetting.json")
            .Build();

            using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

            string query = $"SELECT * FROM {tableName}";
            return await connection.QueryAsync<T>(query);
        }
        public static async Task InsertTableData<T>(T data,string tableName) where T : class,new()
        {
            var config = new ConfigurationBuilder()
            .AddJsonFile("dbsetting.json")
            .Build();
            using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            await connection.ExecuteAsync(
                $"INSERT INTO {tableName} (barcode, quantity, price, {tableName}_time) " +
                $"VALUES (@Barcode, @Quantity, @price, @Time)",
                new
                {
                    Barcode = typeof(T).GetProperty("BarCode").GetValue(data),
                    Quantity = typeof(T).GetProperty("Quantity").GetValue(data),
                    price = typeof(T).GetProperty("Price").GetValue(data),
                    Time = typeof(T).GetProperty("Time").GetValue(data)
                });
        }
        public static async Task DeleteTableData(int id,string tableName)
        {
            var config = new ConfigurationBuilder()
            .AddJsonFile("dbsetting.json")
            .Build();
            using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            await connection.ExecuteAsync(
                $"DELETE FROM {tableName} " +
                $"WHERE id=@Id",
                new { Id = id }
                );
        }
        public static async Task UpdateTableData<T>(T data, int id,string tableName) where T : class,new()
        {
            var config = new ConfigurationBuilder()
            .AddJsonFile("dbsetting.json")
            .Build();
            using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            await connection.ExecuteAsync(
                    $"UPDATE {tableName} " +
                    $"SET quantity = @Quantity, " +
                    $"price = @Price, {tableName}_time = @Time " +
                    $"WHERE id = @Id",
                    new
                    {
                        Quantity = typeof(T).GetProperty("Quantity").GetValue(data),
                        Price = typeof(T).GetProperty("Price").GetValue(data),
                        Time = typeof(T).GetProperty("Time").GetValue(data),
                        Id = id
                    }
                );
        }
    }
}
