using Bite.Services.Common;
using Microsoft.AspNetCore.Mvc;

namespace Bite.Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult HandleFailure<T>(ServiceResult<T> result)
        => result.ResultType switch
        {
            ServiceResultType.NotFound => NotFound(new { message = result.ErrorMessage }),
            ServiceResultType.ValidationError => UnprocessableEntity(new { message = result.ErrorMessage }),
            ServiceResultType.Conflict => Conflict(new { message = result.ErrorMessage }),
            _ => BadRequest(new { message = result.ErrorMessage })
        };
}
