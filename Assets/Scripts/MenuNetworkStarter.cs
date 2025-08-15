using System.Threading.Tasks;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuNetworkStarter : MonoBehaviour
{
    [Header("Fusion")]
    public NetworkRunner runnerPrefab;
    public SceneRef gameScene;

    private static NetworkRunner _runner;
    private bool _isStarting; // chặn double-click

    [Header("UI Roots")]
    public GameObject menuRoot;
    public GameObject createRoot;
    public GameObject joinRoot;

    [Header("Create UI")]
    public TMP_InputField createRoomInput;
    public Button createActionButton;

    [Header("Join UI")]
    public TMP_InputField joinRoomInput;
    public Button joinActionButton;

    [Header("Optional")]
    public TextMeshProUGUI statusLabel;

    void Awake()
    {
        ShowOnly(menuRoot);

        if (createRoomInput) createRoomInput.onValueChanged.AddListener(_ => Validate());
        if (joinRoomInput) joinRoomInput.onValueChanged.AddListener(_ => Validate());
        Validate();
    }

    // ======= NAV =======
    public void ShowCreate() { ShowOnly(createRoot); Focus(createRoomInput); }
    public void ShowJoin() { ShowOnly(joinRoot); Focus(joinRoomInput); }
    public void BackToMenu() { ShowOnly(menuRoot); EventSystem.current?.SetSelectedGameObject(null); }

    void ShowOnly(GameObject target)
    {
        if (menuRoot) menuRoot.SetActive(menuRoot == target);
        if (createRoot) createRoot.SetActive(createRoot == target);
        if (joinRoot) joinRoot.SetActive(joinRoot == target);
        Validate();
    }

    void Focus(TMP_InputField input)
    {
        if (!input) return;
        EventSystem.current?.SetSelectedGameObject(input.gameObject);
        input.ActivateInputField();
        input.caretPosition = input.text.Length;
    }

    void Validate()
    {
        bool validCreate = IsValid(createRoomInput ? createRoomInput.text : "");
        bool validJoin = IsValid(joinRoomInput ? joinRoomInput.text : "");

        if (createActionButton) createActionButton.interactable = !_isStarting && validCreate;
        if (joinActionButton) joinActionButton.interactable = !_isStarting && validJoin;

        if (statusLabel && !_isStarting) statusLabel.text = "";
    }

    bool IsValid(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        s = s.Trim();
        return s.Length >= 3 && s.Length <= 32;
    }

    // ======= Start Fusion (luôn tạo Runner mới) =======
    async Task<StartGameResult> StartWithNewRunner(GameMode mode, string session)
    {
        // Xoá runner cũ, KHÔNG tái sử dụng
        if (_runner != null)
        {
            try { if (_runner.IsRunning) await _runner.Shutdown(); }
            finally { Destroy(_runner.gameObject); _runner = null; }
        }

        _runner = Instantiate(runnerPrefab);
        _runner.name = "NetworkRunner";
        DontDestroyOnLoad(_runner);

        var sm = _runner.GetComponent<NetworkSceneManagerDefault>();
        if (sm == null) sm = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        _runner.ProvideInput = mode != GameMode.Single;

        var args = new StartGameArgs
        {
            GameMode = mode,
            SessionName = session.Trim(),
            Scene = gameScene,
            SceneManager = sm
        };

        var result = await _runner.StartGame(args);
        if (!result.Ok) Debug.LogError($"StartGame {mode} failed: {result.ShutdownReason}");
        return result;
    }

    // ======= BUTTON ACTIONS =======
    public async void OnClickCreateRoom()
    {
        if (_isStarting) return;
        var room = createRoomInput.text;
        if (!IsValid(room)) return;

        _isStarting = true; Validate();
        if (statusLabel) statusLabel.text = $"Creating '{room}'...";
        await StartWithNewRunner(GameMode.Host, room);
        _isStarting = false; Validate();
    }

    // Join-or-Create bằng AutoHostOrClient (1 call duy nhất)
    public async void OnClickJoinOrCreate()
    {
        if (_isStarting) return;
        var room = joinRoomInput.text;
        if (!IsValid(room)) return;

        _isStarting = true; Validate();
        if (statusLabel) statusLabel.text = $"Joining/Creating '{room}'...";
        await StartWithNewRunner(GameMode.AutoHostOrClient, room);
        _isStarting = false; Validate();
    }

    public async void OnClickSingle()
    {
        if (_isStarting) return;
        _isStarting = true; Validate();
        await StartWithNewRunner(GameMode.Single, "Single");
        _isStarting = false; Validate();
    }

    public void OnClickQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
