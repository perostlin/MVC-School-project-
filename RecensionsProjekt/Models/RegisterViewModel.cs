using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RecensionsProjekt.Models
{
    public class RegisterViewModel : LoginViewModel
    {
        [Required(ErrorMessage = "Måste fylla i förnamn!")]
        [StringLength(150)]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "Måste fylla i efternamn!")]
        [StringLength(250)]
        public string Lastname { get; set; }

        [Required(ErrorMessage = "Måste fylla i email-address!")]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}