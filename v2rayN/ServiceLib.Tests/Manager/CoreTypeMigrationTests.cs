using ServiceLib.Tests.CoreConfig;

namespace ServiceLib.Tests.Manager;

public class CoreTypeMigrationTests
{
    [Test]
    public async Task RemovedCoreTypes_ShouldBeMigratedToSingbox()
    {
        var config = CoreConfigTestFactory.CreateConfig();
        CoreConfigTestFactory.BindAppManagerConfig(config);
        SQLiteHelper.Instance.CreateTable<SubItem>();
        SQLiteHelper.Instance.CreateTable<ProfileItem>();
        SQLiteHelper.Instance.CreateTable<DNSItem>();

        var xrayNodeId = $"xray-{Guid.NewGuid():N}";
        var plainNodeId = $"plain-{Guid.NewGuid():N}";
        var subId = $"sub-{Guid.NewGuid():N}";
        var dnsId = $"dns-{Guid.NewGuid():N}";
        // 2 = Xray, 13 = mihomo in the old enum.
        await SQLiteHelper.Instance.ReplaceAsync(new ProfileItem { IndexId = xrayNodeId, ConfigType = EConfigType.VMess, CoreType = (ECoreType)2, Subid = subId });
        await SQLiteHelper.Instance.ReplaceAsync(new ProfileItem { IndexId = plainNodeId, ConfigType = EConfigType.VMess, CoreType = null, Subid = subId });
        await SQLiteHelper.Instance.ReplaceAsync(new SubItem { Id = subId, Remarks = "sub", CustomCoreType = (ECoreType)13 });
        await SQLiteHelper.Instance.ReplaceAsync(new DNSItem { Id = dnsId, Remarks = "V2ray", CoreType = (ECoreType)2 });

        await AppManager.MigrateCoreTypeToSingboxOnly();

        await (await AppManager.Instance.GetProfileItem(xrayNodeId))!.CoreType.Should().BeEqualTo(ECoreType.sing_box);
        await (await AppManager.Instance.GetProfileItem(plainNodeId))!.CoreType.Should().BeNull();
        await (await AppManager.Instance.GetSubItem(subId))!.CustomCoreType.Should().BeEqualTo(ECoreType.sing_box);
        var dnsItems = await AppManager.Instance.DNSItems() ?? [];
        await dnsItems.Any(d => d.Id == dnsId).Should().BeFalse();
    }
}
