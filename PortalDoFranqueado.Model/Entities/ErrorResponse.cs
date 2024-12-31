namespace PortalDoFranqueado.Model.Entities
{
    public class ErrorResponse(string message)
    {
        public string Message { get; set; } = message;

        public ErrorResponse()
            : this(string.Empty)
        { }
    }
}