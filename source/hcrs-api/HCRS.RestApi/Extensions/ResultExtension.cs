using HCRS.Application.Results;
using HCRS.RestApi.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HCRS.RestApi.Extensions;

public static class ResultExtension
{
    public static IActionResult ToActionResult<T>(this Result<T> result,HttpContext httpContext)
    {
        if (!result.IsSuccess)
        {
            var response = new ApiResponse
            {
                IsSuccess = false,
                Message = result.Error!.Message,
            };

            return new ObjectResult(response)
            {
                StatusCode = ErrorMappings.ToStatusCode(result.Error)
            };
        }
        return new OkObjectResult
        (
            new ApiResponse<T>
            {
                IsSuccess = true,
                Data = result.Data,
                Message = "Request Successfull",
            }
        );
    }
}
