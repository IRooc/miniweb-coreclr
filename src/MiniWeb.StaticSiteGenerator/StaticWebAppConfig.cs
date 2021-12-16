namespace MiniWeb.StaticSiteGenerator
{
    //MSDN: https://docs.microsoft.com/en-us/azure/static-web-apps/configuration
    public class StaticWebAppConfig
    {
        public NavigationRoute[]? routes { get; set; }
        public NavigationFallback? navigationFallback { get; set; }
        public Dictionary<string, NavigationOverride>? responseOverrides { get; set; }
        public Dictionary<string, string>? globalHeaders { get; set; }
        public Dictionary<string, string>? mimeTypes { get; set; }
    }
    public class NavigationFallback
    {
        public string? rewrite { get; set; }
        public string[]? exclude { get; set; }
    }

    public class NavigationOverride
    {
        public string? rewrite { get; set; }
        public string? redirect { get; set; }
        public int? statusCode { get; set; }
    }


    public class NavigationRoute
    {
        public string? route { get; set; }
        public string[]? allowedRoles { get; set; }
        public Dictionary<string, string>? headers { get; set; }
        public string[]? methods { get; set; }
        public string? rewrite { get; set; }
        public int? statusCode { get; set; }
        public string? redirect { get; set; }
    }
}
