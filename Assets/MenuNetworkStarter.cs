using System.Threading.Tasks;
using Fusion;
using TMPro;                // nếu bạn dùng TMP InputField
using UnityEngine;

public class MenuNetworkStarter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Kéo prefab Runner (vd: Prototype Runner) vào đây")]
    public NetworkRunner runnerPrefab;

    [Tooltip("Chọn scene chơi (SceneRef của Fusion)")]
    public SceneRef gameScene;

    [Tooltip("Ô nhập tên phòng, có thể để trống")]
    public TMP_InputField roomNameInput;

    private static NetworkRunner _runner; // giữ singleton qua scene

    async Task StartGame(GameMode mode, string session)
    {
        // Nếu đã có runner (do Join/Create trước đó) thì dùng lại
        if (_runner == null)
        {
            _runner = Instantiate(runnerPrefab);
            _runner.name = "NetworkRunner";
            DontDestroyOnLoad(_runner);

            // Đảm bảo có SceneManager
            var sm = _runner.GetComponent<NetworkSceneManagerDefault>();
            if (sm == null) sm = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        // Tránh lỗi "NetworkRunner should not be reused" khi runner đang chạy
        if (_runner.IsRunning)
        {
            await _runner.Shutdown();
        }

        var startArgs = new StartGameArgs
        {
            GameMode = mode,                                   // Single / Host / Client
            SessionName = string.IsNullOrWhiteSpace(session) ? "MainRoom" : session,
            Scene = gameScene,                              // Fusion sẽ load sang scene chơi
            SceneManager = _runner.GetComponent<INetworkSceneManager>()
        };

        var result = await _runner.StartGame(startArgs);
        if (!result.Ok)
        {
            Debug.LogError($"StartGame failed: {result.ShutdownReason}");
        }
    }

    // ====== Các hàm gắn cho Button OnClick ======
    public void OnClickSingle()
    {
        _ = StartGame(GameMode.Single, "Single");
    }

    public void OnClickCreateRoom()
    {
        var room = roomNameInput ? roomNameInput.text : "Room1";
        _ = StartGame(GameMode.Host, room);
    }

    public void OnClickJoinRoom()
    {
        var room = roomNameInput ? roomNameInput.text : "Room1";
        _ = StartGame(GameMode.Client, room);
    }

    public void OnClickQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
