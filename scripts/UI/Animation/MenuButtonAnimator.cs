using Godot;
using System;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

/// <summary>
/// Attaches micro-interaction tweens (dynamic pivot centering, hover swell, press depression)
/// and auditory feedback to Godot buttons.
/// </summary>
public static class MenuButtonAnimator
{
    public static void Attach(
        Button btn,
        AudioStreamPlayer? sfxHover = null,
        AudioStreamPlayer? sfxClick = null,
        float hoverScale = 1.05f,
        float pressScale = 0.96f)
    {
        // Maintain centered pivot during size alterations to prevent off-center scale distortion
        btn.Resized += () => btn.PivotOffset = btn.Size / 2.0f;
        btn.PivotOffset = btn.CustomMinimumSize / 2.0f;

        btn.MouseEntered += () =>
        {
            if (btn.Disabled) return;
            MenuAudioHelper.PlaySound(sfxHover);
            var tween = btn.CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
            tween.TweenProperty(btn, "scale", new Vector2(hoverScale, hoverScale), 0.10f);
        };

        btn.MouseExited += () =>
        {
            if (btn.Disabled) return;
            var tween = btn.CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
            tween.TweenProperty(btn, "scale", Vector2.One, 0.12f);
        };

        btn.ButtonDown += () =>
        {
            if (btn.Disabled) return;
            var tween = btn.CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
            tween.TweenProperty(btn, "scale", new Vector2(pressScale, pressScale), 0.05f);
        };

        btn.ButtonUp += () =>
        {
            if (btn.Disabled) return;
            Vector2 targetScale = btn.IsHovered() ? new Vector2(hoverScale, hoverScale) : Vector2.One;
            var tween = btn.CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
            tween.TweenProperty(btn, "scale", targetScale, 0.12f);
        };

        if (sfxClick != null)
        {
            btn.Pressed += () =>
            {
                if (!btn.Disabled)
                {
                    MenuAudioHelper.PlaySound(sfxClick);
                }
            };
        }
    }
}

