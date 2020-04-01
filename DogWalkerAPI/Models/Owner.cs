using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DogWalkerAPI.Models
{
    public class Owner
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Owner name must be between 2 and 25 characters")]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        [StringLength(14, MinimumLength = 10, ErrorMessage = "Phone number must be 10 characters, example: 555-555-5555")]
        [RegularExpression("^[01]?[- .]?\\(?[2-9]\\d{2}\\)?[- .]?\\d{3}[- .]?\\d{4}$",
        ErrorMessage = "Phone is required and must be properly formatted, accepts only digits and dashes.")]
        public string Phone { get; set; }
        public int NeighborhoodId { get; set; }
        public Neighborhood Neighborhood { get; set; }
        public List<Dog> Dogs { get; set; }
    }
}
