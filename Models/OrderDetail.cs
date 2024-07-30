using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class OrderDetail
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
        public Guid ProductId { get; set; }
        [ForeignKey ("ProductId")]
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Price { get; set; }
        public Guid BillingDetailId { get; set; }
        [ForeignKey("BillingDetailId")]
        public BillingDetail? BillingDetail { get; set; }
        public Guid ShippingDetailId { get; set; }
        [ForeignKey("ShippingDetailId")]
        public BillingDetail? ShippingDetail { get; set; }
    }
}
