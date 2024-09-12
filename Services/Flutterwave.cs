using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebApplication1.Interface;

namespace WebApplication1.Services
{
    public class Flutterwave : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://api.flutterwave.com/v3/payments";/*
        private readonly string _publicKey = "FLWPUBK_TEST-c72bc8cfd5084a7154b9aece0d395257-X";*/
        private readonly string _secretKey = "FLWSECK_TEST-0c99b1fea53f9b28dd86356b8ff73817-X";
        public Flutterwave()
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
                redirect_Url = redirectUrl,
                tx_ref = txRef,
                customer = new
                {
                    email = email
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_baseUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();
                return responseString;
        }
    }
}