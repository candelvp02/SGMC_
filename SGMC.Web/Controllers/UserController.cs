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

        // GET: User/Search?query=
        public async Task<ActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return RedirectToAction(nameof(Index));

            OperationResult<List<UserDto>> result = await _userService.SearchAsync(query);

            if (!result.Exitoso)
            {
                ViewBag.ErrorMessage = result.Mensaje;
                return View("Index", new List<UserDto>());
            }

            ViewBag.Query = query;
            return View("Index", result.Datos);
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
                    return View(registerUserDto);

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
                    return View(updateUserDto);

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

        // POST: User/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Activate(int id)
        {
            try
            {
                OperationResult result = await _userService.ActivateAccountAsync(id);

                if (!result.Exitoso)
                    ViewBag.ErrorMessage = result.Mensaje;
                else
                    ViewBag.SuccessMessage = "Usuario activado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: User/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                OperationResult result = await _userService.DeactivateAsync(id);

                if (!result.Exitoso)
                    ViewBag.ErrorMessage = result.Mensaje;
                else
                    ViewBag.SuccessMessage = "Usuario desactivado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: User/ChangeRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeRole(int id, int roleId)
        {
            try
            {
                OperationResult result = await _userService.ChangeRoleAsync(id, roleId);

                if (!result.Exitoso)
                    ViewBag.ErrorMessage = result.Mensaje;
                else
                    ViewBag.SuccessMessage = "Rol actualizado correctamente.";

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
                    return View(changePasswordDto);

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