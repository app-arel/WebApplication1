/*using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class FlutterwaveController : Controller
    {
        private readonly string _flutterwaveBaseUrl = "https://api.flutterwave.com/v3/payments";
        private readonly string _flutterwaveSecretKey = "FLWSECK_TEST-0c99b1fea53f9b28dd86356b8ff73817-X";
        public ActionResult InitiatePayment()
        {
            var paymentRequest = new FlutterwaveRequest
            {
                TxRef = Guid.NewGuid().ToString(),
                Amount = 1000, // Amount in the currency's smallest unit
                Currency = "NGN", // Replace with your currency code
                RedirectUrl = Url.Action("PaymentCallback", "Payment", null, Request.Url.Scheme),
                PaymentOptions = "card", // Can include more options like 'banktransfer', 'ussd'
                CustomerEmail = "customer@example.com",
                CustomerPhone = "1234567890",
                CustomerName = "John Doe",
                CustomizationsTitle = "Payment for Services",
                CustomizationsDescription = "Payment description",
                CustomizationsLogo = Url.Content("~/images/logo.png")
            };

            var client = new RestClient(_flutterwaveBaseUrl);
            var request = new RestRequest(Method.Post);
            request.AddHeader("Authorization", $"Bearer {_flutterwaveSecretKey}");
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(paymentRequest);

            IRestResponse response = client.Execute(request);
            var paymentResponse = JsonConvert.DeserializeObject<FlutterwaveResponse>(response.Content);

            if (paymentResponse.Status == "success")
            {
                return Redirect(paymentResponse.RedirectUrl);
            }

            return View("Error"); // Handle error appropriately
        }
    }
}
*/