using Godot;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Gameplay;

#nullable enable

namespace ChroniclesOfTheEmpires.UI.Modals;

/// <summary>
/// Modal dialog displaying granular terrain intelligence, resource yields, potential deposits,
/// and upgrade construction costs for inspected HexCell objects.
/// Designed for 640x360 viewport layout constraints.
/// </summary>
public partial class TileInfoModal : PanelContainer
{
    private Label? _titleLabel;
    private Label? _descLabel;
    private Label? _yieldsLabel;
    private Label? _traversalLabel;
    private Label? _coordLabel;
    private RichTextLabel? _depositLabel;
    private Button? _btnClose;

    public HexCell? ActiveCell { get; private set; }

    public override void _Ready()
    {
        _titleLabel = GetNodeOrNull<Label>("%TileTitle");
        _descLabel = GetNodeOrNull<Label>("%TileDesc");
        _yieldsLabel = GetNodeOrNull<Label>("%TileYields");
        _traversalLabel = GetNodeOrNull<Label>("%TileTraversal");
        _coordLabel = GetNodeOrNull<Label>("%TileCoords");
        _btnClose = GetNodeOrNull<Button>("%BtnCloseTile");

        if (_btnClose != null)
        {
            _btnClose.Pressed += CloseModal;
        }

        // Dynamically initialize rich deposit label if not defined in tscn
        var vbox = GetNodeOrNull<VBoxContainer>("Margin/VBox");
        if (vbox != null)
        {
            _depositLabel = new RichTextLabel
            {
                Name = "TileDepositInfo",
                BbcodeEnabled = true,
                FitContent = true,
                AutowrapMode = TextServer.AutowrapMode.Word,
                CustomMinimumSize = new Vector2(0, 20)
            };
            _depositLabel.AddThemeFontSizeOverride("normal_font_size", 8);
            vbox.AddChild(_depositLabel);
        }

        Visible = false;
    }

    public void DisplayCell(HexCell cell)
    {
        ActiveCell = cell;

        if (_titleLabel != null)
        {
            _titleLabel.Text = $"{cell.TerrainData.Biome.ToString().ToUpperInvariant()}: {cell.Name.ToUpperInvariant()}";
        }

        if (_descLabel != null)
        {
            _descLabel.Text = cell.Description;
        }

        if (_yieldsLabel != null)
        {
            _yieldsLabel.Text = $"🌾 {cell.FoodYield}  |  🔨 {cell.ProdYield}  |  🪙 {cell.GoldYield}  |   {cell.SciYield}  |  📿 {cell.FaithYield}";
        }

        if (_traversalLabel != null)
        {
            string barrierStatus = cell.IsSolid ? "Vật cản (Bất khả xâm phạm)" : "Thông thoáng";
            string costText = cell.IsSolid ? "∞" : $"{cell.MoveCost} Điểm";
            _traversalLabel.Text = $"Tiêu hao: {costText}  |  Trạng thái: {barrierStatus}";
        }

        if (_depositLabel != null)
        {
            if (cell.Deposit != null)
            {
                var dep = cell.Deposit;
                string status = dep.IsExploited ? "[color=#68b87d]Đã khai thác[/color]" : "[color=#e08834]Chưa khai thác[/color]";
                var bldCfg = GameConfigManager.GetBuildingConfig(dep.RequiredImprovement);
                string costText = bldCfg != null
                    ? $"[color=#ecd889]{bldCfg.Name}[/color] (Chi phí: {bldCfg.Cost})"
                    : $"Công trình yêu cầu: {dep.RequiredImprovement}";

                _depositLabel.Text = $"💎 [b]{dep.Name}[/b] [{status}]\n   Cộng thêm: {dep.BonusYield}\n   Nâng cấp: {costText}";
            }
            else
            {
                _depositLabel.Text = "[color=#777777]Địa hình tự nhiên, không có mỏ khoáng sản/nông sản.[/color]";
            }
        }

        if (_coordLabel != null)
        {
            string owner = cell.OwnerFactionId >= 0 ? $"Phe {cell.OwnerFactionId}" : "Vùng hoang dã";
            _coordLabel.Text = $"Tọa độ: [{cell.Coords.X}, {cell.Coords.Y}]  |  Chủ quyền: {owner}";
        }

        Visible = true;
        AnimateOpen();
    }

    public void CloseModal()
    {
        ActiveCell = null;
        Visible = false;
    }

    private void AnimateOpen()
    {
        Modulate = new Color(1f, 1f, 1f, 0f);
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 1.0f, 0.12);
    }
}
