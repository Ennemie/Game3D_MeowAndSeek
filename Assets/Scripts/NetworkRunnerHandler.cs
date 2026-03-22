using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class NetworkRunnerHandler : MonoBehaviour
{
    private NetworkRunner _runner;

    async void Start()
    {
        // Lấy chính component NetworkRunner trên đối tượng này
        _runner = GetComponent<NetworkRunner>();

        // Nếu chưa có SceneManager thì thêm vào
        var sceneManager = GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null)
        {
            sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        // Bắt đầu Game trực tiếp trên Runner này
        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "Shared room",
            PlayerCount = 4, // 🔥 KHÓA TỐI ĐA 4 NGƯỜI
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = sceneManager
        });

        if (result.Ok)
        {
            Debug.Log("Runner đã sẵn sàng và vào phòng!");
        }
        else
        {
            Debug.LogError($"Lỗi: {result.ShutdownReason}");
        }
    }
}