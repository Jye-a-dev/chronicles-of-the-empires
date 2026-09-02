using Godot;
using System;

#nullable enable

namespace ChroniclesOfTheEmpires.UI;

/// <summary>
/// Reusable procedural 16-bit audio synthesizer and sound dispatcher for UI elements.
/// </summary>
public static class MenuAudioHelper
{
    public static void PlaySound(AudioStreamPlayer? player)
    {
        if (player?.Stream != null)
        {
            player.Play();
        }
    }

    /// <summary>
    /// Initializes fallback procedural tones if inspector audio slots are empty.
    /// </summary>
    public static void EnsureProceduralStreams(AudioStreamPlayer? hoverPlayer, AudioStreamPlayer? clickPlayer)
    {
        if (hoverPlayer != null && hoverPlayer.Stream == null)
        {
            hoverPlayer.Stream = GenerateSyntheticTone(frequency: 880.0f, durationSec: 0.045f, isMetallic: false);
        }

        if (clickPlayer != null && clickPlayer.Stream == null)
        {
            clickPlayer.Stream = GenerateSyntheticTone(frequency: 380.0f, durationSec: 0.12f, isMetallic: true);
        }
    }

    /// <summary>
    /// Generates short procedural 16-bit PCM audio waveforms with exponential decay envelopes.
    /// </summary>
    public static AudioStreamWav GenerateSyntheticTone(float frequency, float durationSec, bool isMetallic)
    {
        const int sampleRate = 22050;
        int numSamples = (int)(sampleRate * durationSec);
        byte[] pcmData = new byte[numSamples * 2];

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / numSamples;
            float envelope = Mathf.Exp(-progress * (isMetallic ? 8.0f : 16.0f));

            float wave;
            if (isMetallic)
            {
                // Metallic resonance: bronze fundamental (380 Hz) + harmonic overtone (817 Hz) + transient strike
                float fundamental = Mathf.Sin(2.0f * Mathf.Pi * frequency * t);
                float harmonic = 0.35f * Mathf.Sin(2.0f * Mathf.Pi * (frequency * 2.15f) * t);
                float transient = (float)(GD.Randf() * 2.0 - 1.0) * Mathf.Exp(-progress * 50.0f) * 0.25f;
                wave = (fundamental + harmonic + transient) * envelope;
            }
            else
            {
                // UI hover chirp: quick downward pitch sweep
                float currentFreq = frequency * (1.0f - progress * 0.25f);
                wave = Mathf.Sin(2.0f * Mathf.Pi * currentFreq * t) * envelope;
            }

            short sample = (short)Mathf.Clamp(wave * short.MaxValue * 0.6f, short.MinValue, short.MaxValue);
            pcmData[i * 2] = (byte)(sample & 0xFF);
            pcmData[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }

        return new AudioStreamWav
        {
            Format = AudioStreamWav.FormatEnum.Format16Bits,
            MixRate = sampleRate,
            Stereo = false,
            Data = pcmData
        };
    }
}

