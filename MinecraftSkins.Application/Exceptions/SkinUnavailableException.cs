namespace MinecraftSkins.Application.Exceptions;

public class SkinUnavailableException : Exception
{
    public Guid SkinId { get; }
    public string Reason { get; }
    
    public SkinUnavailableException(Guid skinId, string reason = "Skin is not available for purchase") 
        : base($"Skin with ID {skinId} is not available: {reason}")
    {
        SkinId = skinId;
        Reason = reason;
    }
}