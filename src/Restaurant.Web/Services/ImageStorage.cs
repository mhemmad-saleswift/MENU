namespace Restaurant.Web.Services;

/// <summary>Persists uploaded product images and returns a servable URL.</summary>
public interface IImageStorage
{
    Task<string> SaveAsync(Stream stream, string originalFileName, CancellationToken ct = default);
}

/// <summary>
/// Stores images on the local disk under <c>{ContentRoot}/uploads</c>, served at <c>/uploads</c>
/// (see static-file mapping in Program.cs). Swap this implementation for blob storage in the cloud.
/// </summary>
public class LocalImageStorage(StoragePaths paths) : IImageStorage
{
    static readonly string[] Allowed = [".jpg", ".jpeg", ".png", ".webp", ".gif", ".avif"];

    public async Task<string> SaveAsync(Stream stream, string originalFileName, CancellationToken ct = default)
    {
        Directory.CreateDirectory(paths.UploadsPath);

        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (!Allowed.Contains(ext)) ext = ".jpg";

        var name = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(paths.UploadsPath, name);
        await using (var fs = File.Create(path))
            await stream.CopyToAsync(fs, ct);

        return $"/uploads/{name}";
    }
}
