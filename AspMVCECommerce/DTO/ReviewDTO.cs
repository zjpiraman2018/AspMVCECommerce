using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AspMVCECommerce.DTO
{
    public class ReviewDTO
    {
        public int ReviewId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; }
        public string Created { get; set; }
        public int ProductId { get; set; }
    }
}