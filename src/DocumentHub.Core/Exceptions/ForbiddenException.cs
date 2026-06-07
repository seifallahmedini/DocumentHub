namespace DocumentHub.Core.Exceptions;

public class ForbiddenException(string message = "Access denied.") : Exception(message);
