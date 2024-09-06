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

namespace WebApplication1.Controllers
{
    public class CheckOutController : Controller
    {
        private readonly DataContext _context;
        public CheckOutController(DataContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var allCarts = _context.Carts.ToList();
            var userId = Guid.Parse(HttpContext.Session.GetString("userId"));
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);
            var order = _context.Orders.FirstOrDefault(x => x.UserId == userId);
            var customerCart = _context.Carts.Where(x => x.User.Id == userId).ToList();
            var products = _context.Products.ToList();
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






        public IActionResult Flutterwave(string email, decimal amount)
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
            var userEmail = _context.Users.FirstOrDefault(x => x.Id == userId).Email;
            var txRef = Guid.NewGuid().ToString();
            var redirecrUrl = Url.Action("PaymentCallback", "CheckOut", null, Request.Scheme);
            var flutterwaveService = new Flutterwave(new HttpClient());
            var checkOutUrl = flutterwaveService.CreatePaymentIntent(sum, email, "USD", txRef, redirecrUrl).Result;
            return View();
        }




        public IActionResult PayStack()
        {
            return View();
        }



        public IActionResult OrderConfirmation()
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
            return View();
        }

    }
}
