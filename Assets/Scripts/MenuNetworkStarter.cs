using System.Threading.Tasks;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuNetworkStarter : MonoBehaviour
{
    [Header("Fusion")]
    [Tooltip("Kéo prefab Runner (vd: Prototype Runner) vào đây")]
    public NetworkRunner runnerPrefab;

    [Tooltip("Chọn scene chơi (SceneRef của Fusion)")]
    public SceneRef gameScene;

    private static NetworkRunner _runner; // giữ runner qua scene

    [Header("UI: Panels/Canvas")]
    public GameObject menuRoot;        // Panel/Canvas Menu chính
    public GameObject createRoomRoot;  // Panel/Canvas Create Room

    [Header("UI: Room")]
    public TMP_InputField roomNameInput;
    public Button createButton;
    public Button joinButton;
    public TextMeshProUGUI errorLabel; // optional

    void Awake()
    {
        // Khởi trạng thái
        if (menuRoot) menuRoot.SetActive(true);
        if (createRoomRoot) createRoomRoot.SetActive(false);

        if (roomNameInput) roomNameInput.onValueChanged.AddListener(_ => ValidateRoomName());
        ValidateRoomName();
    }

    // ========= Toggle UI =========
    public void ShowCreateRoom()
    {
        if (menuRoot) menuRoot.SetActive(false);
        if (createRoomRoot) createRoomRoot.SetActive(true);

        if (roomNameInput)
        {
            EventSystem.current?.SetSelectedGameObject(roomNameInput.gameObject);
            roomNameInput.text = roomNameInput.text.Trim();
            roomNameInput.ActivateInputField();
        }
        ValidateRoomName();
    }

    public void BackToMenu()
    {
        if (createRoomRoot) createRoomRoot.SetActive(false);
        if (menuRoot) menuRoot.SetActive(true);
        EventSystem.current?.SetSelectedGameObject(null);
    }

    // ========= Validate tên phòng =========
    void ValidateRoomName()
    {
        string s = roomNameInput ? roomNameInput.text : "";
        bool valid = IsValidRoomName(s);

        if (createButton) createButton.interactable = valid;
        if (joinButton) joinButton.interactable = valid;
        if (errorLabel) errorLabel.text = valid ? "" : "Tên phòng 3–32 ký tự, không để trống.";
    }

    bool IsValidRoomName(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        s = s.Trim();
        return s.Length >= 3 && s.Length <= 32;
        // Nếu muốn giới hạn ký tự: dùng Regex: ^[\\w\\- ]{3,32}$
    }

    // ========= Start Fusion =========
    async Task StartGame(GameMode mode, string session)
    {
        if (_runner == null)
        {
            _runner = Instantiate(runnerPrefab);
            _runner.name = "NetworkRunner";
            DontDestroyOnLoad(_runner);

            var sm = _runner.GetComponent<NetworkSceneManagerDefault>();
            if (sm == null) sm = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        if (_runner.IsRunning)
            await _runner.Shutdown();

        var args = new StartGameArgs
        {
            GameMode = mode,                       // Single / Host / Client
            SessionName = session.Trim(),             // dùng đúng tên người chơi nhập
            Scene = gameScene,                  // scene gameplay
            SceneManager = _runner.GetComponent<INetworkSceneManager>()
        };

        var result = await _runner.StartGame(args);
        if (!result.Ok)
            Debug.LogError($"StartGame failed: {result.ShutdownReason}");
    }

    // ========= Button handlers =========
    public void OnClickSingle()
    {
        _ = StartGame(GameMode.Single, "Single");
    }

    public void OnClickCreateRoom()
    {
        if (!IsValidRoomName(roomNameInput.text)) return;
        _ = StartGame(GameMode.Host, roomNameInput.text);
    }

    public void OnClickJoinRoom()
    {
        if (!IsValidRoomName(roomNameInput.text)) return;
        _ = StartGame(GameMode.Client, roomNameInput.text);
    }

    public void OnClickQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
