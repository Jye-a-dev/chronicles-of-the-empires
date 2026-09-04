using Godot;
using ChroniclesOfTheEmpires.Gameplay;

#nullable enable

namespace ChroniclesOfTheEmpires.UI.Modals;

/// <summary>
/// Modal dialog displaying granular terrain intelligence, resource yields, and occupancy for inspected HexCell objects.
/// </summary>
public partial class TileInfoModal : PanelContainer
{
    private Label? _titleLabel;
    private Label? _descLabel;
    private Label? _yieldsLabel;
    private Label? _traversalLabel;
    private Label? _coordLabel;
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

        Visible = false;
    }

    public void DisplayCell(HexCell cell)
    {
        ActiveCell = cell;

        if (_titleLabel != null)
        {
            _titleLabel.Text = cell.Name.ToUpperInvariant();
        }

        if (_descLabel != null)
        {
            _descLabel.Text = cell.Description;
        }

        if (_yieldsLabel != null)
        {
            _yieldsLabel.Text = $"🌾 Lương thực: +{cell.FoodYield}  |  🔨 Sản xuất: +{cell.ProdYield}  |  🪙 Vàng: +{cell.GoldYield}";
        }

        if (_traversalLabel != null)
        {
            string barrierStatus = cell.IsSolid ? "Vật cản (Không thể vượt qua)" : "Thông thoáng";
            string costText = cell.IsSolid ? "∞" : $"{cell.MoveCost} Điểm";
            _traversalLabel.Text = $"Tiêu hao: {costText}  |  Trạng thái: {barrierStatus}";
        }

        if (_coordLabel != null)
        {
            _coordLabel.Text = $"Tọa độ Lục giác: [{cell.Coords.X}, {cell.Coords.Y}]";
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
        tween.TweenProperty(this, "modulate:a", 1.0f, 0.15);
    }
}

