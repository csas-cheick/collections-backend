using backend.Dtos;
using backend.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Récupérer tous les utilisateurs avec pagination et filtres
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<UserListResponseDto>> GetAllUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? role = null,
            [FromQuery] bool? status = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _userService.GetAllUsersAsync(page, pageSize, search, role, status);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Récupérer un utilisateur par son ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserOperationResponseDto>> GetUser(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new UserOperationResponseDto
                {
                    Success = false,
                    Message = "ID utilisateur invalide"
                });
            }

            var result = await _userService.GetUserByIdAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Créer un nouvel utilisateur
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<UserOperationResponseDto>> CreateUser([FromForm] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new UserOperationResponseDto
                {
                    Success = false,
                    Message = "Données invalides"
                });
            }

            try
            {
                var createDto = new CreateUserDto
                {
                    Name = request.Name,
                    UserName = request.UserName,
                    Phone = request.Phone,
                    Email = request.Email,
                    Password = request.Password,
                    Role = request.Role,
                    Country = request.Country,
                    City = request.City,
                    Status = request.Status
                };

                var result = await _userService.CreateUserAsync(createDto, request.PictureFile);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetUser), new { id = result.User!.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new UserOperationResponseDto
                {
                    Success = false,
                    Message = $"Erreur lors de la création de l'utilisateur: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Mettre à jour un utilisateur
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserOperationResponseDto>> UpdateUser(int id, [FromForm] UpdateUserRequest request)
        {
            if (id <= 0)
            {
                return BadRequest(new UserOperationResponseDto
                {
                    Success = false,
                    Message = "ID utilisateur invalide"
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new UserOperationResponseDto
                {
                    Success = false,
                    Message = "Données invalides"
                });
            }

            try
            {
                var updateDto = new UpdateUserDto
                {
                    Name = request.Name,
                    UserName = request.UserName,
                    Phone = request.Phone,
                    Email = request.Email,
                    Role = request.Role,
                    Country = request.Country,
                    City = request.City,
                    Status = request.Status
                };

                var result = await _userService.UpdateUserAsync(id, updateDto, request.PictureFile);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new UserOperationResponseDto
                {
                    Success = false,
                    Message = $"Erreur lors de la mise à jour de l'utilisateur: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Supprimer un utilisateur
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<UserOperationResponseDto>> DeleteUser(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new UserOperationResponseDto
                {
                    Success = false,
                    Message = "ID utilisateur invalide"
                });
            }

            var result = await _userService.DeleteUserAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Changer le mot de passe d'un utilisateur
        /// </summary>
        [HttpPut("{id}/change-password")]
        public async Task<ActionResult<UserOperationResponseDto>> ChangePassword(int id, ChangePasswordDto changePasswordDto)
        {
            if (id <= 0)
            {
                return BadRequest(new UserOperationResponseDto
                {
                    Success = false,
                    Message = "ID utilisateur invalide"
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new UserOperationResponseDto
                {
                    Success = false,
                    Message = "Données invalides"
                });
            }

            var result = await _userService.ChangePasswordAsync(id, changePasswordDto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Activer/Désactiver un utilisateur
        /// </summary>
        [HttpPut("{id}/toggle-status")]
        public async Task<ActionResult<UserOperationResponseDto>> ToggleUserStatus(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new UserOperationResponseDto
                {
                    Success = false,
                    Message = "ID utilisateur invalide"
                });
            }

            var result = await _userService.ToggleUserStatusAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Vérifier si un email existe
        /// </summary>
        [HttpGet("check-email/{email}")]
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email requis");
            }

            var exists = await _userService.UserExistsByEmailAsync(email);
            return Ok(new { exists });
        }

        /// <summary>
        /// Vérifier si un nom d'utilisateur existe
        /// </summary>
        [HttpGet("check-username/{username}")]
        public async Task<ActionResult<bool>> CheckUserNameExists(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest("Nom d'utilisateur requis");
            }

            var exists = await _userService.UserExistsByUserNameAsync(username);
            return Ok(new { exists });
        }

        /// <summary>
        /// Récupérer le profil de l'utilisateur connecté
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<UserOperationResponseDto>> GetCurrentUserProfile([FromQuery] int userId)
        {
            var result = await _userService.GetCurrentUserProfileAsync(userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }

    /// <summary>
    /// Classe de requête pour la création d'un utilisateur
    /// </summary>
    public class CreateUserRequest
    {
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string? City { get; set; }
        public bool Status { get; set; } = true;
        public IFormFile? PictureFile { get; set; }
    }

    /// <summary>
    /// Classe de requête pour la mise à jour d'un utilisateur
    /// </summary>
    public class UpdateUserRequest
    {
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string? City { get; set; }
        public bool Status { get; set; }
        public IFormFile? PictureFile { get; set; }
    }
}