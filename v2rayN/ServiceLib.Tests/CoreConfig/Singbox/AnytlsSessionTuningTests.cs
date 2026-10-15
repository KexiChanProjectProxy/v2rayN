namespace ServiceLib.Tests.CoreConfig.Singbox;

public class AnytlsSessionTuningTests
{
    private static (SingboxConfig Config, string Json) Generate(Config config, ProfileItem node, params ProfileItem[] children)
    {
        CoreConfigTestFactory.BindAppManagerConfig(config);
        var context = CoreConfigTestFactory.CreateContext(config, node, ECoreType.sing_box);
        foreach (var child in children)
        {
            context.AllProxiesMap[child.IndexId] = child;
        }
        var result = new CoreConfigSingboxService(context).GenerateClientConfigContent();
        if (result.Success != true)
        {
            throw new InvalidOperationException(result.Msg);
        }
        var json = result.Data!.ToString()!;
        return (JsonUtils.Deserialize<SingboxConfig>(json)!, json);
    }

    [Test]
    public async Task DefaultSettings_ShouldEmitEverySessionField()
    {
        var (cfg, _) = Generate(CoreConfigTestFactory.CreateConfig(), CoreConfigTestFactory.CreateAnytlsNode());
        var proxy = cfg.outbounds.First(o => o.tag == Global.ProxyTag);

        await proxy.type.Should().BeEqualTo("anytls");
        await proxy.password.Should().BeEqualTo("anytls-pass");
        await proxy.tls!.enabled.Should().BeEqualTo(true);
        await proxy.idle_session_check_interval.Should().BeEqualTo("30s");
        await proxy.idle_session_timeout.Should().BeEqualTo("30s");
        await proxy.min_idle_session.Should().BeEqualTo(5);
        await proxy.ensure_idle_session.Should().BeEqualTo(10);
        await proxy.ensure_idle_session_create_rate.Should().BeEqualTo(5);
        await proxy.heartbeat.Should().BeEqualTo("30s");
        await proxy.min_idle_session_for_age.Should().BeEqualTo(2);
        await proxy.max_connection_lifetime.Should().BeEqualTo("300s");
        await proxy.connection_lifetime_jitter.Should().BeEqualTo("10s");
        await proxy.close_after_idle.Should().BeEqualTo("5m");
        await proxy.close_after_lifetime.Should().BeEqualTo("30m");
    }

    [Test]
    public async Task MasterSwitchOff_ShouldEmitOnlyPasswordAndTls()
    {
        var config = CoreConfigTestFactory.CreateConfig();
        config.Anytls4SboxItem.Enabled = false;
        var (cfg, json) = Generate(config, CoreConfigTestFactory.CreateAnytlsNode());
        var proxy = cfg.outbounds.First(o => o.tag == Global.ProxyTag);

        await proxy.password.Should().BeEqualTo("anytls-pass");
        await proxy.tls.Should().NotBeNull();
        await proxy.min_idle_session.Should().BeNull();
        await proxy.close_after_lifetime.Should().BeNull();
        await json.Contains("idle_session_check_interval").Should().BeFalse();
        await json.Contains("ensure_idle_session").Should().BeFalse();
    }

    [Test]
    public async Task ClearedFields_ShouldBeOmitted()
    {
        var config = CoreConfigTestFactory.CreateConfig();
        config.Anytls4SboxItem.CloseAfterIdle = null;
        config.Anytls4SboxItem.Heartbeat = "";
        config.Anytls4SboxItem.MinIdleSession = 0;
        var (cfg, json) = Generate(config, CoreConfigTestFactory.CreateAnytlsNode());
        var proxy = cfg.outbounds.First(o => o.tag == Global.ProxyTag);

        await proxy.close_after_idle.Should().BeNull();
        await proxy.heartbeat.Should().BeNull();
        await proxy.min_idle_session.Should().BeNull();
        await json.Contains("close_after_idle").Should().BeFalse();
        await proxy.ensure_idle_session.Should().BeEqualTo(10);
        await proxy.close_after_lifetime.Should().BeEqualTo("30m");
    }

    [Test]
    public async Task NonAnytlsOutbound_ShouldNotGetSessionFields()
    {
        var (_, json) = Generate(CoreConfigTestFactory.CreateConfig(), CoreConfigTestFactory.CreateSocksNode(ECoreType.sing_box));

        await json.Contains("idle_session").Should().BeFalse();
        await json.Contains("latency_mode").Should().BeFalse();
    }

    private static ProfileItem CreateGroup(EMultipleLoad load, out ProfileItem[] children)
    {
        var a = CoreConfigTestFactory.CreateAnytlsNode("a1", "anytls-1");
        var b = CoreConfigTestFactory.CreateAnytlsNode("a2", "anytls-2");
        children = [a, b];
        var group = CoreConfigTestFactory.CreatePolicyGroupNode(ECoreType.sing_box, "g1", "group", [a.IndexId, b.IndexId]);
        group.SetProtocolExtra(group.GetProtocolExtra() with { MultipleLoad = load });
        return group;
    }

    [Test]
    public async Task PolicyGroupLeastPing_ShouldUseSessionLatencyMode()
    {
        var group = CreateGroup(EMultipleLoad.LeastPing, out var children);
        var (cfg, _) = Generate(CoreConfigTestFactory.CreateConfig(), group, children);
        var urltest = cfg.outbounds.First(o => o.type == "urltest");

        await urltest.latency_mode.Should().BeEqualTo("session");
        await cfg.outbounds.Count(o => o.type == "anytls" && o.ensure_idle_session == 10).Should().BeEqualTo(2);
    }

    [Test]
    public async Task PolicyGroup_SessionLatencyModeDisabled_ShouldOmitLatencyMode()
    {
        var config = CoreConfigTestFactory.CreateConfig();
        config.CoreBasicItem.EnableSessionLatencyMode4Sbox = false;
        var group = CreateGroup(EMultipleLoad.LeastPing, out var children);
        var (cfg, _) = Generate(config, group, children);

        await cfg.outbounds.First(o => o.type == "urltest").latency_mode.Should().BeNull();
    }

    [Test]
    public async Task PolicyGroupFallback_ShouldKeepUrlTestLatency()
    {
        // tolerance does not apply in session mode, so Fallback keeps URL tests.
        var group = CreateGroup(EMultipleLoad.Fallback, out var children);
        var (cfg, _) = Generate(CoreConfigTestFactory.CreateConfig(), group, children);
        var urltest = cfg.outbounds.First(o => o.type == "urltest");

        await urltest.latency_mode.Should().BeNull();
        await urltest.tolerance.Should().BeEqualTo(5000);
    }
}
