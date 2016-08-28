using RecensionsProjekt.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecensionsProjekt.Models
{
    public class AllReviewViewModel
    {
        public Guid ReviewID { get; set; }
        public Guid CreatorUserID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CreatedDate { get; set; }
        public int UserRating { get; set; }
        public string TypeOfReviewValue { get; set; }
        public string CreatedByName { get; set; }
        public int Likes { get; set; }
        public int DisLikes { get; set; }
        public decimal TotalRating { get; set; }
        public IEnumerable<SelectListItem> RatingValues
        {
            get
            {
                return new[]
                {
                new SelectListItem { Value = "1", Text = "En stjärna" },
                new SelectListItem { Value = "2", Text = "Två stjärnor" },
                new SelectListItem { Value = "3", Text = "Tre stjärnor" },
                new SelectListItem { Value = "4", Text = "Fyra stjärnor" },
                new SelectListItem { Value = "5", Text = "Fem stjärnor" }
            };
            }
        }

        [Required(ErrorMessage = "Du måste skriva något i kommentars-fältet!")]
        [StringLength(maximumLength: 150, MinimumLength = 3)]
        public string CommentToAdd { get; set; }
        public List<CommentToReview> CommentToReviewList { get; set; }

        public AllReviewViewModel()
        {
            CommentToReviewList = new List<CommentToReview>();
        }
        
        public string ReviewImagePath { get; set; }

        public bool HasProfilePicture { get; set; }
    }
}