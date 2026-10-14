namespace MinecraftSkins.Application.Exceptions;

public sealed class IdempotencyKeyConflictException : Exception
{
    public IdempotencyKeyConflictException()
        : base("The idempotency key has already been used by another buyer.")
    {
    }
}
