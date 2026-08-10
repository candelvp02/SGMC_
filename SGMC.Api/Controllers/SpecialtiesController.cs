using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Interfaces.Service;

namespace SGMC.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpecialtiesController : ControllerBase
    {
        private readonly ISpecialtyService _specialtyService;

        public SpecialtiesController(ISpecialtyService specialtyService)
        {
            _specialtyService = specialtyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _specialtyService.GetActiveAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(short id)
        {
            var result = await _specialtyService.GetByIdAsync(id);
            if (!result.Exitoso)
                return NotFound(result);
            return Ok(result);
        }
    }
}