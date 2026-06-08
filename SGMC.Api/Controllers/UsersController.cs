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

        [HttpGet]
        public async Task<ActionResult<OperationResult<List<UserDto>>>> GetAll()
        {
            var result = await _userService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OperationResult<UserDto>>> GetById(int id)
        {
            var result = await _userService.GetByIdAsync(id);
            if (!result.Exitoso)
                return NotFound(result);
            return Ok(result);
        }

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

        [HttpDelete("{id}")]
        public async Task<ActionResult<OperationResult>> Delete(int id)
        {
            var result = await _userService.DeleteAsync(id);
            if (!result.Exitoso)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("active")]
        public async Task<ActionResult<OperationResult<List<UserDto>>>> GetActive()
        {
            var result = await _userService.GetActiveUsersAsync();
            return Ok(result);
        }

        [HttpGet("role/{roleId}")]
        public async Task<ActionResult<OperationResult<List<UserDto>>>> GetByRole(int roleId)
        {
            var result = await _userService.GetByRoleAsync((short)roleId);
            return Ok(result);
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<OperationResult<UserDto>>> GetByEmail(string email)
        {
            var result = await _userService.GetByEmailAsync(email);
            if (!result.Exitoso)
                return NotFound(result);
            return Ok(result);
        }

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

        [HttpPost("password-reset")]
        public async Task<ActionResult<OperationResult>> RequestPasswordReset([FromBody] PasswordResetRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(OperationResult.Fallo("Email requerido"));

            var result = await _userService.RequestPasswordResetAsync(dto.Email);
            return Ok(result);
        }

        [HttpPatch("{id}/activate")]
        public async Task<ActionResult<OperationResult>> Activate(int id)
        {
            var result = await _userService.ActivateAccountAsync(id);
            if (!result.Exitoso)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPatch("{id}/deactivate")]
        public async Task<ActionResult<OperationResult>> Deactivate(int id)
        {
            var result = await _userService.DeactivateAsync(id);
            if (!result.Exitoso)
                return BadRequest(result);
            return Ok(result);
        }
    }

    public class PasswordResetRequestDto
    {
        public string Email { get; set; } = string.Empty;
    }
}
