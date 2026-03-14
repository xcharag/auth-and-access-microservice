namespace sisapi.application.Constants;

public static class AuthMessages
{
    // Success messages
    public const string LoginSuccess = "Inicio de sesión exitoso";
    public const string LogoutSuccess = "Cierre de sesión exitoso";
    public const string RegisterSuccess = "Usuario registrado exitosamente";
    public const string TokenRefreshed = "Token refrescado exitosamente";
    public const string PasswordResetSuccess = "Contraseña restablecida exitosamente";
    public const string UserRestored = "Usuario restaurado exitosamente";
    public const string UserDeleted = "Usuario eliminado exitosamente";

    // Error messages
    public const string InvalidCredentials = "Email o contraseña inválidos";
    public const string UserNotFound = "Usuario no encontrado";
    public const string UserNotDeleted = "El usuario no está eliminado";
    public const string InvalidToken = "Token inválido o expirado";
    public const string RefreshTokenNotFound = "Token de actualización no encontrado";
    public const string RefreshTokenExpired = "El token de actualización ha expirado";
    public const string RegistrationFailed = "Registro de usuario fallido";
    public const string UpdateUserFailed = "Actualización de usuario fallida";
    public const string UnauthorizedAccess = "Acceso no autorizado";
    public const string InternalServerError = "Un error interno del servidor ocurrió";
    public const string UserNotActive = "El usuario no está activo o ha sido eliminado";
    public const string PermissionGranted = "Permiso concedido";
    public const string PermissionDenied = "Permiso denegado";
}

