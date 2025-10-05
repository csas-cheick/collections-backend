using backend.Data;
using backend.Dtos;
using backend.Models;
using backend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public UserService(AppDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<UserListResponseDto> GetAllUsersAsync(int page = 1, int pageSize = 10, string? search = null, string? role = null, bool? status = null)
        {
            try
            {
                var query = _context.User.AsQueryable();

                // Filtrer par recherche
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u => u.Name!.Contains(search) || 
                                           u.UserName!.Contains(search) || 
                                           u.Email!.Contains(search));
                }

                // Filtrer par rôle
                if (!string.IsNullOrEmpty(role))
                {
                    query = query.Where(u => u.Role == role);
                }

                // Filtrer par statut
                if (status.HasValue)
                {
                    query = query.Where(u => u.Status == status.Value);
                }

                var totalCount = await query.CountAsync();

                var users = await query
                    .OrderBy(u => u.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserResponseDto
                    {
                        Id = u.Id,
                        Name = u.Name ?? string.Empty,
                        UserName = u.UserName ?? string.Empty,
                        Phone = u.Phone,
                        Email = u.Email ?? string.Empty,
                        Role = u.Role ?? string.Empty,
                        Country = u.Country,
                        City = u.City,
                        Status = u.Status ?? false,
                        Picture = u.Picture,
                        CreatedAt = DateTime.UtcNow // Note: Vous pourriez vouloir ajouter un champ CreatedAt au modèle User
                    })
                    .ToListAsync();

                return new UserListResponseDto
                {
                    Success = true,
                    Message = "Utilisateurs récupérés avec succès",
                    Users = users,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                return new UserListResponseDto
                {
                    Success = false,
                    Message = $"Erreur lors de la récupération des utilisateurs: {ex.Message}",
                    Users = new List<UserResponseDto>(),
                    TotalCount = 0
                };
            }
        }

        public async Task<UserOperationResponseDto> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await _context.User.FindAsync(id);

                if (user == null)
                {
                    return new UserOperationResponseDto
                    {
                        Success = false,
                        Message = "Utilisateur non trouvé"
                    };
                }

                return new UserOperationResponseDto
                {
                    Success = true,
                    Message = "Utilisateur récupéré avec succès",
                    User = new UserResponseDto
                    {
                        Id = user.Id,
                        Name = user.Name ?? string.Empty,
                        UserName = user.UserName ?? string.Empty,
                        Phone = user.Phone,
                        Email = user.Email ?? string.Empty,
                        Role = user.Role ?? string.Empty,
                        Country = user.Country,
                        City = user.City,
                        Status = user.Status ?? false,
                        Picture = user.Picture,
                        CreatedAt = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                return new UserOperationResponseDto
                {
                    Success = false,
                    Message = $"Erreur lors de la récupération de l'utilisateur: {ex.Message}"
                };
            }
        }

        public async Task<UserOperationResponseDto> GetCurrentUserProfileAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return new UserOperationResponseDto
                    {
                        Success = false,
                        Message = "ID utilisateur invalide"
                    };
                }

                var user = await _context.User.FindAsync(userId);

                if (user == null)
                {
                    return new UserOperationResponseDto
                    {
                        Success = false,
                        Message = "Profil utilisateur non trouvé"
                    };
                }

                return new UserOperationResponseDto
                {
                    Success = true,
                    Message = "Profil utilisateur récupéré avec succès",
                    User = new UserResponseDto
                    {
                        Id = user.Id,
                        Name = user.Name ?? string.Empty,
                        UserName = user.UserName ?? string.Empty,
                        Phone = user.Phone,
                        Email = user.Email ?? string.Empty,
                        Role = user.Role ?? string.Empty,
                        Country = user.Country,
                        City = user.City,
                        Status = user.Status ?? false,
                        Picture = user.Picture,
                        CreatedAt = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                return new UserOperationResponseDto
                {
                    Success = false,
                    Message = $"Erreur lors de la récupération du profil utilisateur: {ex.Message}"
                };
            }
        }

        public async Task<UserOperationResponseDto> CreateUserAsync(CreateUserDto createUserDto, IFormFile? pictureFile = null)
        {
            try
            {
                // Vérifier si l'email existe déjà
                if (await UserExistsByEmailAsync(createUserDto.Email))
                {
                    return new UserOperationResponseDto
                    {
                        Success = false,
                        Message = "Un utilisateur avec cet email existe déjà"
                    };
                }

                // Vérifier si le nom d'utilisateur existe déjà
                if (await UserExistsByUserNameAsync(createUserDto.UserName))
                {
                    return new UserOperationResponseDto
                    {
                        Success = false,
                        Message = "Un utilisateur avec ce nom d'utilisateur existe déjà"
                    };
                }

                // Uploader l'image si fournie
                string? pictureUrl = null;
                if (pictureFile != null)
                {
                    try
                    {
                        pictureUrl = await _cloudinaryService.UploadImageAsync(pictureFile, "users");
                    }
                    catch (Exception uploadEx)
                    {
                        // Si l'upload échoue, on continue sans image
                        Console.WriteLine($"Erreur upload image: {uploadEx.Message}");
                    }
                }

                // Hasher le mot de passe
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

                var user = new User
                {
                    Name = createUserDto.Name,
                    UserName = createUserDto.UserName,
                    Phone = createUserDto.Phone,
                    Email = createUserDto.Email,
                    Password = createUserDto.Password,
                    Role = createUserDto.Role,
                    Country = createUserDto.Country,
                    City = createUserDto.City,
                    Status = createUserDto.Status,
                    Picture = pictureUrl ?? createUserDto.Picture
                };

                _context.User.Add(user);
                await _context.SaveChangesAsync();

                return new UserOperationResponseDto
                {
                    Success = true,
                    Message = "Utilisateur créé avec succès",
                    User = new UserResponseDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        UserName = user.UserName,
                        Phone = user.Phone,
                        Email = user.Email,
                        Role = user.Role,
                        Country = user.Country,
                        City = user.City,
                        Status = user.Status ?? false,
                        Picture = user.Picture,
                        CreatedAt = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                return new UserOperationResponseDto
                {
                    Success = false,
                    Message = $"Erreur lors de la création de l'utilisateur: {ex.Message}"
                };
            }
        }

        public async Task<UserOperationResponseDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto, IFormFile? pictureFile = null)
        {
            try
            {
                var user = await _context.User.FindAsync(id);

                if (user == null)
                {
                    return new UserOperationResponseDto
                    {
                        Success = false,
                        Message = "Utilisateur non trouvé"
                    };
                }

                // Vérifier si l'email existe déjà pour un autre utilisateur
                var existingUserWithEmail = await _context.User
                    .FirstOrDefaultAsync(u => u.Email == updateUserDto.Email && u.Id != id);

                if (existingUserWithEmail != null)
                {
                    return new UserOperationResponseDto
                    {
                        Success = false,
                        Message = "Un autre utilisateur avec cet email existe déjà"
                    };
                }

                // Vérifier si le nom d'utilisateur existe déjà pour un autre utilisateur
                var existingUserWithUserName = await _context.User
                    .FirstOrDefaultAsync(u => u.UserName == updateUserDto.UserName && u.Id != id);

                if (existingUserWithUserName != null)
                {
                    return new UserOperationResponseDto
                    {
                        Success = false,
                        Message = "Un autre utilisateur avec ce nom d'utilisateur existe déjà"
                    };
                }

                // Uploader l'image si fournie
                string? pictureUrl = null;
                if (pictureFile != null)
                {
                    try
                    {
                        pictureUrl = await _cloudinaryService.UploadImageAsync(pictureFile, "users");
                    }
                    catch (Exception uploadEx)
                    {
                        // Si l'upload échoue, on continue sans modifier l'image
                        Console.WriteLine($"Erreur upload image: {uploadEx.Message}");
                    }
                }

                // Mettre à jour les propriétés
                user.Name = updateUserDto.Name;
                user.UserName = updateUserDto.UserName;
                user.Phone = updateUserDto.Phone;
                user.Email = updateUserDto.Email;
                user.Role = updateUserDto.Role;
                user.Country = updateUserDto.Country;
                user.City = updateUserDto.City;
                user.Status = updateUserDto.Status;
                
                // Mettre à jour la photo seulement si une nouvelle image a été uploadée ou si une URL est fournie
                if (pictureUrl != null)
                {
                    user.Picture = pictureUrl;
                }
                else if (updateUserDto.Picture != null)
                {
                    user.Picture = updateUserDto.Picture;
                }

                await _context.SaveChangesAsync();

                return new UserOperationResponseDto
                {
                    Success = true,
                    Message = "Utilisateur mis à jour avec succès",
                    User = new UserResponseDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        UserName = user.UserName,
                        Phone = user.Phone,
                        Email = user.Email,
                        Role = user.Role,
                        Country = user.Country,
                        City = user.City,
                        Status = user.Status ?? false,
                        Picture = user.Picture,
                        CreatedAt = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                return new UserOperationResponseDto
                {
                    Success = false,
                    Message = $"Erreur lors de la mise à jour de l'utilisateur: {ex.Message}"
                };
            }
        }

        public async Task<UserOperationResponseDto> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _context.User.FindAsync(id);

                if (user == null)
                {
                    return new UserOperationResponseDto
                    {
                        Success = false,
                        Message = "Utilisateur non trouvé"
                    };
                }

                _context.User.Remove(user);
                await _context.SaveChangesAsync();

                return new UserOperationResponseDto
                {
                    Success = true,
                    Message = "Utilisateur supprimé avec succès"
                };
            }
            catch (Exception ex)
            {
                return new UserOperationResponseDto
                {
                    Success = false,
                    Message = $"Erreur lors de la suppression de l'utilisateur: {ex.Message}"
                };
            }
        }

        public async Task<UserOperationResponseDto> ChangePasswordAsync(int id, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _context.User.FindAsync(id);

                if (user == null)
                {
                    return new UserOperationResponseDto
                    {
                        Success = false,
                        Message = "Utilisateur non trouvé"
                    };
                }

                // Vérifier l'ancien mot de passe
                if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.Password))
                {
                    return new UserOperationResponseDto
                    {
                        Success = false,
                        Message = "L'ancien mot de passe est incorrect"
                    };
                }

                // Hasher le nouveau mot de passe
                user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

                await _context.SaveChangesAsync();

                return new UserOperationResponseDto
                {
                    Success = true,
                    Message = "Mot de passe changé avec succès"
                };
            }
            catch (Exception ex)
            {
                return new UserOperationResponseDto
                {
                    Success = false,
                    Message = $"Erreur lors du changement de mot de passe: {ex.Message}"
                };
            }
        }

        public async Task<UserOperationResponseDto> ToggleUserStatusAsync(int id)
        {
            try
            {
                var user = await _context.User.FindAsync(id);

                if (user == null)
                {
                    return new UserOperationResponseDto
                    {
                        Success = false,
                        Message = "Utilisateur non trouvé"
                    };
                }

                user.Status = !(user.Status ?? false);
                await _context.SaveChangesAsync();

                return new UserOperationResponseDto
                {
                    Success = true,
                    Message = $"Statut de l'utilisateur {(user.Status.Value ? "activé" : "désactivé")} avec succès",
                    User = new UserResponseDto
                    {
                        Id = user.Id,
                        Name = user.Name ?? string.Empty,
                        UserName = user.UserName ?? string.Empty,
                        Phone = user.Phone,
                        Email = user.Email ?? string.Empty,
                        Role = user.Role ?? string.Empty,
                        Country = user.Country,
                        City = user.City,
                        Status = user.Status.Value,
                        Picture = user.Picture,
                        CreatedAt = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                return new UserOperationResponseDto
                {
                    Success = false,
                    Message = $"Erreur lors du changement de statut: {ex.Message}"
                };
            }
        }

        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            return await _context.User.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UserExistsByUserNameAsync(string userName)
        {
            return await _context.User.AnyAsync(u => u.UserName == userName);
        }
    }
}