namespace MyTodos.Services.IdentityService.Domain.PermissionAggregate.Constants;

public static class PermissionConstants
{
    public static class FieldLengths
    {
        public const int CodeMaxLength = 200;
        public const int NameMaxLength = 200;
        public const int DescriptionMaxLength = 500;
    }

    public static class ErrorMessages
    {
        public const string CodeRequired = "Code is required";
        public const string CodeTooLong = "Code cannot exceed {0} characters";
        public const string NameRequired = "Name is required";
        public const string NameTooLong = "Name cannot exceed {0} characters";
        public const string DescriptionTooLong = "Description cannot exceed {0} characters";
    }
}