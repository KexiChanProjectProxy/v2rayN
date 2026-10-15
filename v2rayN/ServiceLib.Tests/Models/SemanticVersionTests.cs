namespace ServiceLib.Tests.Models;

public class SemanticVersionTests
{
    [Test]
    public async Task FourPartVersions_ShouldCompareOnRevision()
    {
        await (new SemanticVersion("v1.14.2.7") > new SemanticVersion("1.14.2.6")).Should().BeTrue();
        await (new SemanticVersion("1.14.2.6") >= new SemanticVersion("v1.14.2.7")).Should().BeFalse();
        await (new SemanticVersion("1.14.2") < new SemanticVersion("1.14.2.1")).Should().BeTrue();
        await (new SemanticVersion("1.14.3") > new SemanticVersion("1.14.2.9")).Should().BeTrue();
        await new SemanticVersion("1.14.2.7").Equals(new SemanticVersion("v1.14.2.7")).Should().BeTrue();
    }

    [Test]
    public async Task ToStandardVersionString_ShouldKeepRevision()
    {
        await new SemanticVersion("v1.14.2.7").ToStandardVersionString("v").Should().BeEqualTo("v1.14.2.7");
        await new SemanticVersion("1.12.0").ToStandardVersionString("v").Should().BeEqualTo("v1.12.0");
        await new SemanticVersion("1.12.0-beta.1").ToStandardVersionString().Should().BeEqualTo("1.12.0-beta.1");
    }

    [Test]
    public async Task RawTag_ShouldBeKeptForDownloadUrls()
    {
        // UpdateService formats {0} with the tag and {1} with the tag minus "v".
        var version = new SemanticVersion("v1.14.2.7");
        await version.ToString().Should().BeEqualTo("v1.14.2.7");
        await version.ToString().RemovePrefix("v").Should().BeEqualTo("1.14.2.7");
    }

    [Test]
    [Arguments("sing-box version 1.14.2.7\n\nEnvironment: go1.26.6 linux/amd64", "1.14.2.7")]
    [Arguments("sing-box version 1.12.0-beta.1\n", "1.12.0-beta.1")]
    [Arguments("sing-box version 1.13.4", "1.13.4")]
    public async Task VersionRegex_ShouldCaptureFourPartVersions(string output, string expected)
    {
        var method = typeof(UpdateService).GetMethod("SemVerRegex", BindingFlags.Static | BindingFlags.NonPublic)!;
        var regex = (System.Text.RegularExpressions.Regex)method.Invoke(null, null)!;
        await regex.Match(output).Groups["version"].Value.Should().BeEqualTo(expected);
    }
}
