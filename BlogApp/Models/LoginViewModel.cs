using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlogApp.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Eposta")]
        public string? Email { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "{0}alanı en az {2} karakter uzunluğunda olmalıdır.",MinimumLength =6)]
        [Display(Name ="Parola")]
        public string? Password { get; set; }
    }
}