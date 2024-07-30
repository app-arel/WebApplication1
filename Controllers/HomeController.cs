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

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _context;
        private readonly Cloudinary _cloudinary;
        public HomeController(ILogger<HomeController> logger, DataContext context, Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
            _context = context;
            _logger = logger;
        }

        /*
                public CloudinaryService(IConfiguration config)
                {
                    var account = new Account(
                        config["CloudinarySettings:dsrgs17wp"],
                        config["CloudinarySettings:174826148423816"],
                        config["CloudinarySettings:HGpO4T2megoKT1sZgZS3aNLeb1Q"]
                    );

                    _cloudinary = new Cloudinary(account);
                }*/
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
            var categories = _context.Categories.ToList();
            var products = _context.Products.ToList(); 
            var view = new CompositeViewModel
            {
                Category = categories,
            };
            return View(view);
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Registerer(UserViewModel userViewModel)
        {
            var user = new User
            {
                FirstName = userViewModel.FirstName,
                LastName = userViewModel.LastName,
                Email = userViewModel.Email,
                Password = userViewModel.Password,
            };
            if(userViewModel.Password != userViewModel.ConfirmPassword)
            {
                return View("Error");
            }
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return View("Success");
        }
        [HttpGet]public IActionResult Login()
        {
            return View();
        }
        public IActionResult Loginer(UserViewModel userViewModel)
        {
            var get = _context.Users.FirstOrDefaultAsync(x => x.Email == userViewModel.Email && x.Password == userViewModel.Password) ;
            if (get == null)
            {
                return View("Error");
            }
            return View("Success");
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
            var products = _context.Products.ToList();
            return View(products);
        }
        public IActionResult SingleProduct()
        {
            return View();
        }
        public IActionResult ShoppingCart()
        {
            return View();
        }
        public IActionResult CheckOut()
        {
            return View();
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

}
}
