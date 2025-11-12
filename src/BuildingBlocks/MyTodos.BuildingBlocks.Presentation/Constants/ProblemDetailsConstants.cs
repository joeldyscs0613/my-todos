namespace MyTodos.BuildingBlocks.Presentation.Constants;

public struct ProblemDetailsConstants
{
    public struct Titles
    {
        public const string BadRequest = "Bad Request";
        public const string NotFound = "Not Found";
        public const string Conflict = "Conflict";
        public const string Unauthorized = "Unauthorized";
        public const string Forbidden = "Forbidden";
        public const string UnprocessableEntity = "Unprocessable Entity";
        public const string InternalServerError = "Internal Server Error";
    }
    
    public struct Types
    {
        public const string BadRequest = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
        public const string NotFound = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
        public const string Conflict = "https://tools.ietf.org/html/rfc7231#section-6.5.8";
        public const string Unauthorized = "https://tools.ietf.org/html/rfc7235#section-3.1";
        public const string Forbidden = "https://tools.ietf.org/html/rfc7231#section-6.5.3";
        public const string UnprocessableEntity = "https://tools.ietf.org/html/rfc4918#section-11.2";
        public const string InternalServerError = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
    }
}