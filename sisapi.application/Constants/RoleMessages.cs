namespace sisapi.application.Constants;

public static class RoleMessages
{
    // Success messages
    public const string RoleCreated = "Role created successfully";
    public const string RoleUpdated = "Role updated successfully";
    public const string RoleDeleted = "Role deleted successfully";
    public const string RoleRetrieved = "Role retrieved successfully";
    public const string RolesRetrieved = "Roles retrieved successfully";

    // Error messages
    public const string RoleNotFound = "Role not found";
    public const string RoleAlreadyExists = "Role with this name already exists";
    public const string RoleCreationFailed = "Failed to create role";
    public const string RoleUpdateFailed = "Failed to update role";
    public const string RoleDeletionFailed = "Failed to delete role";
    public const string InvalidRoleData = "Invalid role data provided";
    public const string RoleInUse = "Cannot delete role as it is assigned to users";
}

