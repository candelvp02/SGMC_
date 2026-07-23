using System.ComponentModel.DataAnnotations;
using SGMC.Application.Dto.Appointments;

namespace SGMC.Web.Models.Appointment
{
    // ViewModel principal para la página Index de Citas
    public class AppointmentIndexViewModel
    {
        // Los filtros aplicados (vienen de la URL)
        public AppointmentFilterDto Filter { get; set; } = new();

        // La lista de citas a mostrar (resultado del filtro)
        public List<AppointmentListViewModel> Appointments { get; set; } = new();

        // Listas para poblar los dropdowns de filtro
        public List<PatientSelectViewModel>? Patients { get; set; }
        public List<DoctorSelectViewModel>? Doctors { get; set; }
        public List<StatusSelectViewModel>? Statuses { get; set; }
    }

    // ViewModel para la vista de creación de citas
    public class CreateAppointmentViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un paciente")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un paciente válido")]
        [Display(Name = "Paciente")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un doctor")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un doctor válido")]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "La fecha de la cita es requerida")]
        [Display(Name = "Fecha y Hora de la Cita")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; } = DateTime.Now.AddDays(1);

        [Display(Name = "Notas adicionales")]
        [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        public string? Notes { get; set; }

        // Listas para los dropdowns
        public List<PatientSelectViewModel>? Patients { get; set; }
        public List<DoctorSelectViewModel>? Doctors { get; set; }

        // Convierte el ViewModel a DTO
        public CreateAppointmentDto ToDto()
        {
            return new CreateAppointmentDto
            {
                PatientId = this.PatientId,
                DoctorId = this.DoctorId,
                AppointmentDate = this.AppointmentDate
            };
        }
    }

    // ViewModel para la vista de edición de citas
    public class EditAppointmentViewModel
    {
        [Required]
        public int AppointmentId { get; set; }

        [Required(ErrorMessage = "La fecha de la cita es requerida")]
        [Display(Name = "Fecha y Hora de la Cita")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un estado")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un estado válido")]
        [Display(Name = "Estado")]
        public int StatusId { get; set; }

        [Display(Name = "Notas")]
        [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        public string? Notes { get; set; }

        // Propiedades de solo lectura
        [Display(Name = "Paciente")]
        public string PatientName { get; set; } = string.Empty;

        [Display(Name = "Doctor")]
        public string DoctorName { get; set; } = string.Empty;

        [Display(Name = "Fecha de Creación")]
        public DateTime CreatedAt { get; set; }

        // Lista de estados para el dropdown
        public List<StatusSelectViewModel>? Statuses { get; set; }

        // Crea el ViewModel desde un DTO
        public static EditAppointmentViewModel FromDto(AppointmentDto dto)
        {
            return new EditAppointmentViewModel
            {
                AppointmentId = dto.AppointmentId,
                AppointmentDate = dto.AppointmentDate,
                StatusId = dto.StatusId,
                PatientName = dto.PatientName,
                DoctorName = dto.DoctorName,
                CreatedAt = dto.CreatedAt
            };
        }

        // Convierte el ViewModel a DTO
        public UpdateAppointmentDto ToDto()
        {
            return new UpdateAppointmentDto
            {
                AppointmentId = this.AppointmentId,
                AppointmentDate = this.AppointmentDate,
                StatusId = this.StatusId,
                Notes = this.Notes,
                Id = this.AppointmentId
            };
        }
    }

    // ViewModel para la lista de citas
    public class AppointmentListViewModel
    {
        public int AppointmentId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Propiedades calculadas
        public string AppointmentDateFormatted => AppointmentDate.ToString("dd/MM/yyyy hh:mm tt");
        public string StatusBadgeClass => StatusId switch
        {
            1 => "bg-warning",      // Pendiente
            2 => "bg-success",      // Confirmada
            3 => "bg-danger",       // Cancelada
            4 => "bg-info",         // Completada
            _ => "bg-secondary"
        };
        public bool IsPending => StatusId == 1;
        public bool IsConfirmed => StatusId == 2;
        public bool IsCancelled => StatusId == 3;
        public bool IsCompleted => StatusId == 4;
        public bool IsFuture => AppointmentDate > DateTime.Now;
        public bool CanEdit => StatusId != 3 && StatusId != 4;
        public bool CanCancel => StatusId != 3 && IsFuture;

        public static AppointmentListViewModel FromDto(AppointmentDto dto)
        {
            return new AppointmentListViewModel
            {
                AppointmentId = dto.AppointmentId,
                PatientName = dto.PatientName,
                DoctorName = dto.DoctorName,
                AppointmentDate = dto.AppointmentDate,
                StatusName = dto.StatusName,
                StatusId = dto.StatusId,
                CreatedAt = dto.CreatedAt
            };
        }
    }

    // ViewModel para detalles de la cita
    public class AppointmentDetailsViewModel
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Propiedades calculadas
        public string AppointmentDateFormatted => AppointmentDate.ToString("dddd, dd 'de' MMMM 'de' yyyy");
        public string AppointmentTimeFormatted => AppointmentDate.ToString("hh:mm tt");
        public string StatusBadgeClass => StatusId switch
        {
            1 => "bg-warning",
            2 => "bg-success",
            3 => "bg-danger",
            4 => "bg-info",
            _ => "bg-secondary"
        };
        public bool IsPending => StatusId == 1;
        public bool IsConfirmed => StatusId == 2;
        public bool IsCancelled => StatusId == 3;
        public bool IsCompleted => StatusId == 4;
        public bool IsFuture => AppointmentDate > DateTime.Now;
        public bool CanConfirm => IsPending && IsFuture;
        public bool CanReschedule => !IsCancelled && !IsCompleted;
        public bool CanCancel => !IsCancelled && !IsCompleted && IsFuture;

        public static AppointmentDetailsViewModel FromDto(AppointmentDto dto)
        {
            return new AppointmentDetailsViewModel
            {
                AppointmentId = dto.AppointmentId,
                PatientId = dto.PatientId,
                PatientName = dto.PatientName,
                DoctorId = dto.DoctorId,
                DoctorName = dto.DoctorName,
                AppointmentDate = dto.AppointmentDate,
                StatusName = dto.StatusName,
                StatusId = dto.StatusId,
                CreatedAt = dto.CreatedAt
            };
        }
    }

    // ViewModel auxiliar para selección de pacientes
    public class PatientSelectViewModel
    {
        public int PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string IdentificationNumber { get; set; } = string.Empty;
        public string DisplayText => $"{FullName} - {IdentificationNumber}";
    }

    // ViewModel auxiliar para selección de doctores
    public class DoctorSelectViewModel
    {
        public int DoctorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public string DisplayText => $"Dr. {FullName} - {SpecialtyName}";
    }

    // ViewModel auxiliar para selección de estados
    public class StatusSelectViewModel
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }

    // ViewModel para la vista de calendario del médico (PBI #32)
    public class DoctorCalendarViewModel
    {
        public DateTime DisplayedMonth { get; set; }
        public Dictionary<DateTime, List<AppointmentListViewModel>> AppointmentsByDay { get; set; } = new();

        public string MonthLabel => DisplayedMonth.ToString("MMMM yyyy");
        public DateTime PreviousMonth => DisplayedMonth.AddMonths(-1);
        public DateTime NextMonth => DisplayedMonth.AddMonths(1);

        public List<AppointmentListViewModel> AppointmentsOn(DateTime day) =>
            AppointmentsByDay.TryGetValue(day.Date, out var list) ? list : new List<AppointmentListViewModel>();
    }
}