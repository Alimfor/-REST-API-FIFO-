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
        public static decimal CalculateProfitFifo(DateTime startDate, DateTime endDate)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("dbsetting.json")
                .Build();
            decimal profit = 0;
            var stock = new Dictionary<string, Queue<(int quantity, decimal cost)>>();

            using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                // get data supply
                string supplyQuery = "SELECT * FROM supply WHERE supply_time >= @startDate AND supply_time <= @endDate";
                var supplyData = connection.Query(supplyQuery, new { startDate, endDate }).ToList();
                foreach (var row in supplyData)
                {
                    string barcode = row.barcode;
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
                var saleData = connection.Query(saleQuery, new { startDate, endDate }).ToList();
                foreach (var row in saleData)
                {
                    string barcode = row.barcode;
                    int quantity = row.quantity;
                    decimal price = row.price;

                    // We calculate the profit for the goods sold
                    while (quantity > 0 && stock.ContainsKey(barcode) && stock[barcode].Count > 0)
                    {
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
            return profit;
        }
        public static IEnumerable<T> GetAllDataTable<T>(string tableName)
        {
            var config = new ConfigurationBuilder()
            .AddJsonFile("dbsetting.json")
            .Build();

            //var result = new List<string>();

            using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));

            string query = $"SELECT * FROM {tableName}";
            return SqlMapper.Query<T>(connection,query);

            //var command = new SqlCommand(query, connection);
            //connection.Open();

            //using SqlDataReader reader = command.ExecuteReader();
            //while (reader.Read())
            //{
            //    int id = reader.GetInt32(0);
            //    long barcode = reader.GetInt64(1);
            //    int quantity = reader.GetInt32(2);
            //    int price = reader.GetInt32(3);
            //    DateTime supplyTime = reader.GetDateTime(4);

            //    string data = $"{id}, {barcode}, {quantity}, {price}, {supplyTime}";
            //    result.Add(data);
            //}
            //return result;
        }
        public static void InsertTableData<T>(T data,string tableName) where T : class,new()
        {
            var config = new ConfigurationBuilder()
            .AddJsonFile("dbsetting.json")
            .Build();
            using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            connection.Execute(
                $"INSERT INTO {tableName} (barcode, quantity, price, {tableName}_time) " +
                $"VALUES (@Barcode, @Quantity, @price, @Time)",
                new
                {
                    Barcode = typeof(T).GetProperty("BarCode").GetValue(data),
                    Quantity = typeof(T).GetProperty("Quantity").GetValue(data),
                    price = typeof(T).GetProperty("Price").GetValue(data),
                    Time = typeof(T).GetProperty("Time").GetValue(data)
                });
            //connection.Open();

            //string query = $"INSERT INTO {tableName} (barcode, quantity, price, sale_time) " +
            //               "VALUES (@barcode, @quantity, @price, @sale_time)";

            //using var command = new SqlCommand(query, connection);
            //command.Parameters.AddWithValue("@barcode", typeof(T).GetProperty("BarCode").GetValue(data));
            //command.Parameters.AddWithValue("@quantity", typeof(T).GetProperty("Quantity").GetValue(data));
            //command.Parameters.AddWithValue("@price", typeof(T).GetProperty("Price").GetValue(data));
            //command.Parameters.AddWithValue("@sale_time", typeof(T).GetProperty("Time").GetValue(data));

            //command.ExecuteNonQuery();
        }
        public static void DeleteTableData(int id,string tableName)
        {
            var config = new ConfigurationBuilder()
            .AddJsonFile("dbsetting.json")
            .Build();
            using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            connection.Execute(
                $"DELETE FROM {tableName} " +
                $"WHERE id=@Id",
                new { Id = id }
                );
        }
        public static void UpdateTableData<T>(T data, int id,string tableName) where T : class,new()
        {
            var config = new ConfigurationBuilder()
            .AddJsonFile("dbsetting.json")
            .Build();
            using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            connection.Execute(
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
