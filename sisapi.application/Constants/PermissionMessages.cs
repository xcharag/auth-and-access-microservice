namespace sisapi.application.Constants;

public static class PermissionMessages
{
    // Success messages
    public const string PermissionCreated = "Permission created successfully";
    public const string PermissionUpdated = "Permission updated successfully";
    public const string PermissionDeleted = "Permission deleted successfully";
    public const string PermissionRetrieved = "Permission retrieved successfully";
    public const string PermissionsRetrieved = "Permissions retrieved successfully";

    // Error messages
    public const string PermissionNotFound = "Permission not found";
    public const string PermissionAlreadyExists = "Permission with this code already exists";
    public const string PermissionCreationFailed = "Failed to create permission";
    public const string PermissionUpdateFailed = "Failed to update permission";
    public const string PermissionDeletionFailed = "Failed to delete permission";
    public const string InvalidPermissionData = "Invalid permission data provided";
    public const string PermissionInUse = "Cannot delete permission as it is assigned to roles";
}

