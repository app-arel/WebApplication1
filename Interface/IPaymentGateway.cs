namespace WebApplication1.Interface
{
    public interface IPaymentGateway
    {
        public Task<string> CreatePaymentIntent(decimal amount, string email, string currency, string txRef, string redirectUrl);
    }
}
