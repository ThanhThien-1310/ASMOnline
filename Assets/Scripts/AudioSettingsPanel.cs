using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioSettingsPanel : MonoBehaviour
{
    [Header("Music")]
    public Toggle musicEnabledToggle;
    public Slider musicVolumeSlider;           // 0..1
    public TextMeshProUGUI musicPercentLabel;  // optional (có thể để trống)

    [Header("SFX")]
    public Toggle sfxEnabledToggle;
    public Slider sfxVolumeSlider;             // 0..1
    public TextMeshProUGUI sfxPercentLabel;    // optional

    // PlayerPrefs keys
    const string KEY_MUSIC_ON = "audio.music.on";
    const string KEY_MUSIC_VOL = "audio.music.vol";
    const string KEY_SFX_ON = "audio.sfx.on";
    const string KEY_SFX_VOL = "audio.sfx.vol";

    void Awake()
    {
        // Gắn listener
        if (musicEnabledToggle) musicEnabledToggle.onValueChanged.AddListener(_ => OnMusicToggleChanged());
        if (musicVolumeSlider) musicVolumeSlider.onValueChanged.AddListener(_ => OnMusicVolumeChanged());

        if (sfxEnabledToggle) sfxEnabledToggle.onValueChanged.AddListener(_ => OnSfxToggleChanged());
        if (sfxVolumeSlider) sfxVolumeSlider.onValueChanged.AddListener(_ => OnSfxVolumeChanged());
    }

    void OnEnable()
    {
        // Load mặc định nếu chưa có: ON & 0.8
        bool musicOn = PlayerPrefs.GetInt(KEY_MUSIC_ON, 1) == 1;
        float musicVol = PlayerPrefs.GetFloat(KEY_MUSIC_VOL, 0.8f);
        bool sfxOn = PlayerPrefs.GetInt(KEY_SFX_ON, 1) == 1;
        float sfxVol = PlayerPrefs.GetFloat(KEY_SFX_VOL, 0.8f);

        if (musicEnabledToggle) musicEnabledToggle.isOn = musicOn;
        if (musicVolumeSlider) musicVolumeSlider.value = Mathf.Clamp01(musicVol);
        if (sfxEnabledToggle) sfxEnabledToggle.isOn = sfxOn;
        if (sfxVolumeSlider) sfxVolumeSlider.value = Mathf.Clamp01(sfxVol);

        // Cập nhật UI/Debug lần đầu
        ApplyInteractableStates();
        UpdatePercentLabels();
        LogState("Init");
    }

    // ===== Callbacks =====
    void OnMusicToggleChanged()
    {
        ApplyInteractableStates();
        Save();
        LogState("Music Toggle");
    }

    void OnMusicVolumeChanged()
    {
        UpdatePercentLabels();
        Save();
        LogState("Music Volume");
    }

    void OnSfxToggleChanged()
    {
        ApplyInteractableStates();
        Save();
        LogState("SFX Toggle");
    }

    void OnSfxVolumeChanged()
    {
        UpdatePercentLabels();
        Save();
        LogState("SFX Volume");
    }

    // ===== Helpers =====
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
            musicPercentLabel.text = $"{Round5(musicVolumeSlider.value * 100f)}%";

        if (sfxPercentLabel && sfxVolumeSlider)
            sfxPercentLabel.text = $"{Round5(sfxVolumeSlider.value * 100f)}%";
    }

    int Round5(float value01to100)
    {
        // làm tròn bậc 5% cho “không cần quá chính xác”
        return Mathf.Clamp(Mathf.RoundToInt(Mathf.Round(value01to100 / 5f) * 5f), 0, 100);
    }

    void Save()
    {
        if (musicEnabledToggle) PlayerPrefs.SetInt(KEY_MUSIC_ON, musicEnabledToggle.isOn ? 1 : 0);
        if (musicVolumeSlider) PlayerPrefs.SetFloat(KEY_MUSIC_VOL, musicVolumeSlider.value);
        if (sfxEnabledToggle) PlayerPrefs.SetInt(KEY_SFX_ON, sfxEnabledToggle.isOn ? 1 : 0);
        if (sfxVolumeSlider) PlayerPrefs.SetFloat(KEY_SFX_VOL, sfxVolumeSlider.value);
        PlayerPrefs.Save();
    }

    void LogState(string tag)
    {
        string musicStr = musicEnabledToggle ? (musicEnabledToggle.isOn ? "ON" : "OFF") : "NA";
        string sfxStr = sfxEnabledToggle ? (sfxEnabledToggle.isOn ? "ON" : "OFF") : "NA";
        int musicPct = musicVolumeSlider ? Round5(musicVolumeSlider.value * 100f) : -1;
        int sfxPct = sfxVolumeSlider ? Round5(sfxVolumeSlider.value * 100f) : -1;

        Debug.Log($"[Audio:{tag}] Music={musicStr} ({musicPct}%) | SFX={sfxStr} ({sfxPct}%)", this);
    }

    // ===== Optional buttons =====
    public void OnClickResetDefaults()
    {
        if (musicEnabledToggle) musicEnabledToggle.isOn = true;
        if (musicVolumeSlider) musicVolumeSlider.value = 0.8f;
        if (sfxEnabledToggle) sfxEnabledToggle.isOn = true;
        if (sfxVolumeSlider) sfxVolumeSlider.value = 0.8f;

        ApplyInteractableStates();
        UpdatePercentLabels();
        Save();
        LogState("ResetDefaults");
    }
}
