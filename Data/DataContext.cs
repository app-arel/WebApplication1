using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<BillingDetail> BillingDetails { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductPromo> ProductPromos { get; set; }
        public DbSet<Promo> Promos { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<State> states { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
