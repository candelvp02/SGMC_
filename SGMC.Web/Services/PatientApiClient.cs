//using System.Net.Http;
//using System.Text;
//using System.Text.Json;
//using SGMC.Application.Dto.Users;
//using SGMC.Web.Models;

//namespace SGMC.Web.Services
//{
//    public class PatientApiClient : IPatientApiClient
//    {
//        private readonly HttpClient _httpClient;
//        private readonly JsonSerializerOptions _jsonOptions;

//        public PatientApiClient(HttpClient httpClient)
//        {
//            _httpClient = httpClient;
//            _jsonOptions = new JsonSerializerOptions
//            {
//                PropertyNameCaseInsensitive = true
//            };
//        }

//        public async Task<ApiResponse<List<PatientDto>>> GetAllAsync()
//        {
//            var response = await _httpClient.GetAsync("Patients");
//            return await HandleResponse<List<PatientDto>>(response);
//        }

//        public async Task<ApiResponse<PatientDto>> GetByIdAsync(int id)
//        {
//            var response = await _httpClient.GetAsync($"Patients/{id}");
//            return await HandleResponse<PatientDto>(response);
//        }

//        public async Task<ApiResponse<PatientDto>> CreateAsync(RegisterPatientDto dto)
//        {
//            var json = JsonSerializer.Serialize(dto, _jsonOptions);
//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            var response = await _httpClient.PostAsync("Patients", content);
//            return await HandleResponse<PatientDto>(response);
//        }

//        public async Task<ApiResponse<PatientDto>> UpdateAsync(UpdatePatientDto dto)
//        {
//            var json = JsonSerializer.Serialize(dto, _jsonOptions);
//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            var response = await _httpClient.PutAsync($"Patients/{dto.PatientId}", content);
//            return await HandleResponse<PatientDto>(response);
//        }

//        public async Task<ApiResponse<bool>> DeleteAsync(int id)
//        {
//            var response = await _httpClient.DeleteAsync($"Patients/{id}");

//            if (!response.IsSuccessStatusCode)
//            {
//                return new ApiResponse<bool>
//                {
//                    Success = false,
//                    ErrorMessage = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}"
//                };
//            }

//            return new ApiResponse<bool>
//            {
//                Success = true,
//                Data = true
//            };
//        }

//        private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
//        {
//            var apiResponse = new ApiResponse<T>();

//            if (!response.IsSuccessStatusCode)
//            {
//                apiResponse.Success = false;
//                apiResponse.ErrorMessage = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
//                return apiResponse;
//            }

//            var content = await response.Content.ReadAsStringAsync();

//            if (string.IsNullOrWhiteSpace(content))
//            {
//                apiResponse.Success = false;
//                apiResponse.ErrorMessage = "Empty response from API.";
//                return apiResponse;
//            }

//            var opResult = JsonSerializer.Deserialize<OperationResultDto<T>>(content, _jsonOptions);

//            if (opResult == null)
//            {
//                apiResponse.Success = false;
//                apiResponse.ErrorMessage = "Could not parse API response.";
//                return apiResponse;
//            }

//            apiResponse.Success = opResult.Exitoso;
//            apiResponse.ErrorMessage = opResult.Mensaje;
//            apiResponse.Data = opResult.Datos;

//            return apiResponse;
//        }
//    }
//}

using System.Net.Http;
using System.Text;
using System.Text.Json;
using SGMC.Application.Dto.Users;
using SGMC.Web.Models;

namespace SGMC.Web.Services
{
    public class PatientApiClient : IPatientApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public PatientApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        public async Task<ApiResponse<List<PatientDto>>> GetAllAsync()
        {
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("LLAMANDO A: GET Patients");
                Console.WriteLine("========================================");

                var response = await _httpClient.GetAsync("Patients");

