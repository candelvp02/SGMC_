using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SGMC.Application.Interfaces.Service;

namespace SGMC.Web.ViewComponents
{
    public class SpecialtySelectViewComponent : ViewComponent
    {
        private readonly ISpecialtyService _specialtyService;

        public SpecialtySelectViewComponent(ISpecialtyService specialtyService)
        {
            _specialtyService = specialtyService;
        }

        // selectedId: especialidad ya seleccionada (para Edit); null en Create
        // fieldName: nombre del <select> a generar, para que coincida con el model binding (ej. "SpecialtyId")
        public async Task<IViewComponentResult> InvokeAsync(short? selectedId, string fieldName = "SpecialtyId")
        {
            var result = await _specialtyService.GetActiveAsync();
            var specialties = result.Exitoso && result.Datos != null
                ? result.Datos
                : new List<SGMC.Application.Dto.Medical.SpecialtyDto>();

            ViewBag.FieldName = fieldName;
            var items = new SelectList(specialties, "SpecialtyId", "SpecialtyName", selectedId);
            return View(items);
        }
    }
}