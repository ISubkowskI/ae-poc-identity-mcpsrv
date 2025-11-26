using Ae.Poc.Identity.Mcp.Dtos;

namespace Ae.Poc.Identity.Mcp.Data
{
    public static class ToolResultFactory
    {
        public static ToolResult<T, ErrorOutgoingDto> Success<T>(T value) => new()
        {
            IsSuccess = true,
            Value = value,
            Error = null
        };

        public static ToolResult<T, ErrorOutgoingDto> ValidationFailed<T>(IEnumerable<string> validationWarnings, string status = "Validation Failed")
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

        public static ToolResult<T, ErrorOutgoingDto> Warning<T>(string message, string status = "Warning") => new()
        {
            IsSuccess = false,
            Value = default,
            Error = new ErrorOutgoingDto
            {
                Errors = [message ?? "Unknown warning message."],
                Status = status
            }
        };


        public static ToolResult<T, ErrorOutgoingDto> Failure<T>(IEnumerable<string> errors, string status = "Error")
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

        public static ToolResult<T, ErrorOutgoingDto> FromException<T>(Exception ex) => new()
        {
            IsSuccess = false,
            Value = default,
            Error = new ErrorOutgoingDto
            {
                Errors = [ex?.Message ?? "Unknown exception message."],
                Status = "Error"
            }
        };

    }
}