                Console.WriteLine($"STATUS CODE: {(int)response.StatusCode} {response.StatusCode}");
                Console.WriteLine($"CONTENT TYPE: {response.Content.Headers.ContentType}");

                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine("========================================");
                Console.WriteLine("RAW API RESPONSE:");
                Console.WriteLine(content);
                Console.WriteLine("========================================");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"ERROR: HTTP {response.StatusCode}");
                    return new ApiResponse<List<PatientDto>>
                    {
                        Success = false,
                        ErrorMessage = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}"
                    };
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    Console.WriteLine("ERROR: Respuesta vacía");
                    return new ApiResponse<List<PatientDto>>
                    {
                        Success = false,
                        ErrorMessage = "Empty response from API."
                    };
                }

                var opResult = JsonSerializer.Deserialize<OperationResultDto<List<PatientDto>>>(content, _jsonOptions);

                Console.WriteLine("========================================");
                Console.WriteLine("DESERIALIZACIÓN:");
                Console.WriteLine($"  opResult == null? {opResult == null}");
                if (opResult != null)
                {
                    Console.WriteLine($"  Exitoso: {opResult.Exitoso}");
                    Console.WriteLine($"  Mensaje: {opResult.Mensaje}");
                    Console.WriteLine($"  Datos == null? {opResult.Datos == null}");
                    if (opResult.Datos != null)
                    {
                        Console.WriteLine($"  Datos.Count: {opResult.Datos.Count}");
                        Console.WriteLine($"  Primer paciente ID: {opResult.Datos.FirstOrDefault()?.PatientId}");
                        Console.WriteLine($"  Primer paciente Nombre: {opResult.Datos.FirstOrDefault()?.FullName}");
                    }
                }
                Console.WriteLine("========================================");

                if (opResult == null)
                {
                    return new ApiResponse<List<PatientDto>>
                    {
                        Success = false,
                        ErrorMessage = "Could not parse API response."
                    };
                }

                return new ApiResponse<List<PatientDto>>
                {
                    Success = opResult.Exitoso,
                    ErrorMessage = opResult.Mensaje,
                    Data = opResult.Datos
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("========================================");
                Console.WriteLine($"EXCEPTION: {ex.GetType().Name}");
                Console.WriteLine($"MESSAGE: {ex.Message}");
                Console.WriteLine($"INNER EXCEPTION: {ex.InnerException?.Message}");
                Console.WriteLine($"STACK TRACE:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("========================================");

                return new ApiResponse<List<PatientDto>>
                {
                    Success = false,
                    ErrorMessage = $"Exception: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<PatientDto>> GetByIdAsync(int id)
        {
            try
            {
                Console.WriteLine($"========================================");
                Console.WriteLine($"LLAMANDO A: GET Patients/{id}");
                Console.WriteLine($"========================================");

                var response = await _httpClient.GetAsync($"Patients/{id}");
                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"STATUS: {response.StatusCode}");
                Console.WriteLine($"RESPONSE: {content}");
                Console.WriteLine($"========================================");

                return await HandleResponse<PatientDto>(response, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en GetByIdAsync: {ex.Message}");
                return new ApiResponse<PatientDto>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<PatientDto>> CreateAsync(RegisterPatientDto dto)
        {
            try
            {
                Console.WriteLine($"========================================");
                Console.WriteLine($"LLAMANDO A: POST Patients");
                Console.WriteLine($"========================================");

                var json = JsonSerializer.Serialize(dto, _jsonOptions);
                Console.WriteLine($"REQUEST BODY:");
                Console.WriteLine(json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("Patients", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"STATUS: {response.StatusCode}");
                Console.WriteLine($"RESPONSE: {responseContent}");
                Console.WriteLine($"========================================");

                return await HandleResponse<PatientDto>(response, responseContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en CreateAsync: {ex.Message}");
                return new ApiResponse<PatientDto>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<PatientDto>> UpdateAsync(UpdatePatientDto dto)
        {
            try
            {
                Console.WriteLine($"========================================");
                Console.WriteLine($"LLAMANDO A: PUT Patients/{dto.PatientId}");
                Console.WriteLine($"========================================");

                var json = JsonSerializer.Serialize(dto, _jsonOptions);
                Console.WriteLine($"REQUEST BODY:");
                Console.WriteLine(json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"Patients/{dto.PatientId}", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"STATUS: {response.StatusCode}");
                Console.WriteLine($"RESPONSE: {responseContent}");
                Console.WriteLine($"========================================");

                return await HandleResponse<PatientDto>(response, responseContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en UpdateAsync: {ex.Message}");
                return new ApiResponse<PatientDto>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                Console.WriteLine($"========================================");
                Console.WriteLine($"LLAMANDO A: DELETE Patients/{id}");
                Console.WriteLine($"========================================");

                var response = await _httpClient.DeleteAsync($"Patients/{id}");
                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"STATUS: {response.StatusCode}");
                Console.WriteLine($"RESPONSE: {content}");
                Console.WriteLine($"========================================");

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        ErrorMessage = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}"
                    };
                }

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en DeleteAsync: {ex.Message}");
                return new ApiResponse<bool>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response, string? content = null)
        {
            var apiResponse = new ApiResponse<T>();

            if (content == null)
            {
                content = await response.Content.ReadAsStringAsync();
            }

            if (!response.IsSuccessStatusCode)
            {
                apiResponse.Success = false;
                apiResponse.ErrorMessage = ExtractErrorMessage(content, response);
                return apiResponse;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                apiResponse.Success = false;
                apiResponse.ErrorMessage = "Empty response from API.";
                return apiResponse;
            }

            try
            {
                var opResult = JsonSerializer.Deserialize<OperationResultDto<T>>(content, _jsonOptions);

                if (opResult == null)
                {
                    apiResponse.Success = false;
                    apiResponse.ErrorMessage = "Could not parse API response.";
                    return apiResponse;
                }

                apiResponse.Success = opResult.Exitoso;
                apiResponse.ErrorMessage = opResult.Mensaje;
                apiResponse.Data = opResult.Datos;

                return apiResponse;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON DESERIALIZATION ERROR: {ex.Message}");
                apiResponse.Success = false;
                apiResponse.ErrorMessage = $"JSON Error: {ex.Message}";
                return apiResponse;
            }
        }

        // Intenta extraer el mensaje y la lista de "errores" del cuerpo de una respuesta
        // no exitosa (400, 409, etc.); si el cuerpo no viene en el formato esperado,
        // cae de vuelta al mensaje genérico HTTP.
        private static string ExtractErrorMessage(string? content, HttpResponseMessage response)
        {
            var fallback = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";

            if (string.IsNullOrWhiteSpace(content))
                return fallback;

            try
            {
                var opResult = JsonSerializer.Deserialize<OperationResultDto<object>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (opResult == null)
                    return fallback;

                if (opResult.Errores != null && opResult.Errores.Count > 0)
                    return string.Join(" ", opResult.Errores);

                if (!string.IsNullOrWhiteSpace(opResult.Mensaje))
                    return opResult.Mensaje;

                return fallback;
            }
            catch (JsonException)
            {
                return fallback;
            }
        }
    }
}