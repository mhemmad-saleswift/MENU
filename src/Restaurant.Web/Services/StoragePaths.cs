namespace Restaurant.Web.Services;

/// <summary>Resolved persistent storage locations (survive App Service redeploys).</summary>
public record StoragePaths(string DataPath, string UploadsPath);
