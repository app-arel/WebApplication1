using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebApplication1.Interface;

namespace WebApplication1.Services
{
    public class Stripe : IPaymentGateway
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl = "https://api.flutterwave.com/v3/charges?type=bank_transfer";

        private readonly string _secretKey = "sk_test_51Pqwpt06g9mc5Jcl5o9rBCDu7LfOTVI7WXhsvOlIj7AdWFppX27QsQflSMMmHytpLrSsMKnyRmGUaSlD3JDLggDC00pewGOhNr";
        private readonly string _publishableKey = "pk_test_51Pqwpt06g9mc5Jclk71gLFW4KzA85e7yliE1Fq0VX8lNpIaiO2QRc62uRZG54mUQlNbdm0x9Ux1iiU5YZrmwV3HR00TJcMsiex";
        public Stripe()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _secretKey);
        }



        public async Task<string> CreatePaymentIntent(decimal amount, string email, string currency, string txRef, string redirectUrl)
        {
            var request = new
            {
                amount = amount,
                email = email,
                currency = "NGN",
                tx_ref = "PaymentRef"
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"https://api.flutterwave.com/v3/charges?type=bank_transfer", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
    }
}