using UnityEngine;

public class SettingsApplyController : MonoBehaviour
{
    [Tooltip("Kéo các component panel (AudioSettingsPanel, VideoSettingsPanel, GameplaySettingsPanel, ...) vào đây")]
    public MonoBehaviour[] sections; // các component phải implement ISettingsSection

    ISettingsSection[] _cached;

    void Awake()
    {
        _cached = new ISettingsSection[sections.Length];
        for (int i = 0; i < sections.Length; i++)
            _cached[i] = sections[i] as ISettingsSection;
    }

    // Gọi khi mở Settings (hoặc khi Cancel)
    public void LoadAllFromPrefs()
    {
        foreach (var s in _cached)
            s?.LoadFromPrefs();
    }

    // Gán vào nút "Apply"
    public void OnClickApply()
    {
        foreach (var s in _cached)
            s?.ApplyAndSave();

        Debug.Log("[Settings] All sections applied & saved.");
    }
}
