namespace sisapi.domain.Abstractions;

/// <summary>
/// Interface for verifying user permissions
/// Used to break circular dependency between infrastructure and application layers
/// </summary>
public interface IPermissionVerifier
{
    /// <summary>
    /// Verify if a user has permission for a specific module, controller, and action
    /// </summary>
    /// <param name="userId">The user ID to verify</param>
    /// <param name="module">The module name (e.g., SISAPI, SIGA)</param>
    /// <param name="controller">The controller name (e.g., User, Company)</param>
    /// <param name="action">The action type (Read, Write, Update, Delete)</param>
    /// <returns>True if user has permission, false otherwise</returns>
    Task<bool> HasPermissionAsync(int userId, string module, string controller, string action);
}

