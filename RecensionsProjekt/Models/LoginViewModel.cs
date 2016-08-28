using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RecensionsProjekt.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Måste ange användarnamn!")]
        [StringLength(maximumLength: 100, MinimumLength = 6, ErrorMessage = "Fältet Användarnamn måste vara minst 6 bokstäver och max 100 bokstäver.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Måste ange lösenord!")]
        public string Password { get; set; }
    }
}