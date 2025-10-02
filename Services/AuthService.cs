using backend.Data;
using backend.Dtos;
using backend.Models;
using backend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Chercher l'utilisateur par email ou nom d'utilisateur
                var user = await _context.User
                    .FirstOrDefaultAsync(u => 
                        u.Email == loginDto.EmailOrUsername || 
                        u.UserName == loginDto.EmailOrUsername);

                if (user == null)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Utilisateur non trouvé"
                    };
                }

                // Vérifier le mot de passe (en clair pour l'instant)
                if (user.Password != loginDto.Password)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Mot de passe incorrect"
                    };
                }

                // Vérifier si l'utilisateur est actif
                if (user.Status != true)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Compte désactivé"
                    };
                }

                // Créer la réponse avec les données utilisateur (sans token)
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    UserName = user.UserName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role,
                    Country = user.Country,
                    City = user.City,
                    Status = user.Status,
                    Picture = user.Picture
                };

                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Connexion réussie",
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = $"Erreur lors de la connexion: {ex.Message}"
                };
            }
        }
    }
}