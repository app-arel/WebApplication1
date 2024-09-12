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
    public class Paystack : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://api.paystack.co/transaction/initialize";
        private readonly string _secretKey = "sk_test_09fcc7324b1fb8f097bcf64e7b5d6b07bf598f81";
        public Paystack(HttpClient httpClient)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _secretKey);
        }



        public async Task<string> CreatePaymentIntent(decimal amount, string email, string currency, string txRef, string redirectUrl)
        {
            var request = new
            {
                amount = amount,
                email = email,
                currency = currency,
                tx_ref = txRef,
                redirect_url = redirectUrl
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_baseUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();
                return responseString;
        }
    }
}