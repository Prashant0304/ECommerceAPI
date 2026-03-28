using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTOs
{
    public class ProductCreateDto
    {
        [Required(ErrorMessage ="Product Name is Required")]
        [StringLength(100,ErrorMessage ="Name cannot exceed 100 Characters")]
        public string? Name { get; set; }

        [Required(ErrorMessage ="Description is required")]
        [StringLength(500,ErrorMessage ="Desciption cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage ="Price Is required")]
        [Range(0.1,100000,ErrorMessage ="Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(0,10000,ErrorMessage ="Stock cannot be negative")]
        public int Stock { get; set; }
        [Required(ErrorMessage ="Category is required")]
        public int CategoryId { get; set; }
    }
}
