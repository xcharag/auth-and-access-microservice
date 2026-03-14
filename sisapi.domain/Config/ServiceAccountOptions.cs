namespace sisapi.domain.Config;

public class ServiceAccountOptions
{
    public const string SectionName = "ServiceAccounts";
    public List<ServiceAccountOptionsItem> Accounts { get; set; } = new();
}

public class ServiceAccountOptionsItem
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int? TokenExpiryInMinutes { get; set; }
    public List<string> Scopes { get; set; } = new();
}
