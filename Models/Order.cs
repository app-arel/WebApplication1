using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public enum OrderStatus
    {
        Pending,
        Paid,
        Processing,
        Delivered
    }
    public class Order
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; } 
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime DeliveryDate { get; set; }
    }
}
