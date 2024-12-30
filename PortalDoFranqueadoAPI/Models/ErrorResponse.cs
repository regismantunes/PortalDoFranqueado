namespace PortalDoFranqueadoAPI.Models
{
    public class ErrorResponse(string message)
    {
        public string Message { get; set; } = message;

        public ErrorResponse()
            : this(string.Empty) 
        { }
    }
}