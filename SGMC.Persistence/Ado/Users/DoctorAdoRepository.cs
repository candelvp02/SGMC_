using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SGMC.Application.Dto.System;
using SGMC.Application.Dto.Users;
using SGMC.Application.Interfaces.Persistence;
using SGMC.Persistence.Common;

namespace SGMC.Persistence.Ado.Users
{
    public class DoctorAdoRepository : IDoctorAdoRepository
    {
        private readonly StoredProcedureExecutor _sp;
        private readonly ILogger<DoctorAdoRepository> _logger;

        public DoctorAdoRepository(StoredProcedureExecutor sp, ILogger<DoctorAdoRepository> logger)
        {
            _sp = sp;
            _logger = logger;
        }

        public async Task<List<DoctorDto>> ListActiveAsync()
        {
            var doctors = new List<DoctorDto>();
            try
            {
                using var r = await _sp.ExecuteReaderAsync("users.usp_Doctor_ListActive");
                while (await r.ReadAsync())
                {
                    doctors.Add(MapDoctorDto(r));
                }
                return doctors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing active doctors");
                return new List<DoctorDto>();
            }
        }

        public async Task<List<DoctorDto>> SearchByNameAsync(string name)
        {
            var doctors = new List<DoctorDto>();
            try
            {
                using var r = await _sp.ExecuteReaderAsync(
                    "users.usp_Doctor_SearchByName",
                    ("@Name", name)
                );
                while (await r.ReadAsync())
                {
                    doctors.Add(MapDoctorDto(r));
                }
                return doctors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching doctors by name: {Name}", name);
                return new List<DoctorDto>();
            }
        }

        public async Task<int> GetTotalAppointmentsAsync(int doctorId)
        {
            try
            {
                var result = await _sp.ExecuteScalarAsync<int?>(
                    "users.usp_Doctor_GetTotalAppointments",
                    ("@DoctorID", doctorId)
                );
                return result ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total appointments for doctor {DoctorId}", doctorId);
                return 0;
            }
        }

        public async Task<List<DoctorDto>> ListBySpecialtyAsync(short specialtyId)
        {
            var doctors = new List<DoctorDto>();
            try
            {
                using var r = await _sp.ExecuteReaderAsync(
                    "users.usp_Doctor_ListBySpecialty",
                    ("@SpecialtyID", specialtyId)
                );
                while (await r.ReadAsync())
                {
                    doctors.Add(MapDoctorDto(r));
                }
                return doctors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing doctors by specialty {SpecialtyId}", specialtyId);
                return new List<DoctorDto>();
            }
        }

        public async Task<DoctorDto?> GetByIdWithDetailsAsync(int doctorId)
        {
            try
            {
                using var r = await _sp.ExecuteReaderAsync(
                    "users.usp_Doctor_GetByIdWithDetails",
                    ("@DoctorID", doctorId)
                );

                if (await r.ReadAsync())
                {
                    return MapDoctorDto(r);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctor by ID with details: {DoctorId}", doctorId);
                return null;
            }
        }

        public async Task<bool> UpdateAvailabilityModeAsync(int doctorId, short availabilityModeId)
        {
            try
            {
                var result = await _sp.ExecuteNonQueryAsync(
                    "users.usp_Doctor_UpdateAvailabilityMode",
                    ("@DoctorID", doctorId),
                    ("@AvailabilityModeID", availabilityModeId)
                );
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating availability mode for doctor {DoctorId}", doctorId);
                return false;
            }
        }

        // Método privado helper para mapear SqlDataReader a DoctorDto
        private static DoctorDto MapDoctorDto(SqlDataReader r)
        {
            return new DoctorDto
            {
                DoctorId = r.GetInt32(r.GetOrdinal("DoctorID")),
                FirstName = r.GetString(r.GetOrdinal("FirstName")),
                LastName = r.GetString(r.GetOrdinal("LastName")),
                DateOfBirth = r.IsDBNull(r.GetOrdinal("DateOfBirth"))
                    ? null
                    : DateOnly.FromDateTime(r.GetDateTime(r.GetOrdinal("DateOfBirth"))),
                IdentificationNumber = r.GetString(r.GetOrdinal("IdentificationNumber")),
                Gender = r.GetString(r.GetOrdinal("Gender")),
                Email = r.IsDBNull(r.GetOrdinal("Email"))
                    ? string.Empty
                    : r.GetString(r.GetOrdinal("Email")),
                SpecialtyId = r.GetInt16(r.GetOrdinal("SpecialtyID")),
                SpecialtyName = r.GetString(r.GetOrdinal("SpecialtyName")),
                LicenseNumber = r.GetString(r.GetOrdinal("LicenseNumber")),
                PhoneNumber = r.GetString(r.GetOrdinal("PhoneNumber")),
                YearsOfExperience = r.GetInt32(r.GetOrdinal("YearsOfExperience")),
                Education = r.GetString(r.GetOrdinal("Education")),
                Bio = r.IsDBNull(r.GetOrdinal("Bio"))
                    ? null
                    : r.GetString(r.GetOrdinal("Bio")),
                ConsultationFee = r.IsDBNull(r.GetOrdinal("ConsultationFee"))
                    ? null
                    : r.GetDecimal(r.GetOrdinal("ConsultationFee")),
                ClinicAddress = r.IsDBNull(r.GetOrdinal("ClinicAddress"))
                    ? null
                    : r.GetString(r.GetOrdinal("ClinicAddress")),
                AvailabilityMode = r.IsDBNull(r.GetOrdinal("AvailabilityMode"))
                    ? null
                    : r.GetString(r.GetOrdinal("AvailabilityMode")),
                LicenseExpirationDate = DateOnly.FromDateTime(r.GetDateTime(r.GetOrdinal("LicenseExpirationDate"))),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive"))
            };
        }
    }
}