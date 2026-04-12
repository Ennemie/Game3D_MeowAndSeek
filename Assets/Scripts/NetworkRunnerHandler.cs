using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

[RequireComponent(typeof(NetworkRunner))]
public class NetworkRunnerHandler : MonoBehaviour
{
    private NetworkRunner _runner;

    async void Start()
    {
        _runner = GetComponent<NetworkRunner>();

        // 🔥 CHẶN 100% REUSE
        if (_runner == null)
        {
            Debug.LogError("❌ Không có NetworkRunner");
            return;
        }

        if (_runner.IsRunning || _runner.State != NetworkRunner.States.Shutdown)
        {
            Debug.LogWarning("⚠️ Runner đã được start trước đó → BỎ QUA");
            return;
        }

        await StartGame();
    }

    private async Task StartGame()
    {
        _runner.ProvideInput = true;

        var sceneManager = GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null)
        {
            sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        Debug.Log("🚀 StartGame...");

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "Room_" + Random.Range(1000, 9999),
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = sceneManager
        });

        if (result.Ok)
        {
            Debug.Log("✅ OK");
        }
        else
        {
            Debug.LogError($"❌ {result.ShutdownReason}");
        }
    }
}