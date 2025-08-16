using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject videoPanel;

    [Header("Left Buttons (optional, for state)")]
    [SerializeField] private Button gameplayButton;
    [SerializeField] private Button audioButton;
    [SerializeField] private Button videoButton;

    [Header("Focus targets (optional)")]
    [SerializeField] private GameObject gameplayFirstFocus; // ex: a slider/toggle in Gameplay
    [SerializeField] private GameObject audioFirstFocus;    // ex: master volume slider
    [SerializeField] private GameObject videoFirstFocus;    // ex: resolution dropdown

    [Header("Labels (optional)")]
    [SerializeField] private TextMeshProUGUI sectionLabel;  // ex: "Game Play" / "Audio" / "Video Settings"

    [Header("Back action (wire to your menu)")]
    public UnityEvent onBack; // map to MenuNetworkStarter.BackToMenu() (hoặc bất kỳ hàm nào)

    void OnEnable()
    {
        // Mặc định mở tab Gameplay khi vào Settings
        ShowGameplay();
    }

    // ===== Public handlers to hook from buttons =====
    public void ShowGameplay()
    {
        ShowOnly(gameplayPanel);
        UpdateSection("Game Play");
        Focus(gameplayFirstFocus);
        SetTabStates(gameplayButton);
    }

    public void ShowAudio()
    {
        ShowOnly(audioPanel);
        UpdateSection("Audio");
        Focus(audioFirstFocus);
        SetTabStates(audioButton);
    }

    public void ShowVideo()
    {
        ShowOnly(videoPanel);
        UpdateSection("Video Settings");
        Focus(videoFirstFocus);
        SetTabStates(videoButton);
    }

    public void Back()
    {
        onBack?.Invoke();
    }

    // ===== Internals =====
    void ShowOnly(GameObject target)
    {
        if (gameplayPanel) gameplayPanel.SetActive(gameplayPanel == target);
        if (audioPanel) audioPanel.SetActive(audioPanel == target);
        if (videoPanel) videoPanel.SetActive(videoPanel == target);
    }

    void UpdateSection(string text)
    {
        if (sectionLabel) sectionLabel.text = text;
    }

    void Focus(GameObject go)
    {
        if (!go) return;
        EventSystem.current?.SetSelectedGameObject(go);
        // Nếu là TMP_InputField/Selectable, Unity sẽ tự focus highlight/caret
    }

    void SetTabStates(Button active)
    {
        if (!active) return;
        Button[] arr = { gameplayButton, audioButton, videoButton };
        foreach (var b in arr)
        {
            if (!b) continue;
            // Cho cảm giác "đang chọn": nút active không bấm được, các nút khác bấm được
            b.interactable = (b != active);
        }
    }
}
