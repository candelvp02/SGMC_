namespace SGMC.Web.Models
{
    public class OperationResultDto<T>
    {
        public bool Exitoso { get; set; }
        public string? Mensaje { get; set; }
        public T? Datos { get; set; }
        public List<string>? Errores { get; set; }
    }
}