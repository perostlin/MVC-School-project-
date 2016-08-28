using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecensionsProjekt.Models
{
    public class OtherReviewViewModel
    {
        public Guid ReviewID { get; set; }

        [Required(ErrorMessage = "Måste ange en titel!")]
        [StringLength(maximumLength: 50, MinimumLength = 1)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Måste ge en beskrivning!")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Måste sätta ett betyg!")]
        public int UserRating { get; set; }
        public IEnumerable<SelectListItem> RatingValues
        {
            get
            {
                return new[]
                {
                new SelectListItem { Value = "1", Text = "En stjärnor" },
                new SelectListItem { Value = "2", Text = "Två stjärnor" },
                new SelectListItem { Value = "3", Text = "Tre stjärnor" },
                new SelectListItem { Value = "4", Text = "Fyra stjärnor" },
                new SelectListItem { Value = "5", Text = "Fem stjärnor" }
            };
            }
        }

        [Required(ErrorMessage = "Måste ange typ av recension!")]
        public string TypeOfReviewValue { get; set; }
        public IEnumerable<SelectListItem> Values
        {
            get
            {
                return new[]
                {
                new SelectListItem { Value = "Bok", Text = "Bok" },
                new SelectListItem { Value = "Film", Text = "Film" },
                new SelectListItem { Value = "Spel", Text = "Spel" },
            };
            }
        }
    }
}