using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VideoSettingsPanel : MonoBehaviour, ISettingsSection
{
    [Header("UI")]
    public TMP_Dropdown resolutionDropdown;  // dùng TMP_Dropdown
    public TMP_Dropdown qualityDropdown;     // Low / Medium / High
    public Toggle vSyncToggle;
    public TextMeshProUGUI debugLabel;       // optional

    const string KEY_RES_W = "video.res.w";
    const string KEY_RES_H = "video.res.h";
    const string KEY_QUALITY3 = "video.quality3"; // 0=Low,1=Medium,2=High
    const string KEY_VSYNC = "video.vsync";

    private List<(int w, int h)> _resList = new();

    void Awake()
    {
        if (resolutionDropdown) resolutionDropdown.onValueChanged.AddListener(_ => LogPending());
        if (qualityDropdown) qualityDropdown.onValueChanged.AddListener(_ => LogPending());
        if (vSyncToggle) vSyncToggle.onValueChanged.AddListener(_ => LogPending());

        BuildResolutionOptions();
        BuildQualityOptions_Three();
    }

    void OnEnable() => LoadFromPrefs();

    void BuildResolutionOptions()
    {
        _resList.Clear();
        var uniq = new HashSet<(int, int)>();
        foreach (var r in Screen.resolutions)
            if (uniq.Add((r.width, r.height))) _resList.Add((r.width, r.height));
        _resList = _resList.OrderBy(x => x.w * x.h).ToList();

        if (resolutionDropdown)
        {
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(_resList.Select(x => $"{x.w} x {x.h}").ToList());
            resolutionDropdown.RefreshShownValue();
        }
    }

    void BuildQualityOptions_Three()
    {
        if (!qualityDropdown) return;
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string> { "Low", "Medium", "High" });
        qualityDropdown.RefreshShownValue();
    }

    int FindResIndex(int w, int h)
    {
        int idx = _resList.FindIndex(x => x.w == w && x.h == h);
        return Mathf.Clamp(idx < 0 ? _resList.Count - 1 : idx, 0, Mathf.Max(0, _resList.Count - 1));
    }

    void LogPending()
    {
        if (!resolutionDropdown || _resList.Count == 0) return;
        var (w, h) = _resList[Mathf.Clamp(resolutionDropdown.value, 0, _resList.Count - 1)];
        string q = qualityDropdown ? new[] { "Low", "Medium", "High" }[Mathf.Clamp(qualityDropdown.value, 0, 2)] : "Medium";
        string vs = (vSyncToggle && vSyncToggle.isOn) ? "ON" : "OFF";
        if (debugLabel) debugLabel.text = $"(Pending) {w}x{h}, {q}, VSync {vs}";
        Debug.Log($"[Video:Pending] {w}x{h} | {q} | VSync {vs}");
    }

    // ===== ISettingsSection =====
    public void LoadFromPrefs()
    {
        int w = PlayerPrefs.GetInt(KEY_RES_W, Screen.width);
        int h = PlayerPrefs.GetInt(KEY_RES_H, Screen.height);
        int q3 = PlayerPrefs.GetInt(KEY_QUALITY3, MapUnityLevelTo3(QualitySettings.GetQualityLevel()));
        bool vs = PlayerPrefs.GetInt(KEY_VSYNC, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;

        if (resolutionDropdown) resolutionDropdown.value = FindResIndex(w, h);
        if (qualityDropdown) qualityDropdown.value = Mathf.Clamp(q3, 0, 2);
        if (vSyncToggle) vSyncToggle.isOn = vs;

        resolutionDropdown?.RefreshShownValue();
        qualityDropdown?.RefreshShownValue();
        LogPending();
    }

    public void ApplyAndSave()
    {
        // Lấy lựa chọn hiện tại
        var (w, h) = _resList[Mathf.Clamp(resolutionDropdown.value, 0, _resList.Count - 1)];
        int q3 = Mathf.Clamp(qualityDropdown.value, 0, 2);
        bool vs = vSyncToggle && vSyncToggle.isOn;

        // Ánh xạ 3 mức → Unity quality level và áp dụng
        int targetLevel = Map3ToUnityLevel(q3);
        QualitySettings.SetQualityLevel(targetLevel, true);

        // VSync
        QualitySettings.vSyncCount = vs ? 1 : 0;

        // Resolution (giữ chế độ fullscreen hiện tại)
        var mode = Screen.fullScreenMode;
#if UNITY_2022_2_OR_NEWER
        Screen.SetResolution(w, h, mode);
#else
        Screen.SetResolution(w, h, mode, Screen.currentResolution.refreshRate);
#endif

        // Lưu
        PlayerPrefs.SetInt(KEY_RES_W, w);
        PlayerPrefs.SetInt(KEY_RES_H, h);
        PlayerPrefs.SetInt(KEY_QUALITY3, q3);
        PlayerPrefs.SetInt(KEY_VSYNC, vs ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"[Video:Applied] {w}x{h} | {(q3 == 0 ? "Low" : q3 == 1 ? "Medium" : "High")} (→ {QualitySettings.names[targetLevel]}) | VSync {(vs ? "ON" : "OFF")}");
        if (debugLabel) debugLabel.text = $"Applied: {w}x{h}, {(q3 == 0 ? "Low" : q3 == 1 ? "Medium" : "High")}, VSync {(vs ? "ON" : "OFF")}";
    }

    // Map hiện tại của project về 3 mức
    int MapUnityLevelTo3(int unityLevel)
    {
        int n = Mathf.Max(1, QualitySettings.names.Length);
        if (n <= 1) return 1;
        float t = unityLevel / (n - 1f);
        if (t < 1f / 3f) return 0;
        if (t < 2f / 3f) return 1;
        return 2;
    }

    // Map 3 mức → level Unity gần nhất
    int Map3ToUnityLevel(int q3)
    {
        int n = Mathf.Max(1, QualitySettings.names.Length);
        if (q3 <= 0) return 0;
        if (q3 >= 2) return n - 1;
        return Mathf.RoundToInt((n - 1) * 0.5f);
    }
}
