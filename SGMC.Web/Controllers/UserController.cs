using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.System;
using SGMC.Application.Dto.Users;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Base;
using SGMC.Web.Models.User;

namespace SGMC.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: User
        public async Task<ActionResult> Index()
        {
            OperationResult<List<UserDto>> result = await _userService.GetAllAsync();

            if (!result.Exitoso)
            {
                ViewBag.ErrorMessage = result.Mensaje;
                return View(new List<UserDto>());
            }

            return View(result.Datos);
        }

        // GET: User/Details/5
        public async Task<ActionResult> Details(int id)
        {
            OperationResult<UserDto> result = await _userService.GetByIdAsync(id);

            if (!result.Exitoso)
            {
                ViewBag.ErrorMessage = result.Mensaje;
                return View();
            }

            return View(result.Datos);
        }

        // GET: User/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RegisterUserDto registerUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(registerUserDto);
                }

                OperationResult<UserDto> result = await _userService.RegisterAsync(registerUserDto);

                if (!result.Exitoso)
                {
                    ViewBag.ErrorMessage = result.Mensaje;
                    return View(registerUserDto);
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(registerUserDto);
            }
        }

        // GET: User/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            OperationResult<UserDto> result = await _userService.GetByIdAsync(id);

            if (!result.Exitoso || result.Datos == null)
            {
                ViewBag.ErrorMessage = result.Mensaje;
                return View();
            }

            var updateDto = new UpdateUserDto
            {
                UserId = result.Datos.UserId,
                Email = result.Datos.Email
            };

            return View(updateDto);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UpdateUserDto updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(updateUserDto);
                }

                OperationResult<UserDto> result = await _userService.UpdateProfileAsync(updateUserDto);

                if (!result.Exitoso)
                {
                    ViewBag.ErrorMessage = result.Mensaje;
                    return View(updateUserDto);
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(updateUserDto);
            }
        }

        // GET: User/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            OperationResult<UserDto> result = await _userService.GetByIdAsync(id);

            if (!result.Exitoso)
            {
                ViewBag.ErrorMessage = result.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Datos);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                OperationResult result = await _userService.DeleteAsync(id);

                if (!result.Exitoso)
                {
                    ViewBag.ErrorMessage = result.Mensaje;
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: User/ChangePassword/5
        public ActionResult ChangePassword(int id)
        {
            var model = new ChangePasswordDto { UserId = id };
            return View(model);
        }

        // POST: User/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(changePasswordDto);
                }

                OperationResult result = await _userService.ChangePasswordAsync(changePasswordDto);

                if (!result.Exitoso)
                {
                    ViewBag.ErrorMessage = result.Mensaje;
                    return View(changePasswordDto);
                }

                ViewBag.SuccessMessage = "Contraseña cambiada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(changePasswordDto);
            }
        }
    }
}
