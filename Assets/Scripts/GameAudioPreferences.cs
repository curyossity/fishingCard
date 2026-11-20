using UnityEngine;

/// <summary>Shares the existing player sound preference between game phases without resetting saved choices.</summary>
public static class GameAudioPreferences
{
    private const string SoundKey = "Deepwater.Landing.Sound";
    public static bool SoundEnabled => PlayerPrefs.GetInt(SoundKey, 1) != 0;
    /// <summary>Persists only an explicit sound-setting change.</summary>
    public static void SetSoundEnabled(bool value) { PlayerPrefs.SetInt(SoundKey, value ? 1 : 0); PlayerPrefs.Save(); }
}
