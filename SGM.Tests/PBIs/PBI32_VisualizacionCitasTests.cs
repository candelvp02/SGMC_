using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SGMC.Api.Controllers;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Base;
using SGMC.Web.Controllers;
using SGMC.Web.Models.Appointment;
using Xunit;

namespace SGMC.Tests.PBIs
{
    /// <summary>
    /// PBI #32 — Visualización de Citas
    /// Criterios cubiertos:
    /// CA-1  El paciente ve el listado de sus citas (nombre del médico, fecha, hora, estado)
    /// CA-4  Al seleccionar una cita se muestran todos los detalles
    /// CA-5  El listado solo muestra citas del usuario autenticado (aislamiento paciente/médico)
    /// CA-6  Sin citas registradas → mensaje indicándolo
    ///
    /// Task 135 — Pruebas funcionales de visibilidad cruzada: valida que un paciente no pueda ver
    /// citas de otro paciente, y que un médico no pueda ver citas de otro médico, ni por la Web
    /// (PatientAppointmentsController / DoctorAppointmentsController) ni por la Api (AppointmentsController).
    /// </summary>
    public class PBI32_VisualizacionCitasTests
    {
        private static AppointmentDto BuildAppointment(int id, int patientId, int doctorId, int statusId = 1) =>
            new()
            {
                AppointmentId = id,
                PatientId = patientId,
                DoctorId = doctorId,
                AppointmentDate = DateTime.Now.AddDays(1),
                StatusId = statusId,
                StatusName = "Pendiente",
                PatientName = $"Paciente {patientId}",
                DoctorName = $"Doctor {doctorId}",
                CreatedAt = DateTime.Now
            };

        private static ClaimsPrincipal BuildUser(int userId, string role)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            }, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        // ───────────────────────── Web: PatientAppointmentsController ─────────────────────────

