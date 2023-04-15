using System.Text.Json.Serialization;

namespace ProjectHack.Model
{
    public class SaleForUpdate : Sale
    {
        [JsonIgnore]
        public long BarCode { get; set; }
    }
    public class SuppliForUpdate : Sale
    {
        [JsonIgnore]
        public long BarCode { get; set; }
    }
}
