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
        public async Task<ActionResult<UserOperationResponseDto>> CreateUser(CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new UserOperationResponseDto
                {
                    Success = false,
                    Message = "Données invalides"
                });
            }

            var result = await _userService.CreateUserAsync(createUserDto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetUser), new { id = result.User!.Id }, result);
        }

        /// <summary>
        /// Mettre à jour un utilisateur
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserOperationResponseDto>> UpdateUser(int id, UpdateUserDto updateUserDto)
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

            var result = await _userService.UpdateUserAsync(id, updateUserDto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
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
}