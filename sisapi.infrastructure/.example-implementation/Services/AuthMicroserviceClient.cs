using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace sisconapi.infrastructure.Services;

/// <summary>
/// Service for communicating with the authentication microservice.
/// Use this to validate tokens, fetch user details, or check permissions remotely.
/// </summary>
public interface IAuthMicroserviceClient
{
    Task<bool> ValidateTokenAsync(string token);
    Task<UserInfo?> GetCurrentUserAsync(string token);
    Task<IEnumerable<string>> GetUserPermissionsAsync(string token);
    Task<bool> VerifyPermissionAsync(string token, string module, string controller, string action, int typePermission = 0);
}

public class AuthMicroserviceClient : IAuthMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthMicroserviceClient> _logger;

    public AuthMicroserviceClient(HttpClient httpClient, ILogger<AuthMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync("/api/auth/validate");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token with auth microservice");
            return false;
        }
    }

    public async Task<UserInfo?> GetCurrentUserAsync(string token)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync("/api/auth/me");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<UserInfo>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user info from auth microservice");
            return null;
        }
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(string token)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync("/api/auth/permissions");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<string>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? Enumerable.Empty<string>();
            }
            
            return Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user permissions from auth microservice");
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// Verifies if the current user has the specified permission by calling the auth microservice.
    /// This is the recommended approach to avoid JWT token bloat.
    /// </summary>
    /// <param name="token">JWT bearer token</param>
    /// <param name="module">Module name (e.g., "SISCON", "SISAPI")</param>
    /// <param name="controller">Controller name (e.g., "User", "Company")</param>
    /// <param name="action">Action name (Read, Write, Update, Delete)</param>
    /// <param name="typePermission">Type of permission: 0=ControllerAction, 1=MenuOption, 2=UserAction, 3=ProjectView</param>
    /// <returns>True if the user has the permission, false otherwise</returns>
    public async Task<bool> VerifyPermissionAsync(string token, string module, string controller, string action, int typePermission = 0)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var url = $"/api/Auth/verify-permission?module={module}&controller={controller}&action={action}&typePermission={typePermission}";
            
            _logger.LogDebug("Verifying permission: {Url}", url);
            
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PermissionVerificationResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return result?.Success == true && result?.Data == true;
            }
            
            _logger.LogWarning("Permission verification failed with status code: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying permission with auth microservice - Module: {Module}, Controller: {Controller}, Action: {Action}", 
                module, controller, action);
            return false;
        }
    }
}

/// <summary>
/// User information from the auth microservice
/// </summary>
public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<string> Permissions { get; set; } = Enumerable.Empty<string>();
}

/// <summary>
/// Response from the verify-permission endpoint
/// </summary>
public class PermissionVerificationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool Data { get; set; }
    public List<string>? Errors { get; set; }
}

