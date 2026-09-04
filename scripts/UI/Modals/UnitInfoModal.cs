using Godot;
using ChroniclesOfTheEmpires.Gameplay;

#nullable enable

namespace ChroniclesOfTheEmpires.UI.Modals;

/// <summary>
/// Modal dialog displaying comprehensive military intelligence, combat attributes, and orders for active units.
/// </summary>
public partial class UnitInfoModal : PanelContainer
{
    private Label? _unitTitle;
    private Label? _unitFaction;
    private Label? _unitDesc;
    private ProgressBar? _hpBar;
    private Label? _hpLabel;
    private Label? _statsLabel;
    private Label? _movesLabel;
    private Label? _statusLabel;
    private Button? _btnClose;

    public UnitController? ActiveUnit { get; private set; }

    public override void _Ready()
    {
        _unitTitle = GetNodeOrNull<Label>("%UnitTitle");
        _unitFaction = GetNodeOrNull<Label>("%UnitFaction");
        _unitDesc = GetNodeOrNull<Label>("%UnitDesc");
        _hpBar = GetNodeOrNull<ProgressBar>("%HpBar");
        _hpLabel = GetNodeOrNull<Label>("%HpLabel");
        _statsLabel = GetNodeOrNull<Label>("%UnitCombatStats");
        _movesLabel = GetNodeOrNull<Label>("%UnitMoves");
        _statusLabel = GetNodeOrNull<Label>("%UnitStatus");
        _btnClose = GetNodeOrNull<Button>("%BtnCloseUnit");

        if (_btnClose != null)
        {
            _btnClose.Pressed += CloseModal;
        }

        Visible = false;
    }

    public void DisplayUnit(UnitController unit)
    {
        ActiveUnit = unit;

        if (_unitTitle != null)
        {
            _unitTitle.Text = unit.UnitName.ToUpperInvariant();
        }

        if (_unitFaction != null)
        {
            bool isPlayer = unit.FactionId == 0;
            _unitFaction.Text = isPlayer ? "★ QUÂN ĐỘI HOÀNG GIA" : "◆ QUÂN ĐOÀN ĐỐI ĐỊCH";
            _unitFaction.Modulate = isPlayer ? new Color(0.96f, 0.82f, 0.35f) : new Color(0.92f, 0.35f, 0.35f);
        }

        if (_unitDesc != null)
        {
            _unitDesc.Text = unit.Description;
            _unitDesc.Visible = !string.IsNullOrEmpty(unit.Description);
        }

        if (_hpBar != null)
        {
            _hpBar.MaxValue = unit.HpMax;
            _hpBar.Value = unit.HpCurrent;
        }

        if (_hpLabel != null)
        {
            _hpLabel.Text = $"Máu: {unit.HpCurrent} / {unit.HpMax}";
        }

        if (_statsLabel != null)
        {
            _statsLabel.Text = $"⚔ Tấn công: {unit.Attack}   |   🛡 Phòng ngự: {unit.Defense}";
        }

        RefreshMovementInfo();

        Visible = true;
        AnimateOpen();
    }

    public void RefreshMovementInfo()
    {
        if (ActiveUnit == null) return;

        if (_movesLabel != null)
        {
            _movesLabel.Text = $"⚡ Bước đi còn lại: {ActiveUnit.MovementRangeRemaining} / {ActiveUnit.MovementRangeMax}";
            _movesLabel.Modulate = ActiveUnit.MovementRangeRemaining > 0
                ? new Color(0.45f, 0.85f, 0.55f)
                : new Color(0.85f, 0.45f, 0.45f);
        }

        if (_statusLabel != null)
        {
            _statusLabel.Text = ActiveUnit.MovementRangeRemaining > 0
                ? "Trạng thái: Sẵn sàng nhận lệnh tác chiến"
                : "Trạng thái: Đã hoàn tất lượt đi";
        }
    }

    public void CloseModal()
    {
        ActiveUnit = null;
        Visible = false;
    }

    private void AnimateOpen()
    {
        Modulate = new Color(1f, 1f, 1f, 0f);
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 1.0f, 0.15);
    }
}

