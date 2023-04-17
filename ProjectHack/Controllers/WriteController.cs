using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProjectHack.Model;
using ProjectHack.Model.RAMinformationService;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Text.Json.Serialization;
using static ProjectHack.Controllers.WriteController;

namespace ProjectHack.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WriteController : ControllerBase
    {
        private readonly IMemoryCache memoryCache;
        public WriteController(IMemoryCache cache) => memoryCache = cache;


        [HttpGet]
        [Route("Selectprofit")]
        public async Task<ActionResult<decimal>> CalculateProfitFifo(long barcode, DateTime startDate, DateTime endDate)
        {
            string cachekey = $"profit-{barcode}-{startDate}-{endDate}";

            if (memoryCache.TryGetValue(cachekey, out decimal profit))
            {
                return Ok(profit);
            }

            profit = await Inventory.CalculateProfitFifoAsync(barcode, startDate, endDate);
            memoryCache.Set(cachekey, profit, TimeSpan.FromMinutes(30));
            return Ok(profit);
        }

        [HttpGet]
        [Route("GetAllDataSale")]
        public async Task<ActionResult> GetAllDataTableAsync()
        {
            var data = await Inventory.GetAllDataTableAsync<Sale>("sale");
            return Ok(data);
        }

        [HttpGet]
        [Route("GetAllDataSupply")]
        public async Task<ActionResult> GetAllDataSupplyAsync()
        {
            var data = await Inventory.GetAllDataTableAsync<Supply>("supply");
            return Ok(data);
        }

        [HttpPost]
        [Route("AddSaleData")]
        public async Task<IActionResult> AddSaleDataAsync(Sale data)
        {
            await Inventory.InsertTableData(data,"sale");
            return Ok();
        }

        [HttpPost]
        [Route("AddSupplyData")]
        public async Task<IActionResult> AddSupplyDataAsync(Supply data)
        {
            await Inventory.InsertTableData(data,"supply");
            return Ok();
        }

        [HttpDelete]
        [Route("DeleteSaleData")]
        public async Task<OkResult> DeleteSaleDataAsync(int id)
        {
            await Inventory.DeleteTableData(id,"sale");
            return Ok();
        }
        
        [HttpDelete]
        [Route("DeleteSupplyData")]
        public async Task<OkResult> DeleteSupplyDataAsync(int id)
        {
            await Inventory.DeleteTableData(id,"supply");
            return Ok();
        }

        [HttpPut]
        [Route("UpdateSaleData")]
        public async Task<OkResult> UpdateSaleDataAsync(SaleForUpdate data,int id)
        {
            await Inventory.UpdateTableData(data,id, "sale");
            return Ok();
        }
        
        [HttpPut]
        [Route("UpdateSypplyData")]
        public async Task<OkResult> UpdateSupplyDataAsync(SuppliForUpdate data,int id)
        {
            await Inventory.UpdateTableData(data,id, "supply");
            return Ok();
        }

        [HttpGet]
        [Route("DataBaseMemory")]
        public async Task<ActionResult> GetDataBaseMemoryAsync()
        {
            var connectionString = new ConfigurationBuilder()
            .AddJsonFile("dbsetting.json")
            .Build();
            long memory = 0;
            using (var connection = new SqlConnection(connectionString.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                var command = new SqlCommand("SELECT physical_memory_in_use_kb FROM sys.dm_os_process_memory", connection);
                memory = (long)await command.ExecuteScalarAsync();
            }
            return Ok(memory);
        }

        [HttpGet]
        [Route("Memory")]
        public IActionResult GetMemoryUsage()
        {
            var memoryInfoService = HttpContext.RequestServices.GetService<IMemoryInfoService>();
            long usedMemory = memoryInfoService.GetUsedMemory();
            return Ok($"Used memory: {usedMemory} bytes");
        }
    }
    
}