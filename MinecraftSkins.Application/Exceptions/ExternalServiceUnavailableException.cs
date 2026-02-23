namespace MinecraftSkins.Application.Exceptions;

public class ExternalServiceUnavailableException : Exception
{
    public string ServiceName { get; }
    
    public ExternalServiceUnavailableException(string serviceName, string message, Exception innerException) 
        : base(message, innerException)
    {
        ServiceName = serviceName;
    }
    
    public ExternalServiceUnavailableException(string serviceName, string message) 
        : base(message)
    {
        ServiceName = serviceName;
    }
}