﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DogWalkerAPI.Models
{
    public class Owner
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int NeighborhoodId { get; set; }
        public Neighborhood Neighborhood { get; set; }

        public List<Dog> Dogs { get; set; }
    }
}
