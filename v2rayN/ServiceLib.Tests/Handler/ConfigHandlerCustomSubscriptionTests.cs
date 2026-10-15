using ServiceLib.Tests.CoreConfig;

namespace ServiceLib.Tests.Handler;

public class ConfigHandlerCustomSubscriptionTests
{
    private const string FullSingboxConfigs = """
        [
          {"inbounds":[{"type":"socks","tag":"socks","listen":"127.0.0.1","listen_port":10808}],"outbounds":[{"type":"vless","tag":"proxy","server":"127.0.0.1","server_port":443,"uuid":"00000000-0000-0000-0000-000000000001"}]},
          {"inbounds":[{"type":"socks","tag":"socks","listen":"127.0.0.1","listen_port":10808}],"outbounds":[{"type":"anytls","tag":"proxy","server":"127.0.0.1","server_port":443,"password":"p","tls":{"enabled":true}}]}
        ]
        """;

    [Test]
    public async Task AddBatchServers_FullConfigsFromSubscription_InheritSubscriptionPreSocksPort()
    {
        var config = CoreConfigTestFactory.CreateConfig();
        CoreConfigTestFactory.BindAppManagerConfig(config);
        SQLiteHelper.Instance.CreateTable<SubItem>();
        SQLiteHelper.Instance.CreateTable<ProfileItem>();

        var subId = $"sub-{Guid.NewGuid():N}";
        await SQLiteHelper.Instance.ReplaceAsync(new SubItem
        {
            Id = subId,
            Remarks = "sub",
            Url = "http://127.0.0.1/sub",
            PreSocksPort = 10808,
        });

        var count = await ConfigHandler.AddBatchServers(config, FullSingboxConfigs, subId, true);
        var profiles = await AppManager.Instance.ProfileItems(subId) ?? [];

        await count.Should().BeEqualTo(2);
        await profiles.Should().HaveCount(2);
        foreach (var profile in profiles)
        {
            await profile.ConfigType.Should().BeEqualTo(EConfigType.Custom);
            await profile.CoreType.Should().BeEqualTo(ECoreType.sing_box);
            await profile.PreSocksPort.Should().BeEqualTo(10808);
        }
    }
}
