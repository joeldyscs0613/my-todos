namespace MyTodos.Services.IdentityService.Domain.UserAggregate.Constants;

public struct UserConstants
{
    public struct FieldLengths
    {
        public const int UsernameMaxLength = 100;
        public const int EmailMaxLength = 256;
        public const int PasswordHashMaxLength = 500;
        public const int FirstNameMaxLength = 100;
        public const int LastNameMaxLength = 100;
    }
}