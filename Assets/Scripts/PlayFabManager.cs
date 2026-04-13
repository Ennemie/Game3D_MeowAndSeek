using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using System.Collections.Generic;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Login();
    }

    private void Login()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request,
            result => Debug.Log("<color=green>PlayFab: Đăng nhập thành công!</color>"),
            error => Debug.LogError("PlayFab: Lỗi đăng nhập: " + error.GenerateErrorReport())
        );
    }

    // Hàm gửi dữ liệu trận đấu
    public void SendMatchDataAndEvent(MatchResult data)
    {
        if (!PlayFabClientAPI.IsClientLoggedIn()) return;

        // 1. Chuẩn bị dữ liệu JSON
        string jsonContent = JsonUtility.ToJson(data);

        // 2. Tạo Key dựa trên thời gian (Ví dụ: Match_2026-04-13_01-10-05)
        string timeKey = "Match_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        // --- HÀNH ĐỘNG 1: Gửi Player Data với Key là thời gian ---
        var dataRequest = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> {
                { timeKey, jsonContent }
            }
        };
        PlayFabClientAPI.UpdateUserData(dataRequest,
            result => Debug.Log($"<color=green>PlayFab: Đã lưu Data với Key: {timeKey}</color>"),
            error => Debug.LogError("PlayFab: Lỗi lưu Data: " + error.GenerateErrorReport()));


        // --- HÀNH ĐỘNG 2: Gửi PlayStream Event (Nhật ký hệ thống) ---
        var eventRequest = new WriteClientPlayerEventRequest
        {
            EventName = "match_finished",
            Body = new Dictionary<string, object>
            {
                { "MatchTime", timeKey }, // Gắn thêm cái Key thời gian vào Event để dễ đối chiếu
                { "WinnerTeam", data.winnerTeam },
                { "Duration", data.durationSeconds },
                { "Players", data.players }
            }
        };
        PlayFabClientAPI.WritePlayerEvent(eventRequest,
            result => Debug.Log("<color=cyan>PlayFab: Đã ghi nhận PlayStream Event!</color>"),
            null);
    }
}

// Các lớp định nghĩa dữ liệu để chuyển sang JSON
[System.Serializable]
public class MatchPlayerInfo
{
    public string playerName;
    public string role;
    public bool isWinner;
}

[System.Serializable]
public class MatchResult
{
    public string winnerTeam;
    public int durationSeconds;
    public List<MatchPlayerInfo> players = new List<MatchPlayerInfo>();
}