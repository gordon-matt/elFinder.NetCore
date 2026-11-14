using elFinder.NetCore.Models;

namespace elFinder.NetCore.Helpers;

public static class MimeHelper
{
    // Replaces the built-in type when the extension is already known, and adds it when it is not.
    public static void Configure(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var overrides = configuration
            .GetSection("elFinder:MimeTypeMap")
            .GetChildren()
            .Select(child => new KeyValuePair<string, string>(child.Key, child.Value ?? string.Empty));

        MimeTypeMap.ApplyOverrides(overrides);
    }

    public static MimeType GetMimeType(string extension)
    {
        // TODO:
        //  1. Compare MimeTypeMap with: https://codingislove.com/list-mime-types-2016/
        //  2. Compare MimeTypeMap with: https://slick.pl/kb/htaccess/complete-list-mime-types/
        //  3. Consider modifying MimeTypeMap to return our custom MimeType struct, so that we don't need this MimeHelper class anymore

        string mimeType = MimeTypeMap.GetMimeType(extension);
        string[] split = mimeType.Split('/');
        return new MimeType { Type = split[0], Subtype = split[1] };
    }
}