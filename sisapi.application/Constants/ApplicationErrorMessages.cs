namespace sisapi.application.Constants;

public static class ApplicationErrorMessages
{
    public const string UsuarioNoEncontrado = "Usuario no encontrado";
    public const string UsuarioNoEliminado = "Usuario no encontrado o no está eliminado";
    public const string UsernameDuplicado = "Ya existe un usuario con ese nombre de usuario";
    public const string EmailDuplicado = "Ya existe un usuario con ese correo electrónico";
    public const string CrearUsuarioError = "No se pudo crear el usuario";
    public const string ActualizarUsuarioError = "No se pudo actualizar el usuario";
    public const string EliminarUsuarioError = "No se pudo eliminar el usuario";
    public const string RestaurarUsuarioError = "No se pudo restaurar el usuario";
    public const string ObtenerUsuariosError = "No se pudo obtener la lista de usuarios";
    public const string ObtenerUsuarioError = "No se pudo obtener el usuario";
    public const string ObtenerUsuariosEmpresaError = "No se pudo obtener los usuarios de la empresa";
    public const string UsuarioYaTieneRol = "El usuario ya tiene este rol";
    public const string UsuarioNoTieneRol = "El usuario no tiene este rol";
    public const string RolNoExiste = "El rol no existe";
    public const string AsignarRolError = "No se pudo asignar el rol";
    public const string RemoverRolError = "No se pudo remover el rol";

    public const string InteresadoNoEncontrado = "Interesado no encontrado";
    public const string InteresadoYaConvertido = "Este interesado ya fue convertido";
    public const string InteresadoEmailDuplicado = "Ya existe un usuario con este correo";
    public const string InteresadoCreacionError = "No se pudo registrar el interesado";
    public const string InteresadosConsultaError = "No se pudo obtener la lista de interesados";
    public const string InteresadoConsultaError = "No se pudo obtener el interesado";
    public const string InteresadoConversionError = "No se pudo convertir el interesado a usuario";
    public const string ConfirmacionPasswordInvalida = "La contraseña y su confirmación no coinciden";

    public const string EmpresaNoEncontrada = "Empresa no encontrada";
    public const string EmpresaNombreDuplicado = "Ya existe una empresa con ese nombre";
    public const string EmpresaNitDuplicado = "Ya existe una empresa con ese NIT";
    public const string EmpresaEmailDuplicado = "Ya existe una empresa con ese correo electrónico";
    public const string EmpresaCreacionError = "No se pudo crear la empresa";
    public const string EmpresaActualizacionError = "No se pudo actualizar la empresa";
    public const string EmpresaEliminacionError = "No se pudo eliminar la empresa";
    public const string EmpresaUsuarioAsignacionError = "No se pudo asignar el usuario a la empresa";
    public const string DatosInvalidos = "Los datos proporcionados son inválidos";

    public const string PermisoNoEncontrado = "Permiso no encontrado";
    public const string PermisoRolNoEncontrado = "Permiso del rol no encontrado";
    public const string PermisoYaAsignado = "El permiso ya está asignado a este rol";
    public const string EmpresaIdRequerido = "Debe proporcionar la empresa del usuario";
}
