using AspMVCECommerce.Models;
using System.Collections.Generic;

namespace AspMVCECommerce.ViewModel
{
    public class OrderDetailViewModel
    {
        public OrderDetailViewModel()
        {
            LineItems = new List<LineItem>();
        }

        public Order Order { get; set; }
        public CheckOut CheckOut { get; set; }
        public List<LineItem> LineItems { get; set; }
    }
}