namespace WebApplication1.Models
{
    public class FlutterwaveRequest
    {
        public string TxRef { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string RedirectUrl { get; set; }
        public string PaymentOptions { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerName { get; set; }
        public string CustomizationsTitle { get; set; }
        public string CustomizationsDescription { get; set; }
        public string CustomizationsLogo { get; set; }
    }

}
