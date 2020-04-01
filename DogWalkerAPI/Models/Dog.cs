using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DogWalkerAPI.Models
{
    public class Dog
    {
        public int Id { get; set; }
        [Required]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Dog name must be between 2 and 25 characters")]
        public string Name { get; set; }
        [Required]
        public int OwnerId { get; set; }
        public string Breed { get; set; }
        public string Notes { get; set; }
        public Owner Owner { get; set; }

    }
}

