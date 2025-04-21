namespace TravelMemories.Core.DTOs
{
    public class ErrorDto
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public List<string> Details { get; set; } = new List<string>();

        public static ErrorDto FromException(string message, string type = "Error")
        {
            return new ErrorDto
            {
                Type = type,
                Message = message
            };
        }

        public static ErrorDto ValidationError(string message, List<string>? details = null)
        {
            return new ErrorDto
            {
                Type = "ValidationError",
                Message = message,
                Details = details ?? new List<string>()
            };
        }

        public static ErrorDto Unauthorized(string message = "You are not authorized to perform this action")
        {
            return new ErrorDto
            {
                Type = "UnauthorizedError",
                Message = message
            };
        }

        public static ErrorDto NotFound(string message = "Resource not found")
        {
            return new ErrorDto
            {
                Type = "NotFoundError",
                Message = message
            };
        }
    }
}