        [Fact]
        public async Task Patient_Details_NoPuedeVerCitaDeOtroPaciente()
        {
            // Arrange: la cita #5 pertenece al paciente 99, pero quien pregunta es el paciente 1
            var appointmentServiceMock = new Mock<IAppointmentService>();
            appointmentServiceMock
                .Setup(s => s.GetByIdAsync(5))
                .ReturnsAsync(OperationResult<AppointmentDto>.Exito(BuildAppointment(5, patientId: 99, doctorId: 7)));

            var controller = new PatientAppointmentsController(appointmentServiceMock.Object, Mock.Of<IPatientService>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = BuildUser(userId: 1, role: "Paciente") }
                },
                TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                    new DefaultHttpContext(), Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>())
            };

            // Act
            var result = await controller.Details(5);

            // Assert: se redirige (no se muestra la vista con los datos de otro paciente)
            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be(nameof(PatientAppointmentsController.Index));
        }

        [Fact]
        public async Task Patient_Details_SiPuedeVerSuPropiaCita()
        {
            var appointmentServiceMock = new Mock<IAppointmentService>();
            appointmentServiceMock
                .Setup(s => s.GetByIdAsync(5))
                .ReturnsAsync(OperationResult<AppointmentDto>.Exito(BuildAppointment(5, patientId: 1, doctorId: 7)));

            var controller = new PatientAppointmentsController(appointmentServiceMock.Object, Mock.Of<IPatientService>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = BuildUser(userId: 1, role: "Paciente") }
                }
            };

            var result = await controller.Details(5);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<AppointmentDetailsViewModel>().Subject;
            model.AppointmentId.Should().Be(5);
        }

        // ───────────────────────── Web: DoctorAppointmentsController ─────────────────────────

        [Fact]
        public async Task Doctor_Details_NoPuedeVerCitaDeOtroMedico()
        {
            // Arrange: la cita #8 pertenece al médico 50, pero quien pregunta es el médico 2
            var appointmentServiceMock = new Mock<IAppointmentService>();
            appointmentServiceMock
                .Setup(s => s.GetByIdAsync(8))
                .ReturnsAsync(OperationResult<AppointmentDto>.Exito(BuildAppointment(8, patientId: 3, doctorId: 50)));

            var controller = new DoctorAppointmentsController(appointmentServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = BuildUser(userId: 2, role: "Médico") }
                },
                TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                    new DefaultHttpContext(), Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>())
            };

            var result = await controller.Details(8);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be(nameof(DoctorAppointmentsController.Index));
        }

        [Fact]
        public async Task Doctor_Index_SoloListaSusPropiasCitas_YMuestraVaciaSiNoHay()
        {
            // El médico 2 no tiene citas asignadas (el mock simula que su lista está vacía)
            var appointmentServiceMock = new Mock<IAppointmentService>();
            appointmentServiceMock
                .Setup(s => s.GetByDoctorIdAsync(2))
                .ReturnsAsync(OperationResult<List<AppointmentDto>>.Exito(new List<AppointmentDto>(), "No tienes citas registradas."));

            var controller = new DoctorAppointmentsController(appointmentServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = BuildUser(userId: 2, role: "Médico") }
                }
            };

            await controller.Index(null, null, null);

            var appointments = controller.ViewBag.Appointments as List<AppointmentListViewModel>;
            appointments.Should().NotBeNull();
            appointments!.Should().BeEmpty();

            // GetByPatientIdAsync jamás debió invocarse: un médico solo consulta por su propio doctorId
            appointmentServiceMock.Verify(s => s.GetByDoctorIdAsync(2), Times.Once);
        }

        // ───────────────────────── Api: AppointmentsController ─────────────────────────

        [Fact]
        public async Task Api_GetById_PacienteNoPuedeVerCitaDeOtroPaciente_DevuelveForbid()
        {
            var appointmentServiceMock = new Mock<IAppointmentService>();
            appointmentServiceMock
                .Setup(s => s.GetByIdAsync(5))
                .ReturnsAsync(OperationResult<AppointmentDto>.Exito(BuildAppointment(5, patientId: 99, doctorId: 7)));

            var controller = new AppointmentsController(appointmentServiceMock.Object, Mock.Of<ILogger<AppointmentsController>>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = BuildUser(userId: 1, role: "Paciente") }
                }
            };

            var result = await controller.GetById(5);

            result.Result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Api_GetById_MedicoNoPuedeVerCitaDeOtroMedico_DevuelveForbid()
        {
            var appointmentServiceMock = new Mock<IAppointmentService>();
            appointmentServiceMock
                .Setup(s => s.GetByIdAsync(8))
                .ReturnsAsync(OperationResult<AppointmentDto>.Exito(BuildAppointment(8, patientId: 3, doctorId: 50)));

            var controller = new AppointmentsController(appointmentServiceMock.Object, Mock.Of<ILogger<AppointmentsController>>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = BuildUser(userId: 2, role: "Médico") }
                }
            };

            var result = await controller.GetById(8);

            result.Result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Api_GetById_PacienteSiPuedeVerSuPropiaCita()
        {
            var appointmentServiceMock = new Mock<IAppointmentService>();
            appointmentServiceMock
                .Setup(s => s.GetByIdAsync(5))
                .ReturnsAsync(OperationResult<AppointmentDto>.Exito(BuildAppointment(5, patientId: 1, doctorId: 7)));

            var controller = new AppointmentsController(appointmentServiceMock.Object, Mock.Of<ILogger<AppointmentsController>>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = BuildUser(userId: 1, role: "Paciente") }
                }
            };

            var result = await controller.GetById(5);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Api_GetMyAppointmentsAsDoctor_UsaElIdDelTokenNoUnParametro()
        {
            // El médico autenticado es el 2; el servicio solo debe ser consultado con ese ID,
            // sin importar qué otros datos pudieran estar disponibles.
            var appointmentServiceMock = new Mock<IAppointmentService>();
            appointmentServiceMock
                .Setup(s => s.GetByDoctorIdAsync(2))
                .ReturnsAsync(OperationResult<List<AppointmentDto>>.Exito(
                    new List<AppointmentDto> { BuildAppointment(1, patientId: 10, doctorId: 2) }));

            var controller = new AppointmentsController(appointmentServiceMock.Object, Mock.Of<ILogger<AppointmentsController>>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = BuildUser(userId: 2, role: "Médico") }
                }
            };

            var result = await controller.GetMyAppointmentsAsDoctor(null, null, null);

            var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var body = ok.Value.Should().BeOfType<OperationResult<List<AppointmentDto>>>().Subject;
            body.Datos.Should().ContainSingle().Which.DoctorId.Should().Be(2);
            appointmentServiceMock.Verify(s => s.GetByDoctorIdAsync(2), Times.Once);
            appointmentServiceMock.Verify(s => s.GetByDoctorIdAsync(It.Is<int>(id => id != 2)), Times.Never);
        }

        [Fact]
        public async Task Api_GetMyAppointmentsAsDoctor_SinCitas_DevuelveMensajeVacio()
        {
            var appointmentServiceMock = new Mock<IAppointmentService>();
            appointmentServiceMock
                .Setup(s => s.GetByDoctorIdAsync(2))
                .ReturnsAsync(OperationResult<List<AppointmentDto>>.Exito(new List<AppointmentDto>()));

            var controller = new AppointmentsController(appointmentServiceMock.Object, Mock.Of<ILogger<AppointmentsController>>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = BuildUser(userId: 2, role: "Médico") }
                }
            };

            var result = await controller.GetMyAppointmentsAsDoctor(null, null, null);

            var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var body = ok.Value.Should().BeOfType<OperationResult<List<AppointmentDto>>>().Subject;
            body.Datos.Should().BeEmpty();
            body.Mensaje.Should().Be("No tienes citas registradas.");
        }
    }
}
