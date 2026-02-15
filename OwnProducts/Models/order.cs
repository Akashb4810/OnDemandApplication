using System.ComponentModel.DataAnnotations;

namespace OwnProducts.Models
{
    public class Order
    {
        [Required]
        public string Name { get; set; }

        [Required, Phone]
        public string Mobile { get; set; }

        public string Product { get; set; }

        [Required]
        public int Quantity { get; set; }

        public string Address { get; set; }
    }
}
