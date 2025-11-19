namespace MyTodos.Services.TodoService.Domain.TaskAggregate.Constants;

public static class TaskConstants
{
    public static class FieldLengths
    {
        public const int TitleMaxLength = 200;
        public const int DescriptionMaxLength = 2000;

        // TaskAttachment field lengths
        public const int AttachmentFileNameMaxLength = 255;
        public const int AttachmentUrlOrStorageKeyMaxLength = 500;
        public const int AttachmentContentTypeMaxLength = 100;

        // TaskComment field lengths
        public const int CommentTextMaxLength = 2000;
    }

    public static class ErrorMessages
    {
        public const string TitleRequired = "Title is required";
        public const string TitleTooLong = "Title cannot exceed {0} characters";
        public const string DescriptionTooLong = "Description cannot exceed {0} characters";
        public const string AssigneeMustBelongToOrganization = "Assignee must belong to the same organization";
        public const string ProjectMustBelongToOrganization = "Project must belong to the same organization";
        public const string TagNameRequired = "Tag name is required";
        public const string TagNameTooLong = "Tag name cannot exceed {0} characters";
        public const string CommentTextRequired = "Comment text is required";
        public const string AttachmentFileNameRequired = "Attachment file name is required";
    }
}
