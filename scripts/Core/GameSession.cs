using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

/// <summary>
/// Immutable specification of the launched game session.
/// </summary>
public record GameSessionConfig(
    string Mode, // "campaign", "skirmish", "tutorial", "sandbox"
    string ModeTitle,
    string StageId,
    string StageTitle,
    string MapSize,
    string Biome,
    int RivalsCount,
    string VictoryCondition,
    string Difficulty
);

/// <summary>
/// Global static runtime holder for active game session settings.
/// Passed from CampaignModal -> MainMenu -> LoadingScreen -> WorldMap.
/// </summary>
public static class GameSession
{
    public static GameSessionConfig ActiveConfig { get; set; } = new(
        Mode: "campaign",
        ModeTitle: "Đại Chiến Dịch",
        StageId: "stage_1",
        StageTitle: "Ải 1: Khởi Nguồn Văn Lang",
        MapSize: "32x32",
        Biome: "red_river",
        RivalsCount: 2,
        VictoryCondition: "conquest",
        Difficulty: "normal"
    );
}

