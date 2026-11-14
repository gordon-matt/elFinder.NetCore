namespace elFinder.NetCore.Models;

// https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types
public struct MimeType
{
    public string Type { get; set; }

    public string Subtype { get; set; }

    public readonly string Full => $"{Type}/{Subtype}";

    public static implicit operator string(MimeType m) => m.Full;

    public readonly override string ToString() => Full;
}