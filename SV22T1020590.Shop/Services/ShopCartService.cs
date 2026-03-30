using System.Text.Json;
using SV22T1020590.Models.Sales;

namespace SV22T1020590.Shop.Services
{
    public class ShopCartService
    {
        private const string CartKey = "ShopCart";
        private readonly IHttpContextAccessor _http;

        public ShopCartService(IHttpContextAccessor http)
        {
            _http = http;
        }

        public List<OrderDetail> GetCart()
        {
            var session = _http.HttpContext?.Session;
            if (session == null) return new List<OrderDetail>();
            var json = session.GetString(CartKey);
            if (string.IsNullOrEmpty(json)) return new List<OrderDetail>();
            try
            {
                return JsonSerializer.Deserialize<List<OrderDetail>>(json) ?? new List<OrderDetail>();
            }
            catch
            {
                return new List<OrderDetail>();
            }
        }

        public void SaveCart(List<OrderDetail> cart)
        {
            var session = _http.HttpContext?.Session;
            if (session == null) return;
            var json = JsonSerializer.Serialize(cart ?? new List<OrderDetail>());
            session.SetString(CartKey, json);
        }

        public int Count => GetCart().Sum(x => x.Quantity);
    }
}
