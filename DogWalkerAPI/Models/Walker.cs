using System;
using System.Collections.Generic;
using System.Text;

namespace DogWalkerAPI.Models
{
    class Walker
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int NeighborhoodId { get; set; }

        public Neighborhood Neighborhood { get; set; }
    }
}
