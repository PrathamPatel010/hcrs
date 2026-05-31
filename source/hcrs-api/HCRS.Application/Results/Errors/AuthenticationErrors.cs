namespace HCRS.Application.Results.Errors;

public static class AuthenticationErrors
{
    public static readonly Error UserNotFound = new("Authentication.UserNotFound", "Invalid credentials");
    public static readonly Error UserAccountSuspended = new("Authentication.UserAccountSuspended", "User account is suspended");
}
