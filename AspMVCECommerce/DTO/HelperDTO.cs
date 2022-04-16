using AspMVCECommerce.Models;
using System;

namespace AspMVCECommerce.DTO
{
    public static class HelperDTO
    {


        public static LineItemDTO ToLineItemDTO(this LineItem lineItem)
        {
            var lineItemDTO = new LineItemDTO()
            {
                 ColorId = lineItem.ColorId,
                  LineItemId = lineItem.LineItemId,
                   ProductId = lineItem.ProductId,
                    ShoppingCartId = lineItem.ShoppingCartId,
                     Quantity = lineItem.Quantity,
                      SizeId = lineItem.SizeId,
                       TotalPrice = lineItem.TotalPrice
            };

            return lineItemDTO;
        }


        public static LineItem ToLineItem(this LineItemDTO lineItemDTO)
        {
            var lineItem = new LineItem()
            {
                ColorId = lineItemDTO.ColorId,
                LineItemId = lineItemDTO.LineItemId,
                ProductId = lineItemDTO.ProductId,
                ShoppingCartId = lineItemDTO.ShoppingCartId,
                Quantity = lineItemDTO.Quantity,
                SizeId = lineItemDTO.SizeId,
                TotalPrice = lineItemDTO.TotalPrice
            };

            return lineItem;
        }



        public static ReviewDTO ToReviewDTO(this Review review)
        {
            var reviewDTO = new ReviewDTO()
            {
                ReviewId = review.ReviewId,
                Name = review.Name,
                Description = review.Description,
                Email = review.Email,
                ProductId = review.ProductId,
                Rating = review.Rating,
                Created = review.Created.ToDateTimeString()
            };

            return reviewDTO;
        }

        public static Review ToReview(this ReviewDTO reviewDTO)
        {
            var created = new DateTime();

            if (string.IsNullOrEmpty(reviewDTO.Created))
            {
                created = DateTime.Now;
            }

            var review = new Review()
            {
                ReviewId = reviewDTO.ReviewId,
                Name = reviewDTO.Name,
                Description = reviewDTO.Description,
                Email = reviewDTO.Email,
                ProductId = reviewDTO.ProductId,
                Rating = reviewDTO.Rating,
                Created = created
            };

            return review;
        }

        public static string ToAverageRatingString(this double ar)
        {
            string formattedAR = "";
            if (ar.ToString().IndexOf(".") > -1)
            {
                formattedAR = ar.ToString("0.00").Remove(ar.ToString("0.00").Length - 1, 1);
            }
            else
            {
                formattedAR = ar.ToString();
            }
            return formattedAR;
        }

        private static string ToDateTimeString(this DateTime dateTime)
        {
            return ((DateTime)dateTime).ToString("dd MMM yyyy, h:mm tt");
        }
    }
}