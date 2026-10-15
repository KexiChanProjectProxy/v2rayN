namespace ServiceLib.ViewModels;

public partial class FullConfigTemplateViewModel : MyReactiveObject, ICloseable
{
    public event EventHandler? RequestClose;

    #region Reactive

    [Reactive]
    public partial bool EnableFullConfigTemplate4Singbox { get; set; }

    [Reactive]
    public partial string FullConfigTemplate4Singbox { get; set; } = string.Empty;

    [Reactive]
    public partial string FullTunConfigTemplate4Singbox { get; set; } = string.Empty;

    [Reactive]
    public partial bool AddProxyOnly4Singbox { get; set; }

    [Reactive]
    public partial string ProxyDetour4Singbox { get; set; } = string.Empty;

    public ReactiveCommand<RxVoid, RxVoid> SaveCmd { get; }

    #endregion Reactive

    public FullConfigTemplateViewModel()
    {
        _config = AppManager.Instance.Config;
        SaveCmd = ReactiveCommand.CreateFromTask(async () =>
        {
            await SaveSettingAsync();
        });

        _ = Init();
    }

    private async Task Init()
    {
        var item2 = await AppManager.Instance.GetFullConfigTemplateItem(ECoreType.sing_box);
        EnableFullConfigTemplate4Singbox = item2?.Enabled ?? false;
        FullConfigTemplate4Singbox = item2?.Config ?? string.Empty;
        FullTunConfigTemplate4Singbox = item2?.TunConfig ?? string.Empty;
        AddProxyOnly4Singbox = item2?.AddProxyOnly ?? false;
        ProxyDetour4Singbox = item2?.ProxyDetour ?? string.Empty;
    }

    private async Task SaveSettingAsync()
    {
        if (!await SaveSingboxConfigAsync())
        {
            return;
        }

        NoticeManager.Instance.Enqueue(ResUI.OperationSuccess);
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private async Task<bool> SaveSingboxConfigAsync()
    {
        var item = await AppManager.Instance.GetFullConfigTemplateItem(ECoreType.sing_box);
        if (item == null)
        {
            return false;
        }
        item.Enabled = EnableFullConfigTemplate4Singbox;
        item.Config = FullConfigTemplate4Singbox;
        item.TunConfig = FullTunConfigTemplate4Singbox;

        item.AddProxyOnly = AddProxyOnly4Singbox;
        item.ProxyDetour = ProxyDetour4Singbox;

        await ConfigHandler.SaveFullConfigTemplate(_config, item);
        return true;
    }
}
