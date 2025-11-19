namespace MyTodos.Services.IdentityService.Domain.UserAggregate.Constants;

public static class UserInvitationConstants
{
    public static class FieldLengths
    {
        public const int EmailMaxLength = 256;
        public const int InvitationTokenMaxLength = 100;
    }

    public static class ErrorMessages
    {
        public const string EmailRequired = "Email is required";
        public const string EmailTooLong = "Email cannot exceed {0} characters";
        public const string InvitationTokenRequired = "Invitation token is required";
        public const string InvalidOrExpiredInvitation = "Invalid or expired invitation";
    }
}
