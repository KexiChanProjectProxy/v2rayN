namespace ServiceLib.Models.Configs;

[Serializable]
public class Config
{
    #region property

    public string IndexId { get; set; }
    public string SubIndexId { get; set; }

    #endregion property

    #region other entities

    public CoreBasicItem CoreBasicItem { get; set; }
    public TunModeItem TunModeItem { get; set; }
    public GrpcItem GrpcItem { get; set; }
    public RoutingBasicItem RoutingBasicItem { get; set; }
    public GUIItem GuiItem { get; set; }
    public MsgUIItem MsgUIItem { get; set; }
    public UIItem UiItem { get; set; }
    public ConstItem ConstItem { get; set; }
    public SpeedTestItem SpeedTestItem { get; set; }
    public Mux4SboxItem Mux4SboxItem { get; set; }
    public Anytls4SboxItem Anytls4SboxItem { get; set; }
    public HysteriaItem HysteriaItem { get; set; }
    public ClashUIItem ClashUIItem { get; set; }
    public SystemProxyItem SystemProxyItem { get; set; }
    public WebDavItem WebDavItem { get; set; }
    public CheckUpdateItem CheckUpdateItem { get; set; }
    public List<InItem> Inbound { get; set; }
    public List<KeyEventItem> GlobalHotkeys { get; set; }
    public SimpleDNSItem SimpleDNSItem { get; set; }

    #endregion other entities
}
