using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Moq;
using Moq.Protected;
using SGMC.Application.Dto.Users;
using SGMC.Application.Dto.System;
using SGMC.Web.Models;
using SGMC.Web.Services;
using Xunit;

namespace SGMC.Web.Tests.Services
{
    public class DoctorApiClientTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly DoctorApiClient _doctorApiClient;

        public DoctorApiClientTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://localhost:5038/api/")
            };
            _doctorApiClient = new DoctorApiClient(_httpClient);
        }

        [Fact]
        public async Task CreateAsync_WithValidData_ShouldReturnCreatedDoctor()
        {
            // Arrange
            var createDto = new RegisterDoctorDto
            {
                FirstName = "Luis",
                LastName = "González",
                IdentificationNumber = "001-0000020-2",
                DateOfBirth = new DateOnly(1980, 3, 10),
                Gender = "M",
                Email = "luis.gonzalez@hospital.com",
                Password = "SecurePass123",
                PhoneNumber = "809-555-3333",
                SpecialtyId = 1,
                LicenseNumber = "LIC-003",
                LicenseExpirationDate = new DateOnly(2027, 6, 30),
                YearsOfExperience = 12,
                Education = "MD, Universidad Nacional",
                Bio = "Especialista en medicina interna",
                ConsultationFee = 2000.00m,
                ClinicAddress = "Policlínica Central, Consultorio 5"
            };

            var createdDoctor = new DoctorDto
            {
                DoctorId = 3,
                FirstName = "Luis",
                LastName = "González",
                SpecialtyName = "Medicina Interna",
                LicenseNumber = "LIC-003",
                PhoneNumber = "809-555-3333",
                Email = "luis.gonzalez@hospital.com",
                IsActive = true
            };

            var operationResult = new OperationResultDto<DoctorDto>
            {
                Exitoso = true,
                Mensaje = "Doctor creado correctamente",
                Datos = createdDoctor
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var responseContent = JsonSerializer.Serialize(operationResult, jsonOptions);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().EndsWith("Doctors")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _doctorApiClient.CreateAsync(createDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.DoctorId.Should().Be(3);
            result.Data.FirstName.Should().Be("Luis");
            result.Data.LastName.Should().Be("González");
        }

        [Fact]
        public async Task GetAllAsync_WhenApiReturnsSuccess_ShouldReturnDoctorsList()
        {
            // Arrange
            var expectedDoctors = new List<DoctorDto>
            {
                new DoctorDto
                {
                    DoctorId = 1,
                    FirstName = "Carlos",
                    LastName = "Fernández",
                    SpecialtyName = "Cardiología",
                    LicenseNumber = "LIC-001",
                    PhoneNumber = "809-555-1111",
                    YearsOfExperience = 15,
                    IsActive = true
                }
            };

            var operationResult = new OperationResultDto<List<DoctorDto>>
            {
                Exitoso = true,
                Mensaje = "Doctores obtenidos correctamente",
                Datos = expectedDoctors
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var responseContent = JsonSerializer.Serialize(operationResult, jsonOptions);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().EndsWith("Doctors")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _doctorApiClient.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.Data![0].FirstName.Should().Be("Carlos");
            result.Data![0].LastName.Should().Be("Fernández");
        }

        [Fact]
        public async Task GetByIdAsync_WhenDoctorExists_ShouldReturnDoctor()
        {
            // Arrange
            var expectedDoctor = new DoctorDto
            {
                DoctorId = 1,
                FirstName = "Carlos",
                LastName = "Fernández",
                SpecialtyName = "Cardiología",
                LicenseNumber = "LIC-001",
                IsActive = true
            };

            var operationResult = new OperationResultDto<DoctorDto>
            {
                Exitoso = true,
                Mensaje = "Doctor obtenido correctamente",
                Datos = expectedDoctor
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var responseContent = JsonSerializer.Serialize(operationResult, jsonOptions);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().EndsWith("Doctors/1")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _doctorApiClient.GetByIdAsync(1);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.FirstName.Should().Be("Carlos");
            result.Data.LastName.Should().Be("Fernández");
        }

        [Fact]
        public async Task UpdateAsync_WithValidData_ShouldReturnUpdatedDoctor()
        {
            // Arrange
            var updateDto = new UpdateDoctorDto
            {
                DoctorId = 1,
                PhoneNumber = "809-555-4444",
                YearsOfExperience = 16,
                Education = "MD, PhD en Cardiología",
                Bio = "Especialista senior",
                ConsultationFee = 3000.00m,
                ClinicAddress = "Nuevo Centro Médico",
                LicenseExpirationDate = new DateOnly(2028, 12, 31)
            };

            var updatedDoctor = new DoctorDto
            {
                DoctorId = 1,
                FirstName = "Carlos",
                LastName = "Fernández",
                PhoneNumber = "809-555-4444",
                YearsOfExperience = 16,
                IsActive = true
            };

            var operationResult = new OperationResultDto<DoctorDto>
            {
                Exitoso = true,
                Mensaje = "Doctor actualizado correctamente",
                Datos = updatedDoctor
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var responseContent = JsonSerializer.Serialize(operationResult, jsonOptions);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Put &&
                        req.RequestUri!.ToString().EndsWith("Doctors/1")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _doctorApiClient.UpdateAsync(updateDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.YearsOfExperience.Should().Be(16);
        }

        [Fact]
        public async Task DeleteAsync_WhenDoctorExists_ShouldReturnSuccess()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Delete &&
                        req.RequestUri!.ToString().EndsWith("Doctors/1")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var result = await _doctorApiClient.DeleteAsync(1);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }
    }
}