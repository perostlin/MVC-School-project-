using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RecensionsProjekt.Models
{
    public class PasswordViewModel
    {
        [Required(ErrorMessage = "Du måste skriva in ditt önskade lösenord!")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Du måste skriva in ditt nuvarande lösenord!")]
        public string OldPassword { get; set; }

    }
}