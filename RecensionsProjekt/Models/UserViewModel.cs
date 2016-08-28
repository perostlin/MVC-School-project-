using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RecensionsProjekt.Models
{
    public class UserViewModel
    {
        [Required(ErrorMessage = "Måste fylla i förnamn!")]
        [StringLength(maximumLength: 50, MinimumLength = 2)]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "Måste fylla i efternamn!")]
        [StringLength(maximumLength: 50, MinimumLength = 1)]
        public string Lastname { get; set; }

        [Required(ErrorMessage = "Måste fylla i användarnamn!")]
        [StringLength(maximumLength: 250, MinimumLength = 6)]
        public string  Username { get; set; }

        [Required(ErrorMessage = "Måste fylla i email-address!")]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public List<ReviewViewModel> ReviewViewModelList { get; set; }

        public string ProfileImagePath { get; set; }

        public UserViewModel()
        {
            ReviewViewModelList = new List<ReviewViewModel>();
        }
    }
}