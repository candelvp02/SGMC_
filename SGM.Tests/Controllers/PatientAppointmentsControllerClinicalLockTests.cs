using FluentAssertions;
using SGMC.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SGMC.Tests.Controllers
{
    public class PatientAppointmentsControllerClinicalLockTests
    {
        private static IEnumerable<MethodInfo> GetPublicActionMethods()
        {
            return typeof(PatientAppointmentsController)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName); // excluye getters/setters heredados
        }

        [Fact]
        public void Controller_NoDebeExponerNingunaAccionDeEscrituraSobreMedicalRecord()
        {
            // Task 112: auditoria de interfaz - el paciente jamas debe tener
            // un endpoint capaz de crear, editar o eliminar un registro clinico
            var acciones = GetPublicActionMethods();

            var palabrasProhibidas = new[] { "Edit", "Update", "Delete", "Create" };

            var accionesSospechosas = acciones.Where(m =>
                palabrasProhibidas.Any(p => m.Name.Contains(p, StringComparison.OrdinalIgnoreCase)) &&
                (m.Name.Contains("Record", StringComparison.OrdinalIgnoreCase) ||
                 m.Name.Contains("Diagnos", StringComparison.OrdinalIgnoreCase) ||
                 m.Name.Contains("Treatment", StringComparison.OrdinalIgnoreCase) ||
                 m.GetParameters().Any(p =>
                     p.ParameterType.Name.Contains("MedicalRecord", StringComparison.OrdinalIgnoreCase))))
                .ToList();

            accionesSospechosas.Should().BeEmpty(
                "el paciente no debe tener ninguna accion capaz de modificar diagnosticos o tratamientos");
        }

        [Fact]
        public void Controller_NoDebeRecibirNingunParametroDeTipoMedicalRecordDto()
        {
            // Verifica de forma mas estricta: ninguna accion publica del controlador
            // acepta directamente un DTO de escritura de historial clinico
            var acciones = GetPublicActionMethods();

            var tiposProhibidos = acciones
                .SelectMany(m => m.GetParameters())
                .Select(p => p.ParameterType.Name)
                .Where(t =>
                    t.Equals("CreateMedicalRecordDto", StringComparison.OrdinalIgnoreCase) ||
                    t.Equals("UpdateMedicalRecordDto", StringComparison.OrdinalIgnoreCase))
                .ToList();

            tiposProhibidos.Should().BeEmpty(
                "ninguna accion del controlador de paciente debe aceptar DTOs de escritura de historial clinico");
        }

        [Fact]
        public void Controller_SoloDebeExponerAccionesDeSoloLecturaParaCitas()
        {
            // Lista blanca de acciones esperadas en el controlador del paciente.
            // Cualquier accion nueva que no este aqui debe revisarse manualmente
            // antes de aprobarse, para evitar exponer escritura de datos clinicos.
            var accionesEsperadas = new[]
            {
                "Index", "Details", "Reschedule", "CancelConfirmed"
            };

            var accionesReales = GetPublicActionMethods().Select(m => m.Name).ToList();

            accionesReales.Should().BeSubsetOf(accionesEsperadas,
                "cualquier accion nueva en el controlador del paciente debe auditarse " +
                "antes de agregarse a la lista blanca, para evitar exponer escritura clinica");
        }
    }
}