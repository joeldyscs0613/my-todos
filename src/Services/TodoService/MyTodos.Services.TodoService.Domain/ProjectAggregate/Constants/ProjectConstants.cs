namespace MyTodos.Services.TodoService.Domain.ProjectAggregate.Constants;

public static class ProjectConstants
{
    public static class FieldLengths
    {
        public const int NameMaxLength = 200;
        public const int DescriptionMaxLength = 2000;
        public const int ColorMaxLength = 7;  // Hex color code: #RRGGBB
        public const int IconMaxLength = 50;
    }

    public static class ErrorMessages
    {
        public const string NameRequired = "Name is required";
        public const string NameTooLong = "Name cannot exceed {0} characters";
        public const string DescriptionTooLong = "Description cannot exceed {0} characters";
        public const string ColorTooLong = "Color cannot exceed {0} characters";
        public const string IconTooLong = "Icon cannot exceed {0} characters";
        public const string AssigneeMustBelongToOrganization = "Assignee must belong to the same organization as the project";
    }
}
