namespace MyTodos.Services.IdentityService.Domain.TenantAggregate.Constants;

public static class TenantConstants
{
    public static class FieldLengths
    {
        public const int NameMaxLength = 200;
    }

    public static class Invariants
    {
    }

    public static class ErrorMessages
    {
        public const string NameRequired = "Name is required";
        public const string NameTooLong = "Name cannot exceed {0} characters";
        public const string TenantNotFound = "Tenant not found";
        public const string TenantAlreadyExists = "Tenant already exists";
        public const string MaxUsersExceeded = "Maximum number of users exceeded for this tenant's plan";
    }
}