using Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace E_LearningPlatform.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<TValue>(this Result<TValue> result, ControllerBase controller) =>
        result.IsSuccess ? controller.Ok(result.Value) : ToErrorResult(result.Error, controller);

    public static IActionResult ToActionResult(this Result result, ControllerBase controller) =>
        result.IsSuccess ? controller.NoContent() : ToErrorResult(result.Error, controller);

    private static IActionResult ToErrorResult(Error error, ControllerBase controller) => error.Type switch
    {
        ErrorType.NotFound => controller.NotFound(error),
        ErrorType.Validation => controller.BadRequest(error),
        ErrorType.Conflict => controller.Conflict(error),
        ErrorType.Unauthorized => controller.Unauthorized(),
        ErrorType.Forbidden => controller.Forbid(),
        _ => controller.StatusCode(StatusCodes.Status500InternalServerError, error)
    };
}
