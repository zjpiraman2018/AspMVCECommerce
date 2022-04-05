﻿using AspMVCECommerce.Models;
using System;
using System.Globalization;
using System.Net;

namespace AspMVCECommerce.ViewModel
{

    public static class HelperViewModel
    {
        public static Product ToProduct(this ProductViewModel productViewModel)
        {
            DateTime? promoSaleStartDateTime = null; 
            DateTime? promoSaleEndDateTime = null;

            if (productViewModel.PromoSaleOFF != null)
            {
                if(Double.Parse(productViewModel.PromoSaleOFF.ToString()) > 0 && Double.Parse(productViewModel.PromoSaleOFF.ToString()) <= 1)
                {
                    promoSaleStartDateTime = productViewModel.PromoSaleStartDateTime.ToDateTime();
                    promoSaleEndDateTime = productViewModel.PromoSaleEndDateTime.ToDateTime();
                }

            }
            var product = new Product()
            {
                ProductId = productViewModel.ProductId,
                CategoryId = productViewModel.CategoryId,
                BrandId = productViewModel.BrandId,
                Description = WebUtility.HtmlEncode(productViewModel.Description),
                Details = WebUtility.HtmlEncode(productViewModel.Details),
                DiscountedPrice = productViewModel.DiscountedPrice,
                Name = productViewModel.Name,
                OriginalPrice = productViewModel.OriginalPrice,
                Stock = productViewModel.Stock,
                PromoSaleOFF = productViewModel.PromoSaleOFF != null ? Double.Parse(productViewModel.PromoSaleOFF.ToString()) : 0,
                PromoSaleStartDateTime = promoSaleStartDateTime,
                PromoSaleEndDateTime = promoSaleEndDateTime,
            };

            return product;
        }

        public static ProductViewModel ToProductViewModel(this Product product)
        {
            var productViewModel = new ProductViewModel()
            {
                ProductId = product.ProductId,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                Description = WebUtility.HtmlDecode(product.Description),
                Details = WebUtility.HtmlDecode(product.Details),
                DiscountedPrice = product.DiscountedPrice,
                Name = product.Name,
                OriginalPrice = product.OriginalPrice,
                Stock = product.Stock,
                PromoSaleOFF = product.PromoSaleOFF,
                PromoSaleStartDateTime = product.PromoSaleStartDateTime != null ? product.PromoSaleStartDateTime.ToDateTimeString() : "", 
                PromoSaleEndDateTime = product.PromoSaleEndDateTime != null ? product.PromoSaleEndDateTime.ToDateTimeString() : "",

            };

            return productViewModel;
        }

        public static Image ToImage(this ImageViewModel imageViewModel)
        {
            var image = new Image()
            {
                ImageId = imageViewModel.ImageId != null ? Int32.Parse(imageViewModel.ImageId.ToString()) : 0,
                ProductId = imageViewModel.ProductId != null ? Int32.Parse(imageViewModel.ProductId.ToString()) : 0,
                Default = imageViewModel.Default != null ? (imageViewModel.Default.ToString() == "True" ? true : false) : false,
                ImagePath = imageViewModel.ImagePath,
                Title = imageViewModel.Title
            };

            return image;
        }


        public static ImageViewModel ToImageViewModel(this Image image)
        {
            var imageViewModel = new ImageViewModel()
            {
                ImageId = image.ImageId,
                ProductId = image.ProductId,
                Default = image.Default,
                ImagePath = image.ImagePath,
                Title = image.Title
            };

            return imageViewModel;
        }

        public static string ToDateTimeString(this DateTime? dateTime)
        {
            return ((DateTime)dateTime).ToString("MM/dd/yyyy h:mm tt");
        }

        public static DateTime ToDateTime(this string dateTime)
        {
            CultureInfo enUS = new CultureInfo("en-US");
            //string dateString;
            DateTime dateValue;

            // Parse date with no style flags.
            //dateString = " 5/01/2009 8:30 AM";
            try
            {
                dateValue = DateTime.ParseExact(dateTime, "g", enUS, DateTimeStyles.None);

            }
            catch (FormatException)
            {
                dateValue = new DateTime();
            }

            return dateValue;
        }
    }
}