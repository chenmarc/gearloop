namespace Gearloop.App.Services;

/// <summary>
/// Copies item photos into the app's data folder. The database stores paths relative to that
/// folder, because iOS can move the app's folder when the app is updated.
/// </summary>
public static class PhotoStore
{
    private const string Folder = "photos";

    /// <summary>Asks the user for photos from the camera or library and stores copies.</summary>
    /// <returns>The stored relative paths; empty if the user cancelled.</returns>
    public static async Task<IReadOnlyList<string>> AddAsync(bool fromCamera)
    {
        var photos = new List<FileResult>();
        if (fromCamera)
        {
            if (await MediaPicker.Default.CapturePhotoAsync() is { } photo) photos.Add(photo);
        }
        else
        {
            photos.AddRange((await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { SelectionLimit = 10 }))
                .Where(p => p is not null));
        }

        Directory.CreateDirectory(Path.Combine(FileSystem.AppDataDirectory, Folder));
        var stored = new List<string>();
        foreach (var photo in photos)
        {
            var extension = Path.GetExtension(photo.FileName);
            if (string.IsNullOrEmpty(extension)) extension = ".jpg";
            var relativePath = Path.Combine(Folder, $"{Guid.NewGuid():N}{extension}");

            await using var source = await photo.OpenReadAsync();
            await using var target = File.Create(FullPath(relativePath));
            await source.CopyToAsync(target);
            stored.Add(relativePath);
        }
        return stored;
    }

    public static bool CanUseCamera => MediaPicker.Default.IsCaptureSupported;

    public static string FullPath(string relativePath) =>
        Path.Combine(FileSystem.AppDataDirectory, relativePath);

    public static void Delete(IEnumerable<string> relativePaths)
    {
        foreach (var path in relativePaths)
        {
            try { File.Delete(FullPath(path)); }
            catch (IOException) { /* a leftover photo file is harmless */ }
        }
    }
}
