using Godot;
using ChroniclesOfTheEmpires.Core.UI;
using ChroniclesOfTheEmpires.Gameplay;

#nullable enable

namespace ChroniclesOfTheEmpires.UI.Modals;

/// <summary>
/// Modal dialog displaying comprehensive military intelligence, combat attributes, and orders for active units.
/// Integrated with IconManager for pixel-perfect HUD icon telemetry.
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

    private Label? _attackLabel;
    private Label? _defenseLabel;
    private Label? _moraleLabel;
    private TextureRect? _statusIcon;

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

        // Attach HP icon to HP header
        if (_hpLabel != null)
        {
            var hpParent = _hpLabel.GetParent();
            if (hpParent != null)
            {
                int idx = _hpLabel.GetIndex();
                hpParent.RemoveChild(_hpLabel);

                var hpRow = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
                hpRow.AddThemeConstantOverride("separation", 3);
                var hpIcon = IconManager.CreateIconRect("hp", 14);
                hpIcon.SizeFlagsVertical = SizeFlags.ShrinkCenter;
                hpRow.AddChild(hpIcon);
                hpRow.AddChild(_hpLabel);

                hpParent.AddChild(hpRow);
                hpParent.MoveChild(hpRow, idx);
            }
        }

        // Construct Attack / Defense composite row
        var vbox = GetNodeOrNull<VBoxContainer>("Margin/VBox");
        if (vbox != null && _statsLabel != null)
        {
            int idx = _statsLabel.GetIndex();
            _statsLabel.Visible = false;

            var statsRow = new HBoxContainer
            {
                Name = "UnitCombatStatsRow",
                MouseFilter = MouseFilterEnum.Ignore
            };
            statsRow.AddThemeConstantOverride("separation", 10);

            // Attack entry
            var atkRow = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
            atkRow.AddThemeConstantOverride("separation", 3);
            var atkIcon = IconManager.CreateIconRect("attack", 14);
            atkIcon.SizeFlagsVertical = SizeFlags.ShrinkCenter;
            _attackLabel = new Label { Text = "Tấn công: 0", MouseFilter = MouseFilterEnum.Ignore };
            _attackLabel.AddThemeFontSizeOverride("font_size", 9);
            _attackLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.85f, 0.85f));
            atkRow.AddChild(atkIcon);
            atkRow.AddChild(_attackLabel);
            statsRow.AddChild(atkRow);

            // Defense entry
            var defRow = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
            defRow.AddThemeConstantOverride("separation", 3);
            var defIcon = IconManager.CreateIconRect("defense", 14);
            defIcon.SizeFlagsVertical = SizeFlags.ShrinkCenter;
            _defenseLabel = new Label { Text = "Phòng ngự: 0", MouseFilter = MouseFilterEnum.Ignore };
            _defenseLabel.AddThemeFontSizeOverride("font_size", 9);
            _defenseLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.85f, 0.9f));
            defRow.AddChild(defIcon);
            defRow.AddChild(_defenseLabel);
            statsRow.AddChild(defRow);

            vbox.AddChild(statsRow);
            vbox.MoveChild(statsRow, idx + 1);
        }

        // Attach Movement icon to Moves label
        if (_movesLabel != null && vbox != null)
        {
            var movesParent = _movesLabel.GetParent();
            if (movesParent != null)
            {
                int idx = _movesLabel.GetIndex();
                movesParent.RemoveChild(_movesLabel);

                var movesRow = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
                movesRow.AddThemeConstantOverride("separation", 3);
                var movesIcon = IconManager.CreateIconRect("movement", 14);
                movesIcon.SizeFlagsVertical = SizeFlags.ShrinkCenter;
                movesRow.AddChild(movesIcon);
                movesRow.AddChild(_movesLabel);

                movesParent.AddChild(movesRow);
                movesParent.MoveChild(movesRow, idx);
            }
        }

        // Add Morale row
        if (vbox != null)
        {
            var moraleRow = new HBoxContainer
            {
                Name = "UnitMoraleRow",
                MouseFilter = MouseFilterEnum.Ignore
            };
            moraleRow.AddThemeConstantOverride("separation", 3);
            var moraleIcon = IconManager.CreateIconRect("morale", 14);
            moraleIcon.SizeFlagsVertical = SizeFlags.ShrinkCenter;
            _moraleLabel = new Label { Text = "Nhuệ khí: 100/100", MouseFilter = MouseFilterEnum.Ignore };
            _moraleLabel.AddThemeFontSizeOverride("font_size", 8);
            _moraleLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.82f, 0.35f));
            moraleRow.AddChild(moraleIcon);
            moraleRow.AddChild(_moraleLabel);

            int statusIdx = _statusLabel?.GetIndex() ?? vbox.GetChildCount();
            vbox.AddChild(moraleRow);
            vbox.MoveChild(moraleRow, statusIdx);
        }

        // Attach flag surrender icon to status label
        if (_statusLabel != null)
        {
            var statusParent = _statusLabel.GetParent();
            if (statusParent != null)
            {
                int idx = _statusLabel.GetIndex();
                statusParent.RemoveChild(_statusLabel);

                var statusRow = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
                statusRow.AddThemeConstantOverride("separation", 3);
                _statusIcon = IconManager.CreateIconRect("flag_surrender", 14);
                _statusIcon.SizeFlagsVertical = SizeFlags.ShrinkCenter;
                _statusIcon.Visible = false;
                statusRow.AddChild(_statusIcon);
                statusRow.AddChild(_statusLabel);

                statusParent.AddChild(statusRow);
                statusParent.MoveChild(statusRow, idx);
            }
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
            if (unit.IsSurrendered)
            {
                _unitFaction.Text = "ĐƠN VỊ ĐÃ BUÔNG VŨ KHÍ (TRUNG LẬP)";
                _unitFaction.Modulate = new Color(0.82f, 0.82f, 0.82f);
            }
            else
            {
                bool isPlayer = unit.FactionId == 0;
                _unitFaction.Text = isPlayer ? "QUÂN ĐỘI HOÀNG GIA" : "QUÂN ĐOÀN ĐỐI ĐỊCH";
                _unitFaction.Modulate = isPlayer ? new Color(0.96f, 0.82f, 0.35f) : new Color(0.92f, 0.35f, 0.35f);
            }
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

        if (_attackLabel != null)
        {
            _attackLabel.Text = $"Tấn công: {unit.Attack}";
        }

        if (_defenseLabel != null)
        {
            _defenseLabel.Text = $"Phòng ngự: {unit.Defense}";
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
            _movesLabel.Text = $"Bước đi: {ActiveUnit.MovementRangeRemaining} / {ActiveUnit.MovementRangeMax}";
            _movesLabel.Modulate = ActiveUnit.MovementRangeRemaining > 0
                ? new Color(0.45f, 0.85f, 0.55f)
                : new Color(0.85f, 0.45f, 0.45f);
        }

        if (_moraleLabel != null)
        {
            _moraleLabel.Text = $"Nhuệ khí: {ActiveUnit.MoraleCurrent} / {ActiveUnit.MoraleMax}";
            _moraleLabel.Modulate = ActiveUnit.MoraleCurrent < 20
                ? new Color(0.95f, 0.45f, 0.45f)
                : new Color(0.95f, 0.82f, 0.35f);
        }

        if (_statusLabel != null)
        {
            if (_statusIcon != null)
            {
                _statusIcon.Visible = ActiveUnit.IsSurrendered;
            }

            if (ActiveUnit.IsSurrendered)
            {
                _statusLabel.Text = $"VỠ TRẬN ĐẦU HÀNG (Chờ cứu: {ActiveUnit.SurrenderTurnsRemaining} lượt)";
                _statusLabel.Modulate = new Color(0.95f, 0.45f, 0.45f);
            }
            else if (ActiveUnit.MoraleCurrent < 20)
            {
                _statusLabel.Text = "Sĩ khí lung lay, nguy cơ vỡ trận";
                _statusLabel.Modulate = new Color(0.95f, 0.75f, 0.3f);
            }
            else
            {
                _statusLabel.Text = ActiveUnit.MovementRangeRemaining > 0
                    ? "Sẵn sàng tác chiến"
                    : "Đã hoàn tất lượt đi";
                _statusLabel.Modulate = new Color(0.85f, 0.85f, 0.85f);
            }
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

