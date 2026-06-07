namespace DocumentHub.Domain.Exceptions;

public class DuplicateEmailException() : Exception("A user with this email already exists.");
