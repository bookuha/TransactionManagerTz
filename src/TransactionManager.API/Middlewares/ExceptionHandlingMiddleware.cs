using System.Net;
using System.Security.Authentication;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TransactionManager.Logic.Exceptions;

namespace TransactionManager.API.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            await HttpExceptionHandlingUtilities.WriteExceptionToContextAsync(context, ex);
        }
    }
}

public static class HttpExceptionHandlingUtilities
{
    public static async Task WriteExceptionToContextAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        switch (exception)
        {
            case TransactionManagerException tmException:
            {
                context.Response.StatusCode = (int) ResolveHttpStatusCode(tmException);
                var problemDetails = new ProblemDetails
                {
                    Status = (int) ResolveHttpStatusCode(tmException),
                    Type = $"{tmException.EntityName}.{tmException.Error}",
                    Title = tmException.Title,
                    Detail = exception.Message
                };

                var json = JsonSerializer.Serialize(problemDetails);
                await context.Response.WriteAsync(json);
                break;
            }
            default:
            {
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

                ProblemDetails problem = new()
                {
                    Status = (int) HttpStatusCode.InternalServerError,
                    Type = "InternalServerError",
                    Title = "Internal server error.",
                    Detail = "A critical internal server error occurred."
                };

                var json = JsonSerializer.Serialize(problem);
                await context.Response.WriteAsync(json);
                break;
            }
        }
    }

    private static HttpStatusCode ResolveHttpStatusCode(TransactionManagerException exception)
    {
        return exception.Error switch
        {
            Errors.WrongFlow => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.NotImplemented
        };
    }
}