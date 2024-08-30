using WebApplication1.Models;

namespace WebApplication1.ViewModel
{
    public class CountryStateViewModel
    {
        public StateViewModel State { get; set; }
        public IEnumerable<Country> Countries { get; set; }
    }
}
