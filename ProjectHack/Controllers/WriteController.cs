using Microsoft.AspNetCore.Mvc;
using ProjectHack.Model;
using System.Data.SqlClient;
using System.Text.Json.Serialization;

namespace ProjectHack.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WriteController : ControllerBase
    {
        [HttpGet]
        [Route("Selectprofit")]
        public ActionResult<decimal> CalculateProfitFifo(DateTime startDate, DateTime endDate)
        {
            decimal profit = Inventory.CalculateProfitFifo(startDate,endDate);
            return Ok(profit);
        }

        [HttpGet]
        [Route("GetAllDataSales")]
        public ActionResult GetAllDataSales()
        {
            return Ok(Inventory.GetAllDataTable<Sale>("sale"));
        }

        [HttpGet]
        [Route("GetAllDataSupply")]
        public ActionResult GetAllDataSupply()
        {
            return Ok(Inventory.GetAllDataTable<Supply>("supply"));
        }

        [HttpPost]
        [Route("AddSaleData")]
        public IActionResult AddSaleData(Sale data)
        {
            Inventory.InsertTableData(data,"sale");
            return Ok();
        }

        [HttpPost]
        [Route("AddSupplyData")]
        public IActionResult AddSupplyData(Supply data)
        {
            Inventory.InsertTableData(data,"supply");
            return Ok();
        }

        [HttpDelete]
        [Route("DeleteSaleData")]
        public OkResult DeleteSaleData(int id)
        {
            Inventory.DeleteTableData(id,"sale");
            return Ok();
        }
        
        [HttpDelete]
        [Route("DeleteSupplyData")]
        public OkResult DeleteSupplyData(int id)
        {
            Inventory.DeleteTableData(id,"supply");
            return Ok();
        }

        [HttpPut]
        [Route("UpdateSaleData")]
        public OkResult UpdateSaleData(SaleForUpdate data,int id)
        {
            Inventory.UpdateTableData(data,id, "sale");
            return Ok();
        }
        
        [HttpPut]
        [Route("UpdateSypplyData")]
        public OkResult UpdateSupplyData(SuppliForUpdate data,int id)
        {
            Inventory.UpdateTableData(data,id, "supply");
            return Ok();
        }
    }
    
}