using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using SendGrid;
using SendGrid.Helpers.Mail;
using static System.Reflection.Metadata.BlobBuilder;
using System.Text;
using WebApplication1.Services;
using WebApplication1.Interface;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _context;
        private readonly Cloudinary _cloudinary;
        private readonly IPaymentGateway _paymentService;
        public HomeController(ILogger<HomeController> logger, DataContext context, Cloudinary cloudinary, PaymentService paymentService)
        {
            _cloudinary = cloudinary;
            _context = context;
            _logger = logger;
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(long amount)
        {
            var charge = await _paymentService.CreatePaymentIntent( amount, "test@test.com", "sbkad");
            ViewBag.ClientSecret = charge;
            return View("PaymentForm");
        }
        [HttpPost]

        public async Task<IActionResult> AddProduct(ProductViewModel product)
        {

            if (product.File != null)
            {

                var uploadResult = new ImageUploadResult();

                using (var stream = product.File.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(product.File.FileName, stream)
                    };
                    uploadResult = await _cloudinary.UploadAsync(uploadParams);
                }
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var category = await _context.Categories.FirstOrDefaultAsync(x => x.Name == product.CategoryName);
                    var cId = category.Id;
                    var Newproduct = new Product
                    {
                        Name = product.Name,
                        Description = product.Description,
                        CategoryId = cId,
                        Price = product.Price,
                        ImageFile = uploadResult.Url.ToString()
                    };

                    await _context.Products.AddAsync(Newproduct);
                    await _context.SaveChangesAsync();
                    return View("Success");
                }
                return StatusCode((int)uploadResult.StatusCode, uploadResult.Error?.Message);
            }
            return View("Error");
        }

        public IActionResult Index()
        {
            var products = _context.Products.ToList();
            var Bmd = _context.Products.Include(x => x.Category).ToList();
            var categories = _context.Categories.ToList();
            var carts = _context.Carts.ToList();
            var sessionId = HttpContext.Session.GetString("sessionId");
            ViewData["sessionId"] = sessionId;
            ViewBag.userId = HttpContext.Session.GetString("userId");
            if (ViewBag.userId != null)
            {
                if (carts.Count != 0 || products.Count != 0)
                {
                    var userId = HttpContext.Session.GetString("userId");
                    List<decimal> price = new List<decimal>();
                    foreach (var product in products)
                    {
                        foreach (var cart in carts)
                        {
                            if (cart.UserId == Guid.Parse(userId))
                            {
                                if (cart.ProductId == product.Id)
                                {
                                    price.Add(product.Price);
                                }
                            }
                        }
                    }
                    decimal sum = 0;
                    for (int i = 0; i < price.Count; i++)
                    {
                        sum = sum + price[i];
                    }
                    ViewBag.TotalSum = sum;
                    ViewBag.Quantity = price.Count;
                }
            }
            var view = new UserProductsCategoriesCartsViewModel
            {
                Carts = carts,
                Categories = categories,
                Products = Bmd
            };
            return View(view);
        }
        [HttpGet]
        public IActionResult Register()
        {
            var categories = _context.Categories.ToList();
            var view = new UserCategoriesViewModel
            {
                Categories = categories,
            };
            return View(view);
        }
        [HttpPost]
        public async Task<IActionResult> Register(UserCategoriesViewModel userViewModel)
        {
            var user = new User
            {
                FirstName = userViewModel.User.FirstName,
                LastName = userViewModel.User.LastName,
                UserName = userViewModel.User.UserName,
                Email = userViewModel.User.Email,
                Password = userViewModel.User.Password,
            };
            if (userViewModel.User.Password != userViewModel.User.ConfirmPassword)
            {
                return View("Error");
            }
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Testan", "Testan@Nuts.com"));
            message.To.Add(new MailboxAddress(" Test test test", userViewModel.User.Email));
            message.Subject = "Registeration Notification";
            var bodyContent = @$"  
                            <h2>Hi {userViewModel.User.FirstName}!</h2>
                            <p> Welcome to ValeTestSite!</p>
                            <p> God bless! </p> 
                            <img src='https://res.cloudinary.com/dsrgs17wp/image/upload/v1723824014/Screenshot_63_wdbwh2.png' alt='saturn' style='width:300px;'/> ";
            message.Body = new TextPart("html")
            {

                Text = bodyContent
            };
            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                client.Authenticate("nwogohemmanuel@gmail.com", "nsccatpdkiaseepg");
                client.Send(message);
                client.Disconnect(true);
            }
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return View("Success");
        }
        [HttpGet]
        public IActionResult Login()
        {
            var categories = _context.Categories.ToList();
            var view = new UserCategoriesViewModel
            {
                Categories = categories,
            };
            return View(view);
        }
        public IActionResult Login(UserCategoriesViewModel userViewModel)
        {
            var get = _context.Users.FirstOrDefault(x => x.Email == userViewModel.User.Email && x.Password == userViewModel.User.Password);
            if (get == null)
            {
                return View("Error");
            }/*
            var emailService = new EmailService();
            emailService.SendEmail("recipient-email@example.com", "Test Subject", "This is a test email sent using SendGrid API.");*/
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Testan", "Testan@Nuts.com"));
            message.To.Add(new MailboxAddress(" Test test test", get.Email));
            message.Subject = "Login Notification";
            message.Body = new TextPart("plain")
            {

                Text = $"You logged in at {DateTime.Now}\nHave a nice day!"
            };
            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                client.Authenticate("nwogohemmanuel@gmail.com", "nsccatpdkiaseepg");
                client.Send(message);
                client.Disconnect(true);
            }
            HttpContext.Session.SetString("sessionId", Guid.NewGuid().ToString());
            HttpContext.Session.SetString("userId", get.Id.ToString());
            var userId = HttpContext.Session.GetString("userId");
            ViewData["userId"] = userId;
            return RedirectToAction("index");
        }
        [HttpGet]
        public IActionResult AddCategory()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddCategory(CategoryViewModel cat)
        {
            var parent = await _context.Categories.FirstOrDefaultAsync(x => x.Name == cat.ParentName);
            if (parent != null)
            {
                var pId = parent.Id;
                var category = new Category
                {
                    Name = cat.Name,
                    Description = cat.Description,
                    ParentId = pId,
                };
                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();

            }
            if (parent == null)
            {
                var category = new Category
                {
                    Name = cat.Name,
                    Description = cat.Description,
                    ParentId = null,
                };
                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();

            }
            return View("Success");
        }
        [HttpGet]
        public IActionResult AddProduct()
        {
            var Categories = _context.Categories.ToList();
            var view = new CompositeViewModel
            {
                Category = Categories
            };
            return View(view);
        }
        public IActionResult ShopList()
        {
            var sessionId = HttpContext.Session.GetString("sessionId");
            ViewData["sessionId"] = sessionId;
            var products = _context.Products.ToList();
            var categories = _context.Categories.ToList();
            var carts = _context.Carts.ToList();

            ViewBag.userId = HttpContext.Session.GetString("userId");
            if (ViewBag.userId != null)
            {
                if (carts.Count != 0 || products.Count != 0)
                {
                    var userId = HttpContext.Session.GetString("userId");
                    List<decimal> price = new List<decimal>();
                    foreach (var product in products)
                    {
                        foreach (var cart in carts)
                        {
                            if (cart.UserId == Guid.Parse(userId))
                            {
                                if (cart.ProductId == product.Id)
                                {
                                    price.Add(product.Price);
                                }
                            }
                        }
                    }
                    decimal sum = 0;
                    for (int i = 0; i < price.Count; i++)
                    {
                        sum = sum + price[i];
                    }
                    ViewBag.TotalSum = sum;
                    ViewBag.Quantity = price.Count;
                }
            }
            var view = new UserProductsCategoriesCartsViewModel
            {
                Carts = carts,
                Categories = categories,
                Products = products
            };
            return View(view);

        }
        public IActionResult SingleProduct(Guid Id)
        {
            var products = _context.Products.ToList();
            var singleProduct = _context.Products.FirstOrDefault(x =>  x.Id == Id);
            var image = singleProduct.ImageFile;
            ViewBag.ProductId = Id;
            ViewBag.Image = image;
            var Bmd = _context.Products.Include(x => x.Category).ToList();
            var categories = _context.Categories.ToList();
            var carts = _context.Carts.ToList();
            var sessionId = HttpContext.Session.GetString("sessionId");
            ViewData["sessionId"] = sessionId;
            ViewBag.userId = HttpContext.Session.GetString("userId");
            if (ViewBag.userId != null)
            {
                if (carts.Count != 0 || products.Count != 0)
                {
                    var userId = HttpContext.Session.GetString("userId");
                    List<decimal> price = new List<decimal>();
                    foreach (var product in products)
                    {
                        foreach (var cart in carts)
                        {
                            if (cart.UserId == Guid.Parse(userId))
                            {
                                if (cart.ProductId == product.Id)
                                {
                                    price.Add(product.Price);
                                }
                            }
                        }
                    }
                    decimal sum = 0;
                    for (int i = 0; i < price.Count; i++)
                    {
                        sum = sum + price[i];
                    }
                    ViewBag.TotalSum = sum;
                    ViewBag.Quantity = price.Count;
                }
            }
            var view = new UserProductsCategoriesCartsViewModel
            {
                Carts = carts,
                Categories = categories,
                Products = Bmd,
                Product = singleProduct
            };
            return View(view);

        }
    
        [HttpPost]
        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult ShoppingCart()
        {
            var products = _context.Products.ToList();
            var categories = _context.Categories.ToList();
            var carts = _context.Carts.ToList();
            ViewData["sessionId"] = HttpContext.Session.GetString("sessionId");
            if (HttpContext.Session.GetString("sessionId") == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.userId = HttpContext.Session.GetString("userId");
            if (ViewBag.userId != null)
            {
                if (carts.Count != 0 || products.Count != 0)
                {
                    var userId = HttpContext.Session.GetString("userId");
                    List<decimal> price = new List<decimal>();
                    foreach (var product in products)
                    {
                        foreach (var cart in carts)
                        {
                            if (cart.UserId == Guid.Parse(userId))
                            {
                                if (cart.ProductId == product.Id)
                                {
                                    price.Add(product.Price);
                                }
                            }
                        }
                    }
                    decimal sum = 0;
                    for (int i = 0; i < price.Count; i++)
                    {
                        sum = sum + price[i];
                    }
                    ViewBag.TotalSum = sum;
                    ViewBag.Quantity = price.Count;
                }
            }
            var view = new UserProductsCategoriesCartsViewModel
            {
                Carts = carts,
                Categories = categories,
                Products = products
            };
            return View(view);
        }
        [HttpGet]
        public IActionResult AddState()
        {
            var countries = _context.Countries.ToList();
            var view = new CountryStateViewModel
            {
                Countries = countries,
            };
            return View(view);
        }
        [HttpPost]
        public async Task<IActionResult> AddState(CountryStateViewModel item)
        {
           var country = await _context.Countries.FirstOrDefaultAsync(x => x.Name == item.State.CountryName);
            if(item.State.CountryName == null)
            {
                return View("Error");
            }
           var countryId = country.Id;
           var state = new State
           {
                CountryId = countryId,
                Name = item.State.StateName,
                Zip = item.State.ZipCode
           };
           await _context.states.AddAsync(state);
           await _context.SaveChangesAsync();
           return View();
        }
        [HttpGet]
        public IActionResult AddCountry()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddCountry(Country model)
        {
            var country = new Country
            {
                Name = model.Name
            };
            _context.Countries.Add(country);
            _context.SaveChanges();
            return View();
        }
        public IActionResult AddToCart(Guid productId)
        {
            var get = _context.Products.FirstOrDefault(x => x.Id == productId);
            if (HttpContext.Session.GetString("sessionId") == null)
            {
                return RedirectToAction("Login");
            }
            var sessionId = Guid.Parse(HttpContext.Session.GetString("sessionId"));
            var userId = Guid.Parse(HttpContext.Session.GetString("userId"));
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);
            var add = new Cart
            {
                UserId = userId,
                SessionId = sessionId,
                ProductId = productId,
                CreatedDate = DateTime.Now,
                Quantity = 1
            };
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Testan", "Testan@Nuts.com"));
            message.To.Add(new MailboxAddress(" Test test test", user.Email));
            message.Subject = "Cart Notification";
            var bodyContent = @$"  
                            <h2>Hi {user.FirstName}!</h2>
                            <img src='https://res.cloudinary.com/dsrgs17wp/image/upload/v1723824014/Screenshot_63_wdbwh2.png' alt='saturn' style='height:200px;'/>
                            <p> Welcome to ValeTestSite!</p>
                            <p> God bless! </p> 
                            <img src='{get.ImageFile}' alt='productImage' style='height:200px;' />
                            <h4> You just added {get.Name} to your cart and this costs {get.Price} </h4>";
            message.Body = new TextPart("html")
            {

                Text = bodyContent
            };
            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                client.Authenticate("nwogohemmanuel@gmail.com", "nsccatpdkiaseepg");
                client.Send(message);
                client.Disconnect(true);
            }
            _context.Carts.Add(add);
            _context.SaveChanges();
            return RedirectToAction("shopList");
        }/*
        public IActionResult QuantityPrice(UserProductsCategoriesCartsViewModel variable)
        {
            decimal sum = 0;
            var userId = Guid.Parse(HttpContext.Session.GetString("userId"));
            if(variable.Cart.UserId == userId)
            {
                if(variable.Cart.ProductId == variable.Product.Id)
                {
                    sum = sum + (variable.Product.Price * variable.Cart.Quantity);
                }
            }
            ViewBag.TotalSum = sum;

        }*/
        public IActionResult CheckOut()
        {

            var products = _context.Products.ToList();
            var cartProducts = _context.Carts.Include(x => x.Product).ToList();
            var categories = _context.Categories.ToList();
            var carts = _context.Carts.ToList();
            var sessionId = HttpContext.Session.GetString("sessionId");
            ViewData["sessionId"] = sessionId;
            ViewBag.userId = HttpContext.Session.GetString("userId");
            if (ViewBag.userId != null)
            {
                if (carts.Count != 0 || products.Count != 0)
                {
                    var userId = HttpContext.Session.GetString("userId");
                    List<decimal> price = new List<decimal>();
                    foreach (var product in products)
                    {
                        foreach (var cart in carts)
                        {
                            if (cart.UserId == Guid.Parse(userId))
                            {
                                if (cart.ProductId == product.Id)
                                {
                                    price.Add(product.Price);
                                }
                            }
                        }
                    }
                    decimal sum = 0;
                    for (int i = 0; i < price.Count; i++)
                    {
                        sum = sum + price[i];
                    }
                    ViewBag.TotalSum = sum;
                    ViewBag.Quantity = price.Count;
                }
            }
            var view = new BillingDetailsUserViewModel
            {
                Carts = cartProducts,
                Categories = categories,
                Products = products
            };
            return View(view);

        }
        public IActionResult BillingDetails(BillingDetailsViewModel billing)
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("userId"));
            var state = _context.states.FirstOrDefault(x => x.Name == billing.State);
            var billings = new BillingDetail
            {
                UserId = userId,
                CompanyName = billing.Company,
                Phone = billing.Phone,
                Address = billing.Address,
                City = billing.City,
                StateId = state.Id,
                IsActive = true
            };
            _context.BillingDetails.Add(billings);
            _context.SaveChanges();
            return RedirectToAction("index");
        }
        public IActionResult Order()
        {

            var userId = Guid.Parse(HttpContext.Session.GetString("userId"));
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);
            var billingDetails = _context.BillingDetails.ToList();
            var products = _context.Products.ToList();
            var cartProducts = _context.Carts.Include(x => x.Product).ToList();
            var categories = _context.Categories.ToList();
            var carts = _context.Carts.ToList();
            decimal sum = 0 ;
            if (carts.Count != 0 || products.Count != 0)
            {
                List<decimal> price = new List<decimal>();
                foreach (var product in products)
                {
                    foreach (var cart in carts)
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
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Paid,
                AmountPaid = sum,
                PaymentDate = DateTime.Now,
                DeliveryDate = DateTime.Now.AddDays(3),
            };
            _context.Orders.Add(order);
            foreach (var product in products)
            {
                var userOrder = _context.Orders.FirstOrDefault(x => x.UserId == userId);
                var bill = _context.BillingDetails.ToList();
                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = product.Id,
                    Quantity = 1,
                    UnitPrice = product.Price,
                    Price = product.Price * 1,
                    BillingDetailId = bill[bill.Count - 1].Id,
                    ShippingDetailId = bill[bill.Count - 1].Id
                };
                _context.OrderDetails.Add(orderDetail);
                _context.SaveChanges();
            }
            _context.SaveChanges();
            return RedirectToAction($"index", "CheckOut");
        }
        public IActionResult ClearCart()
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("userId"));
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);
            var carts = _context.Carts.ToList();
            foreach(var cart in carts)
            {
                if (cart.UserId == userId)
                {
                    _context.Carts.Remove(cart);
                }
                _context.SaveChanges();
            }
            return RedirectToAction("index");
        }
        [HttpGet]
        public IActionResult AddPromo()
        {
            var viewModel = new PromoProductViewModel
            {
                Products = _context.Products.ToList()
            };
            if(viewModel.Products == null || !viewModel.Products.Any())
            {
                Console.WriteLine("eRRORRRR");
                Console.WriteLine("eRRORRRR");
                Console.WriteLine("eRRORRRR");
                Console.WriteLine("eRRORRRR");
                Console.WriteLine("eRRORRRR");
                Console.WriteLine("eRRORRRR");
                Console.WriteLine("eRRORRRR");
                Console.WriteLine("eRRORRRR");
                Console.WriteLine("eRRORRRR");
                Console.WriteLine("eRRORRRR");
                Console.WriteLine("eRRORRRR");
            }
            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> AddPromo(PromoProductViewModel item)
        {
            Console.WriteLine($"{item.Promo.ProductName}");
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Name == item.Promo.ProductName);
            var duration = DateTime.Now.AddDays(7);
            var running = true;
            if(DateTime.Now >  duration)
            {
                running = false;
            }
            var promo = new Promo
            {
                Name = item.Promo.Name,
                ProductId = product.Id,
                CreatedDate= DateTime.Now,
                IsRunning = running
            };
            await _context.Promos.AddAsync(promo);
            await _context.SaveChangesAsync();
            return View();
        }
        public IActionResult Success()
        {
            var categories = _context.Categories.ToList();
            var products = _context.Products.ToList();
            var view = new MultipleViewModel
            {
                Products = products,
                Categories = categories,
            };
            return View(view);
        }
        public IActionResult Privacy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Uploader()
        {
            return View("Success");
        }
        public class EmailService
        {
            private readonly string apiKey = "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";
            public async Task SendEmailAsync(string toEmail, string subject, string message)
            {
                var client =  new SendGridClient(apiKey);
                var from = new EmailAddress("nwogohe@gmail.com", "commerce");
                var to = new EmailAddress(toEmail);
                var plainTextContext = message;
                var htmlContent = $"<p>{message}</p>";
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContext, htmlContent);

                var response = await client.SendEmailAsync(msg);

                Console.WriteLine(response.StatusCode);
            }
        }

    }
}
