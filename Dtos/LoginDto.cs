using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class LoginDto
    {
        [Required(ErrorMessage = "L'email ou nom d'utilisateur est requis")]
        public string? EmailOrUsername { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis")]
        public string? Password { get; set; }
    }
}