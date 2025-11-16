using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Domain.Errors;

public static class AuthErrors
{
    // ==================== AUTHENTICATION ERRORS (7000-7999) ====================

    public static readonly ErrorCode InvalidCredentials =
        ErrorCode.Create(ErrorCategory.Authentication, 7001);

    public static readonly ErrorCode AccountInactive =
        ErrorCode.Create(ErrorCategory.Authentication, 7002);

    public static readonly ErrorCode EmailNotConfirmed =
        ErrorCode.Create(ErrorCategory.Authentication, 7003);

    public static readonly ErrorCode AccountLocked =
        ErrorCode.Create(ErrorCategory.Authentication, 7004);

    public static readonly ErrorCode InvalidRefreshToken =
        ErrorCode.Create(ErrorCategory.Authentication, 7005);

    public static readonly ErrorCode WeakPassword =
        ErrorCode.Create(ErrorCategory.Authentication, 7006);

    public static readonly ErrorCode PasswordMismatch =
        ErrorCode.Create(ErrorCategory.Authentication, 7007);

    public static readonly ErrorCode InvalidOldPassword =
        ErrorCode.Create(ErrorCategory.Authentication, 7008);

    public static readonly ErrorCode InvalidOtp =
        ErrorCode.Create(ErrorCategory.Authentication, 7009);

    public static readonly ErrorCode ExpiredOtp =
        ErrorCode.Create(ErrorCategory.Authentication, 7010);

    // ==================== API SIGNATURE ERRORS (7011-7020) ====================

    public static readonly ErrorCode MissingSignature =
        ErrorCode.Create(ErrorCategory.Authentication, 7011);

    public static readonly ErrorCode InvalidSignature =
        ErrorCode.Create(ErrorCategory.Authentication, 7012);

    public static readonly ErrorCode ExpiredSignature =
        ErrorCode.Create(ErrorCategory.Authentication, 7013);

    public static readonly ErrorCode MissingTimestamp =
        ErrorCode.Create(ErrorCategory.Authentication, 7014);

    // ==================== REGISTRATION ERRORS (7101-7199) ====================

    public static readonly ErrorCode EmailExists =
        ErrorCode.Create(ErrorCategory.Authentication, 7101);

    public static readonly ErrorCode UsernameExists =
        ErrorCode.Create(ErrorCategory.Authentication, 7102);

    public static readonly ErrorCode UserCreationFailed =
        ErrorCode.Create(ErrorCategory.Authentication, 7103);

    // ==================== AUTHORIZATION ERRORS (7201-7299) ====================

    public static readonly ErrorCode Forbidden =
        ErrorCode.Create(ErrorCategory.Authentication, 7201);
}