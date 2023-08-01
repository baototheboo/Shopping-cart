using ClientApp.Models;

namespace ClientApp.DTO
{
    public class BuyDTO
    {
        public List<OrderDetail> orderDetails { get; set; }

        public Customer customer { get; set; }

        public DateTime requiredDate { get; set; }
    }
}
