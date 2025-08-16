using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioSettingsPanel : MonoBehaviour, ISettingsSection
{
    [Header("Music")]
    public Toggle musicEnabledToggle;
    public Slider musicVolumeSlider;           // 0..1
    public TextMeshProUGUI musicPercentLabel;  // optional

    [Header("SFX")]
    public Toggle sfxEnabledToggle;
    public Slider sfxVolumeSlider;             // 0..1
    public TextMeshProUGUI sfxPercentLabel;    // optional

    const string KEY_MUSIC_ON = "audio.music.on";
    const string KEY_MUSIC_VOL = "audio.music.vol";
    const string KEY_SFX_ON = "audio.sfx.on";
    const string KEY_SFX_VOL = "audio.sfx.vol";

    void Awake()
    {
        if (musicEnabledToggle) musicEnabledToggle.onValueChanged.AddListener(_ => OnAnyChanged());
        if (musicVolumeSlider) musicVolumeSlider.onValueChanged.AddListener(_ => OnAnyChanged());
        if (sfxEnabledToggle) sfxEnabledToggle.onValueChanged.AddListener(_ => OnAnyChanged());
        if (sfxVolumeSlider) sfxVolumeSlider.onValueChanged.AddListener(_ => OnAnyChanged());
    }

    void OnEnable() => LoadFromPrefs();

    void OnAnyChanged()
    {
        ApplyInteractableStates();
        UpdatePercentLabels();
        Debug.Log($"[Audio:Pending] Music {StateStr(musicEnabledToggle)} {Pct(musicVolumeSlider)}% | SFX {StateStr(sfxEnabledToggle)} {Pct(sfxVolumeSlider)}%");
    }

    void ApplyInteractableStates()
    {
        if (musicVolumeSlider && musicEnabledToggle)
            musicVolumeSlider.interactable = musicEnabledToggle.isOn;

        if (sfxVolumeSlider && sfxEnabledToggle)
            sfxVolumeSlider.interactable = sfxEnabledToggle.isOn;
    }

    void UpdatePercentLabels()
    {
        if (musicPercentLabel && musicVolumeSlider)
            musicPercentLabel.text = $"{Pct(musicVolumeSlider)}%";
        if (sfxPercentLabel && sfxVolumeSlider)
            sfxPercentLabel.text = $"{Pct(sfxVolumeSlider)}%";
    }

    int Pct(Slider s) => s ? Mathf.Clamp(Mathf.RoundToInt(Mathf.Round((s.value * 100f) / 5f) * 5f), 0, 100) : 0;
    string StateStr(Toggle t) => (t && t.isOn) ? "ON" : "OFF";

    // ===== ISettingsSection =====
    public void LoadFromPrefs()
    {
        bool mOn = PlayerPrefs.GetInt(KEY_MUSIC_ON, 1) == 1;
        float mVl = PlayerPrefs.GetFloat(KEY_MUSIC_VOL, 0.8f);
        bool xOn = PlayerPrefs.GetInt(KEY_SFX_ON, 1) == 1;
        float xVl = PlayerPrefs.GetFloat(KEY_SFX_VOL, 0.8f);

        if (musicEnabledToggle) musicEnabledToggle.isOn = mOn;
        if (musicVolumeSlider) musicVolumeSlider.value = Mathf.Clamp01(mVl);
        if (sfxEnabledToggle) sfxEnabledToggle.isOn = xOn;
        if (sfxVolumeSlider) sfxVolumeSlider.value = Mathf.Clamp01(xVl);

        ApplyInteractableStates();
        UpdatePercentLabels();
    }

    public void ApplyAndSave()
    {
        if (musicEnabledToggle) PlayerPrefs.SetInt(KEY_MUSIC_ON, musicEnabledToggle.isOn ? 1 : 0);
        if (musicVolumeSlider) PlayerPrefs.SetFloat(KEY_MUSIC_VOL, musicVolumeSlider.value);
        if (sfxEnabledToggle) PlayerPrefs.SetInt(KEY_SFX_ON, sfxEnabledToggle.isOn ? 1 : 0);
        if (sfxVolumeSlider) PlayerPrefs.SetFloat(KEY_SFX_VOL, sfxVolumeSlider.value);
        PlayerPrefs.Save();

        Debug.Log($"[Audio:Applied] Music {StateStr(musicEnabledToggle)} {Pct(musicVolumeSlider)}% | SFX {StateStr(sfxEnabledToggle)} {Pct(sfxVolumeSlider)}%");
    }
}
