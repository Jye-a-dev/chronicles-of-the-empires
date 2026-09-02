using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

/// <summary>
/// Minimal placeholder controller for the World Map campaign scene.
/// </summary>
public partial class WorldMap : Control
{
    public override void _Ready()
    {
        var btnBack = GetNodeOrNull<Button>("%BtnBack") ?? GetNodeOrNull<Button>("CenterContainer/VBoxContainer/BtnBack");
        if (btnBack != null)
        {
            btnBack.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
        }
    }
}

