using HCRS.Application.Results;
using HCRS.Application.Results.Errors;

namespace HCRS.RestApi.Contracts;

public static class ErrorMappings
{
    public static int ToStatusCode(Error error)
    {
        if (error == AuthenticationErrors.UserNotFound)
            return StatusCodes.Status401Unauthorized;
    
        if (error == AuthenticationErrors.UserAccountSuspended)
            return StatusCodes.Status401Unauthorized;
    
        return StatusCodes.Status500InternalServerError;
    }
}
