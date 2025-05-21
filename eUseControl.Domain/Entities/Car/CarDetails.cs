using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eUseControl.Domain.Entities.Car
{
    public class CarDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; } = 0.0m;
        public string ImageUrl { get; set; }
        public int Stock { get; set; } = 1;
    }
}
