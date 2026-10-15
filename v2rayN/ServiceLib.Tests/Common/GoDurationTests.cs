namespace ServiceLib.Tests.Common;

public class GoDurationTests
{
    [Test]
    [Arguments("30s")]
    [Arguments("1m30s")]
    [Arguments("1.5h")]
    [Arguments("500ms")]
    [Arguments("2d")]
    [Arguments(" 5m ")]
    [Arguments("")]
    [Arguments(null)]
    public async Task ValidDurations_ShouldPass(string? input)
    {
        await Utils.IsValidGoDuration(input).Should().BeTrue();
    }

    [Test]
    [Arguments("30")]
    [Arguments("-5s")]
    [Arguments("5 s")]
    [Arguments("5sec")]
    [Arguments("s")]
    public async Task InvalidDurations_ShouldFail(string input)
    {
        await Utils.IsValidGoDuration(input).Should().BeFalse();
    }
}
