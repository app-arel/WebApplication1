using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using WebApplication1.Models;
using WebApplication1.Data;
using MailKit.Security;
using MimeKit;
using System.Text;
using MailKit.Net.Smtp;
using Stripe.Issuing;
using Stripe.Climate;
using WebApplication1.Services;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace WebApplication1.Controllers
{
    public class CheckOutController : Controller
    {
        private readonly DataContext _context;/*
        private readonly HttpClient _httpClient;*/
        public CheckOutController(DataContext context/*, HttpClient httpClient*/)
        {
            _context = context;/*
            _httpClient = httpClient;*/
        }
        public IActionResult Stripe()
        {
            var allCarts = _context.Carts.ToList();
            var userId = Guid.Parse(HttpContext.Session.GetString("userId"));
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);
            var order = _context.Orders.FirstOrDefault(x => x.UserId == userId);
            var customerCart = _context.Carts.Where(x => x.User.Id == userId).ToList();
            var products = _context.Products.ToList();
            var orderId = Guid.Parse(HttpContext.Session.GetString("orderId"));
            var specificOrder = _context.OrderDetails.FirstOrDefault(x => x.OrderId == orderId);
            specificOrder.PaymentOption = "Stripe";
            specificOrder.TransactionTime = DateTime.Now;
            List<Models.Product> Userproducts = new List<Models.Product>();
            List<string> productImages = new List<string>();
            foreach (var product in customerCart)
            {
                foreach (var item in products)
                {
                if(product.ProductId == item.Id)
                    {
                        Userproducts.Add(item);
                        productImages.Add(item.ImageFile);
                    }
                }
            }
            var options = new SessionCreateOptions
            {
                SuccessUrl = $"https://localhost:7182/CheckOut/OrderConfirmation",
                CancelUrl = $"https://localhost:7182/CheckOut/Login",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment"
            };
            List<string>image = new List<string>();
            foreach (var item in Userproducts)
            {
                image.Add(item.ImageFile);
                var sessionListItem = new SessionLineItemOptions
                {                           
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)item.Price * 100,
                        Currency = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Name,
                            Description = item.Description,
                            Images = productImages
                        }
                    },
                    Quantity = 1
                };
                options.LineItems.Add(sessionListItem);
            }
            var service = new SessionService();
            Session session = service.Create(options);
            Response.Headers.Add("Location", session.Url);
            List<Models.Product> userCarts = new List<Models.Product>();
            _context.SaveChanges();
            return new StatusCodeResult(303);
        }

        public IActionResult Login()
        {
            var orderId = Guid.Parse(HttpContext.Session.GetString("orderId"));
            var specificOrder = _context.OrderDetails.FirstOrDefault(x => x.OrderId == orderId);
            specificOrder.TransactionResponse = "failed";
            _context.SaveChanges();
            return View("Stripe");
        }

        public IActionResult Flutterwave(string email, decimal amount)
        {
            var allCarts = _context.Carts.ToList();
            var userId = Guid.Parse(HttpContext.Session.GetString("userId"));
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);
            var userCart = _context.Carts.Where(x => x.UserId == userId).ToList();
            var order = _context.Orders.FirstOrDefault(x => x.UserId == userId);
            var products = _context.Products.ToList();
            List<Models.Product> userCarts = new List<Models.Product>();
            decimal sum = 0;
            if (allCarts.Count != 0 || products.Count != 0)
            {
                List<decimal> price = new List<decimal>();
                foreach (var product in products)
                {
                    foreach (var cart in allCarts)
                    {
                        if (cart.UserId == userId)
                        {
                            if (cart.ProductId == product.Id)
                            {
                                price.Add(product.Price);
                            }
                        }
                    }
                }
                for (int i = 0; i < price.Count; i++)
                {
                    sum = sum + price[i];
                }
            }
            var userEmail = _context.Users.FirstOrDefault(x => x.Id == userId).Email;
            var txRef = Guid.NewGuid().ToString();
            var redirectUrl = Url.Action("PaymentCallback", "CheckOut", null, Request.Scheme);
            var flutterwaveService = new Flutterwave();
            var orderId = Guid.Parse(HttpContext.Session.GetString("orderId"));
            var specificOrder = _context.OrderDetails.FirstOrDefault(x => x.OrderId == orderId);
            specificOrder.PaymentOption = "Flutterwave";
            specificOrder.TransactionTime = DateTime.Now;
            var id = Guid.NewGuid();
            HttpContext.Session.SetString("orderId", id.ToString());
            var newOrder = new Models.Order
            {
                Id = id,
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Paid,
                AmountPaid = sum,
                PaymentDate = DateTime.Now,
                DeliveryDate = DateTime.Now.AddDays(3),
            };
            _context.Orders.Add(newOrder);
            _context.SaveChanges();
            foreach (var product in products)
            {
                foreach (var cart in userCart)
                {
                    if (product.Id == cart.ProductId)
                    {
                        var userOrder = _context.Orders.FirstOrDefault(x => x.UserId == userId);
                        var bill = _context.BillingDetails.ToList();
                        var orderDetail = new OrderDetail
                        {
                            OrderId = id,
                            ProductId = product.Id,
                            Quantity = 1,
                            UnitPrice = product.Price,
                            Price = product.Price * 1,
                            BillingDetailId = bill[bill.Count - 1].Id,
                            ShippingDetailId = bill[bill.Count - 1].Id,
                            PaymentOption = "FlutterWave",
                            TransactionResponse = "",
                            ResponseTime = DateTime.Now,
                            TransactionTime = DateTime.Now
                        };
                        _context.OrderDetails.Add(orderDetail);
                        _context.SaveChanges();
                    }
                }
            }
                _context.SaveChanges();
            var checkOutUrl = flutterwaveService.CreatePaymentIntent(sum, userEmail, "EUR", txRef, redirectUrl).Result;
            return Redirect(checkOutUrl);
        }

        public IActionResult PaymentCallback(string tx_ref, string status)
        {
            if (status == "successful")
            {

            }
            else
            {
                return View("error", "home");
            }
            return View();
        }


        public IActionResult PaystackView()
        {
            var allCarts = _context.Carts.ToList();
            var userId = Guid.Parse(HttpContext.Session.GetString("userId"));
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);
            var order = _context.Orders.FirstOrDefault(x => x.UserId == userId);
            var products = _context.Products.ToList();
            var userCart = _context.Carts.Where(x=> x.UserId == userId).ToList();
            var orderId = Guid.Parse(HttpContext.Session.GetString("orderId"));
            var specificOrder = _context.OrderDetails.FirstOrDefault(x=>x.OrderId == orderId);
            specificOrder.PaymentOption = "Paystack";
            specificOrder.TransactionTime = DateTime.Now;
            
            List<Models.Product> userCarts = new List<Models.Product>();
            decimal sum = 0;
            if (allCarts.Count != 0 || products.Count != 0)
            {
                List<decimal> price = new List<decimal>();
                foreach (var product in products)
                {
                    foreach (var cart in allCarts)
                    {
                        if (cart.UserId == userId)
                        {
                            if (cart.ProductId == product.Id)
                            {
                                price.Add(product.Price);
                            }
                        }
                    }
                }
                for (int i = 0; i < price.Count; i++)
                {
                    sum = sum + price[i];
                }
            }
            int amount = Convert.ToInt32(sum * 100) ;
            ViewBag.sum = amount;
            ViewBag.email = user.Email;
            ViewBag.name = user.LastName + " " + user.FirstName;
            var txRef = Guid.NewGuid().ToString();
            ViewBag.txRef = txRef;
            ViewBag.PaystackPublicKey = "pk_test_fcae7a331171bd9a75de577fca9ecf82e006f929";
            HttpContext.Session.SetString("txRef", txRef);

            var id = Guid.NewGuid();
            HttpContext.Session.SetString("orderId", id.ToString());
            var newOrder = new Models.Order
            {
                Id = id,
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Paid,
                AmountPaid = sum,
                PaymentDate = DateTime.Now,
                DeliveryDate = DateTime.Now.AddDays(3),
            };
            _context.Orders.Add(newOrder);
            foreach (var product in products)
            {
                foreach (var cart in userCart) {
                    if (product.Id == cart.ProductId)
                    {
                        var userOrder = _context.Orders.FirstOrDefault(x => x.UserId == userId);
                        var bill = _context.BillingDetails.ToList();
                        var orderDetail = new OrderDetail
                        {
                            OrderId = id,
                            ProductId = product.Id,
                            Quantity = 1,
                            UnitPrice = product.Price,
                            Price = product.Price * 1,
                            BillingDetailId = bill[bill.Count - 1].Id,
                            ShippingDetailId = bill[bill.Count - 1].Id,
                            PaymentOption = "",
                            TransactionResponse = "",
                            ResponseTime = DateTime.Now,
                            TransactionTime = DateTime.Now
                        };
                        _context.OrderDetails.Add(orderDetail);
                        _context.SaveChanges();
                    }
                }
            }
            _context.SaveChanges();
            _context.SaveChanges();
            return View();
        }


        public async Task<IActionResult> VerifyPayment(string reference)
        {
            if (string.IsNullOrEmpty(reference))
            {
                ViewBag.Message = "Invalid transaction reference.";
                return View();
            }

            // Paystack API endpoint for transaction verification
            string url = $"https://api.paystack.co/transaction/verify/{reference}";

            using (HttpClient client = new HttpClient())
            {
                // Set the secret key in the authorization header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "sk_test_09fcc7324b1fb8f097bcf64e7b5d6b07bf598f81");

                // Make a GET request to Paystack's verification endpoint
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    // Parse the response content
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<PaystackResponse>(jsonString);

                    var orderId = Guid.Parse(HttpContext.Session.GetString("orderId"));
                    var specificOrder = _context.OrderDetails.FirstOrDefault(x => x.OrderId == orderId);
                    specificOrder.ResponseTime = DateTime.Now;
                    if (jsonResponse != null && jsonResponse.status && jsonResponse.data.status == "success")
                    {
                        // Payment was successful, handle your logic here
                        specificOrder.TransactionResponse = "Successful";
                        ViewBag.Message = "Payment successful!";
                        _context.SaveChanges();
                        return View();
                    }
                    else
                    {
                        specificOrder.TransactionResponse = "Failed";
                        ViewBag.Message = "Payment verification failed.";
                        _context.SaveChanges();
                        return View();
                    }
                }
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ViewBag.Message = "Error verifying payment: " + errorContent;
                    _context.SaveChanges();
                    return View();
                }
                else
                {
                    // Log the exact error content for debugging
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ViewBag.Message = "Error verifying payment: " + errorContent;
                    _context.SaveChanges();
                    return View();
                }
                _context.SaveChanges();
            }
            _context.SaveChanges();
        }
    
    public class PaystackResponse
        {
            public bool status { get; set; }
            public Data data { get; set; }
        }

        public class Data
        {
            public string status { get; set; }
        }

        /*
                private async Task<dynamic> VerifyPayment(string reference)
                {
                    var verifyUrl = $"https://api.paystack.co/transaction/verify/{reference}";
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer YOUR_PAYSTACK_SECRET_KEY");

                    var response = await _httpClient.GetAsync(verifyUrl);

                    var responseString = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseString);

                    return jsonResponse.GetProperty("data");
                }*/


        public IActionResult OrderConfirmation()
        {
            var allCarts = _context.Carts.ToList();
            var userId = Guid.Parse(HttpContext.Session.GetString("userId"));
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);
            var order = _context.Orders.FirstOrDefault(x => x.UserId == userId);
            var products = _context.Products.ToList();
            List<Models.Product> userCarts = new List<Models.Product>();
            decimal sum = 0;
            var orderId = Guid.Parse(HttpContext.Session.GetString("orderId"));
            var specificOrder = _context.OrderDetails.FirstOrDefault(x => x.OrderId == orderId);
            specificOrder.TransactionResponse = "Succesful";
            specificOrder.ResponseTime = DateTime.Now;
            if (allCarts.Count != 0 || products.Count != 0)
            {
                List<decimal> price = new List<decimal>();
                foreach (var product in products)
                {
                    foreach (var cart in allCarts)
                    {
                        if (cart.UserId == userId)
                        {
                            if (cart.ProductId == product.Id)
                            {
                                price.Add(product.Price);
                            }
                        }
                    }
                }
                for (int i = 0; i < price.Count; i++)
                {
                    sum = sum + price[i];
                }
            }
            foreach (var cart in allCarts)
            {
                if (cart == _context.Carts.FirstOrDefault(x => x.UserId == userId))
                {
                    var product = _context.Products.FirstOrDefault(x => x.Id == cart.ProductId);
                    userCarts.Add(product);
                    _context.Carts.Remove(cart);
                    _context.SaveChanges();
                }
            }
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Testan", "Testan@Nuts.com"));
            message.To.Add(new MailboxAddress(" Test test test", user.Email));
            message.Subject = "Order Notification";
            var bodyBuilder = new StringBuilder();
            bodyBuilder.AppendLine(@$"  
                            <h2>Hi {user.FirstName}!</h2>
                            <img src='https://res.cloudinary.com/dsrgs17wp/image/upload/v1723824014/Screenshot_63_wdbwh2.png' alt='saturn' style='height:200px;'/>
                            <p> Welcome to ValeTestSite!</p>
                            <p> God bless! </p> 
                            <h3> Here Is Your Order:</h3>
                            <p>Order Date :{order.OrderDate}
                            <p>Order Status :{order.Status}
                            <p>Order Amount Paid: &#8358;{order.AmountPaid}
                            <p>Order Payment Date: {order.PaymentDate}
                            <p>Order Delivery Date: {order.DeliveryDate}");
            bodyBuilder.AppendLine(@"<table solid border='1px' style='border-collapse:collapse; text-align:center;'>
                                     <tr>
                                     <th>Name</th><th>Image</th><th>Price</th>
                                     </tr>");
            foreach (var prod in userCarts)
            {
                bodyBuilder.AppendLine($@"<tr>
                                     <td>{prod.Name}</td>
                                     <td><img src='{prod.ImageFile}' style='height:70px; text-align:ce' align='center'/></td>
                                     <td>&#8358;{prod.Price}</td>
                                     </tr>");
            }
            bodyBuilder.AppendLine("</table>");
            bodyBuilder.AppendLine($"Total: &#8358;{sum}");
            message.Body = new TextPart("html")
            {

                Text = bodyBuilder.ToString()
            };
            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                client.Authenticate("nwogohemmanuel@gmail.com", "nsccatpdkiaseepg");
                client.Send(message);
                client.Disconnect(true);
            }
            _context.SaveChanges();
            return View();
        }
        [HttpGet]
        public IActionResult PaymentOption()
        {
            var allCarts = _context.Carts.ToList();
            var userId = Guid.Parse(HttpContext.Session.GetString("userId"));
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);
            var order = _context.Orders.FirstOrDefault(x => x.UserId == userId);
            var products = _context.Products.ToList();
            List<Models.Product> userCarts = new List<Models.Product>();
            decimal sum = 0;
            if (allCarts.Count != 0 || products.Count != 0)
            {
                List<decimal> price = new List<decimal>();
                foreach (var product in products)
                {
                    foreach (var cart in allCarts)
                    {
                        if (cart.UserId == userId)
                        {
                            if (cart.ProductId == product.Id)
                            {
                                price.Add(product.Price);
                            }
                        }
                    }
                }
                for (int i = 0; i < price.Count; i++)
                {
                    sum = sum + price[i];
                }
            }
            ViewBag.sum = sum;
            ViewBag.email = user.Email;
            ViewBag.name = user.LastName + " " + user.FirstName;
            var txRef  = Guid.NewGuid().ToString();
            ViewBag.txref = txRef;
            Console.WriteLine(txRef);
            Console.WriteLine(txRef);
            Console.WriteLine(txRef);
            Console.WriteLine(txRef);
            Console.WriteLine(txRef);
            Console.WriteLine(txRef);
            Console.WriteLine(txRef);
            Console.WriteLine(txRef);
            Console.WriteLine(txRef);
            Console.WriteLine(txRef);
            Console.WriteLine(txRef);
            Console.WriteLine(txRef);
            Console.WriteLine(txRef);
            Console.WriteLine(txRef);
            Console.WriteLine(txRef);
            Console.WriteLine(txRef);
            Console.WriteLine("tttttttttttttttttxRef");
            Console.WriteLine("tttttttttttttttttxRef");
            Console.WriteLine("tttttttttttttttttxRef");
            Console.WriteLine("tttttttttttttttttxRef");
            Console.WriteLine("tttttttttttttttttxRef");
            Console.WriteLine("tttttttttttttttttxRef");
            Console.WriteLine("tttttttttttttttttxRef");
            Console.WriteLine("tttttttttttttttttxRef");
            Console.WriteLine("tttttttttttttttttxRef");
            Console.WriteLine("tttttttttttttttttxRef");
            Console.WriteLine("tttttttttttttttttxRef");
            Console.WriteLine("tttttttttttttttttxRef");
            return View();
        }

    }
}
