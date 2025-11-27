using Ae.Poc.Identity.Mcp.Dtos;

namespace Ae.Poc.Identity.Mcp.Data
{
    public static class ToolResultFactory
    {
        /// <summary>
        /// Creates a successful tool result with the specified value.
        /// </summary>
        /// <typeparam name="T">The type of the success value.</typeparam>
        /// <param name="value">The value to return on success.</param>
        /// <returns>A successful <see cref="ToolResult{T, ErrorOutgoingDto}"/> containing the value.</returns>
        public static ToolResult<T, ErrorOutgoingDto> Success<T>(T value) => new()
        {
            IsSuccess = true,
            Value = value,
            Error = null
        };

        /// <summary>
        /// Creates a failed tool result with validation errors.
        /// </summary>
        /// <typeparam name="T">The type of the expected success value.</typeparam>
        /// <param name="validationWarnings">Collection of validation error messages.</param>
        /// <param name="status">The status message (default: "Validation Failed").</param>
        /// <returns>A failed <see cref="ToolResult{T, ErrorOutgoingDto}"/> containing validation errors.</returns>
        /// <exception cref="ArgumentException">Thrown when validationWarnings is null or empty.</exception>
        public static ToolResult<T, ErrorOutgoingDto> ValidationFailed<T>(IEnumerable<string> validationWarnings, string status = ToolResultStatus.ValidationFailed)
        {
            if (validationWarnings == null || !validationWarnings.Any())
            {
                throw new ArgumentException("Validation warnings collection must contain at least one message.", nameof(validationWarnings));
            }

            return new()
            {
                IsSuccess = false,
                Value = default,
                Error = new ErrorOutgoingDto
                {
                    Errors = [.. validationWarnings.Select(r => r ?? "Unknown validation warning.")],
                    Status = status
                }
            };
        }

        public static ToolResult<T, ErrorOutgoingDto> Warning<T>(string message, string status = ToolResultStatus.Warning) => new()
        {
            IsSuccess = false,
            Value = default,
            Error = new ErrorOutgoingDto
            {
                Errors = [message ?? "Unknown warning message."],
                Status = status
            }
        };


        public static ToolResult<T, ErrorOutgoingDto> Failure<T>(IEnumerable<string> errors, string status = ToolResultStatus.Error)
        {
            if (errors == null || !errors.Any())
            {
                throw new ArgumentException("Errors collection must contain at least one error message.", nameof(errors));
            }

            return new()
            {
                IsSuccess = false,
                Value = default,
                Error = new ErrorOutgoingDto
                {
                    Errors = [.. errors.Select(r => r ?? "Unknown error.")],
                    Status = status
                }
            };
        }

        public static ToolResult<T, ErrorOutgoingDto> FromException<T>(Exception ex, bool includeStackTrace = false) => new()
        {
            IsSuccess = false,
            Value = default,
            Error = new ErrorOutgoingDto
            {
                Errors = includeStackTrace
                ? [ex?.ToString() ?? "Unknown exception."]
                : [$"{ex?.GetType().Name}: {ex?.Message ?? "Unknown exception."}"],
                Status = ToolResultStatus.Error
            }
        };

    }
}
