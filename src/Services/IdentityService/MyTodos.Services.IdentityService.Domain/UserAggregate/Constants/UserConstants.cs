namespace MyTodos.Services.IdentityService.Domain.UserAggregate.Constants;

public static class UserConstants
{
    public static class FieldLengths
    {
        public const int UsernameMaxLength = 100;
        public const int EmailMaxLength = 256;
        public const int PasswordHashMaxLength = 500;
        public const int FirstNameMaxLength = 100;
        public const int LastNameMaxLength = 100;
    }

    public static class ErrorMessages
    {
        public const string UsernameRequired = "Username is required";
        public const string UsernameTooLong = "Username cannot exceed {0} characters";
        public const string EmailRequired = "Email is required";
        public const string EmailInvalid = "Email is not valid";
        public const string EmailTooLong = "Email cannot exceed {0} characters";
        public const string PasswordRequired = "Password is required";
        public const string PasswordTooShort = "Password must be at least {0} characters";
        public const string FirstNameTooLong = "First name cannot exceed {0} characters";
        public const string LastNameTooLong = "Last name cannot exceed {0} characters";
        public const string InvalidCredentials = "Invalid username or password";
        public const string UserNotFound = "User not found";
        public const string UserAlreadyExists = "User already exists";
        public const string EmailAlreadyExists = "Email already exists";
    }
}