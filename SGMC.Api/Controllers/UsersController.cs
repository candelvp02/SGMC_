using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.System;
using SGMC.Application.Dto.Users;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Base;

namespace SGMC.Api.Controllers
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

        // GET api/users — Listado completo de usuarios - solo admin
        [HttpGet]
        public async Task<ActionResult<OperationResult<List<UserDto>>>> GetAll()
        {
            var result = await _userService.GetAllAsync();
            return Ok(result);
        }

        // GET api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OperationResult<UserDto>>> GetById(int id)
        {
            var result = await _userService.GetByIdAsync(id);
            if (!result.Exitoso)
                return NotFound(result);
            return Ok(result);
        }

        // GET api/users/search?query=nombre_o_email
        [HttpGet("search")]
        public async Task<ActionResult<OperationResult<List<UserDto>>>> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(OperationResult.Fallo("El término de búsqueda es requerido"));

            var result = await _userService.SearchAsync(query);
            return Ok(result);
        }

        // GET api/users/active
        [HttpGet("active")]
        public async Task<ActionResult<OperationResult<List<UserDto>>>> GetActive()
        {
            var result = await _userService.GetActiveUsersAsync();
            return Ok(result);
        }

        // GET api/users/role/{roleId}
        [HttpGet("role/{roleId}")]
        public async Task<ActionResult<OperationResult<List<UserDto>>>> GetByRole(int roleId)
        {
            var result = await _userService.GetByRoleAsync((short)roleId);
            return Ok(result);
        }

        // GET api/users/email/{email}
        [HttpGet("email/{email}")]
        public async Task<ActionResult<OperationResult<UserDto>>> GetByEmail(string email)
        {
            var result = await _userService.GetByEmailAsync(email);
            if (!result.Exitoso)
                return NotFound(result);
            return Ok(result);
        }

        // POST api/users
        [HttpPost]
        public async Task<ActionResult<OperationResult<UserDto>>> Create([FromBody] RegisterUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(OperationResult.Fallo("Datos invalidos"));

            var result = await _userService.RegisterAsync(dto);
            if (!result.Exitoso)
                return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Datos?.UserId }, result);
        }

        // PUT api/users/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<OperationResult<UserDto>>> Update(int id, [FromBody] UpdateUserDto dto)
        {
            if (id != dto.UserId)
                return BadRequest(OperationResult.Fallo("ID no coincide"));

            if (!ModelState.IsValid)
                return BadRequest(OperationResult.Fallo("Datos invalidos"));

            var result = await _userService.UpdateProfileAsync(dto);
            if (!result.Exitoso)
                return BadRequest(result);
            return Ok(result);
        }

        // PATCH api/users/{id}/activate — Activar cuenta - solo admin
        [HttpPatch("{id}/activate")]
        public async Task<ActionResult<OperationResult>> Activate(int id)
        {
            var result = await _userService.ActivateAccountAsync(id);
            if (!result.Exitoso)
                return BadRequest(result);
            return Ok(result);
        }

        // PATCH api/users/{id}/deactivate — Desactivar cuenta - solo admin
        [HttpPatch("{id}/deactivate")]
        public async Task<ActionResult<OperationResult>> Deactivate(int id)
        {
            var result = await _userService.DeactivateAsync(id);
            if (!result.Exitoso)
                return BadRequest(result);
            return Ok(result);
        }

        // PATCH api/users/{id}/role — Cambiar rol - solo admin
        [HttpPatch("{id}/role")]
        public async Task<ActionResult<OperationResult>> ChangeRole(int id, [FromBody] ChangeRoleDto dto)
        {
            if (dto == null || dto.RoleId <= 0)
                return BadRequest(OperationResult.Fallo("Rol inválido"));

            var result = await _userService.ChangeRoleAsync(id, dto.RoleId);
            if (!result.Exitoso)
                return BadRequest(result);
            return Ok(result);
        }

        // POST api/users/authenticate
        [HttpPost("authenticate")]
        public async Task<ActionResult<OperationResult<UserDto>>> Authenticate([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(OperationResult.Fallo("Credenciales invalidas"));

            var result = await _userService.AuthenticateAsync(dto);
            if (!result.Exitoso)
                return Unauthorized(result);
            return Ok(result);
        }

        // POST api/users/password-reset
        [HttpPost("password-reset")]
        public async Task<ActionResult<OperationResult>> RequestPasswordReset(
            [FromBody] PasswordResetRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(OperationResult.Fallo("Email requerido"));

            var result = await _userService.RequestPasswordResetAsync(dto.Email);
            return Ok(result);
        }
    }

    // DTOs locales
    public class PasswordResetRequestDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ChangeRoleDto
    {
        public int RoleId { get; set; }
    }
}