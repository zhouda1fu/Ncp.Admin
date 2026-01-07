namespace Ncp.Admin.Web.Utils;

public class AppConfiguration
{
    public string Secret { get; set; } = string.Empty;
    public int TokenExpiryInMinutes { get; set; }
}

