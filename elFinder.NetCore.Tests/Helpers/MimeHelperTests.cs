using elFinder.NetCore.Helpers;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace elFinder.NetCore.Tests.Helpers;

public class MimeHelperTests : IDisposable
{
    [Fact]
    public void Configure_KnownExtension_ReplacesBuiltInType()
    {
        MimeHelper.Configure(ConfigurationWith(new Dictionary<string, string?>
        {
            ["elFinder:MimeTypeMap:.jpg"] = "image/custom-jpeg"
        }));

        Assert.Equal("image/custom-jpeg", MimeHelper.GetMimeType(".jpg").Full);
        Assert.Equal("image/png", MimeHelper.GetMimeType("png").Full);
    }

    [Fact]
    public void Configure_UnknownExtension_AddsType()
    {
        MimeHelper.Configure(ConfigurationWith(new Dictionary<string, string?>
        {
            ["elFinder:MimeTypeMap:.foo"] = "application/x-foo"
        }));

        Assert.Equal("application/x-foo", MimeHelper.GetMimeType(".foo").Full);
        Assert.Equal("application/octet-stream", MimeHelper.GetMimeType(".missing-extension").Full);
    }

    [Fact]
    public void Configure_ExtensionWithoutDot_MatchesDottedLookup()
    {
        MimeHelper.Configure(ConfigurationWith(new Dictionary<string, string?>
        {
            ["elFinder:MimeTypeMap:PDF"] = "application/x-custom-pdf"
        }));

        Assert.Equal("application/x-custom-pdf", MimeHelper.GetMimeType(".pdf").Full);
    }

    [Fact]
    public void Configure_CalledAgain_ReplacesEarlierOverrides()
    {
        MimeHelper.Configure(ConfigurationWith(new Dictionary<string, string?>
        {
            ["elFinder:MimeTypeMap:.jpg"] = "image/custom-jpeg"
        }));

        MimeHelper.Configure(ConfigurationWith(new Dictionary<string, string?>
        {
            ["elFinder:MimeTypeMap:.png"] = "image/custom-png"
        }));

        Assert.Equal("image/jpeg", MimeHelper.GetMimeType(".jpg").Full);
        Assert.Equal("image/custom-png", MimeHelper.GetMimeType(".png").Full);
    }

    [Fact]
    public void Configure_MissingSection_KeepsBuiltInTypes()
    {
        MimeHelper.Configure(new ConfigurationBuilder().Build());

        Assert.Equal("image/jpeg", MimeHelper.GetMimeType(".jpg").Full);
    }

    [Fact]
    public void Configure_InvalidMimeType_ThrowsAndKeepsBuiltInType()
    {
        var configuration = ConfigurationWith(new Dictionary<string, string?>
        {
            ["elFinder:MimeTypeMap:.jpg"] = "not-a-mime-type"
        });

        Assert.Throws<ArgumentException>(() => MimeHelper.Configure(configuration));
        Assert.Equal("image/jpeg", MimeHelper.GetMimeType(".jpg").Full);
    }

    private static IConfiguration ConfigurationWith(Dictionary<string, string?> values) => new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    public void Dispose() => MimeTypeMap.ResetOverrides();
}