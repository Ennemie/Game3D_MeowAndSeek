using Fusion;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Networked Properties")]
    [Networked, OnChangedRender(nameof(OnGameStartedChanged))]
    public NetworkBool GameStarted { get; set; } = false;

    [Networked] public int CountReady { get; set; }
    [Networked] public PlayerRef SeekerPlayerRef { get; set; } = PlayerRef.None;
    [Networked] public TickTimer RoleTimer { get; set; }
    [Networked] public int GameTime { get; set; }
    private int timer;

    [Header("References")]
    private CanvaController canva;
    private GameObject _openingCam;
    private bool isLocalSeeker = false;

    private int seekerCount;
    private int hiderCount;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public override void Spawned()
    {
        Instance = this;
        canva = GameObject.FindWithTag("Canva")?.GetComponent<CanvaController>();
        _openingCam = GameObject.FindWithTag("OpeningCamera");

        if (GameStarted)
        {
            OnGameStartedChanged();
        }
    }

    private void OnGameStartedChanged()
    {
        if (GameStarted)
        {
            StartGameLogic();
        }
        else
        {
            EndGameLogic();
        }
    }

    private IEnumerator ResetGame()
    {
        yield return new WaitForSeconds(5f);
        if (!Object.HasStateAuthority) yield break;

        // Reset networked state
        GameStarted = false;
        CountReady = 0;
        SeekerPlayerRef = PlayerRef.None;
        RoleTimer = TickTimer.None;
        GameTime = 180;

        // Teleport players back to spawn points and reset their properties
        var playerSpawn = FindObjectOfType<PlayerSpawn>();
        Transform[] spawnPoints = null;
        if (playerSpawn != null)
        {
            // access private serialized field spawnPoints via reflection
            var fi = typeof(PlayerSpawn).GetField("spawnPoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fi != null)
            {
                spawnPoints = fi.GetValue(playerSpawn) as Transform[];
            }
        }

        var playersList = Runner.ActivePlayers.ToList();
        playersList.Sort((a, b) => a.RawEncoded.CompareTo(b.RawEncoded));

        for (int i = 0; i < playersList.Count; i++)
        {
            var player = playersList[i];
            var playerObj = Runner.GetPlayerObject(player);
            if (playerObj == null) continue;

            // Teleport to spawn point if available
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                int index = i;
                index = Mathf.Clamp(index, 0, spawnPoints.Length - 1);

                var target = spawnPoints[index];
                if (target != null)
                {
                    var cc = playerObj.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = false;

                    playerObj.transform.position = target.position;
                    playerObj.transform.rotation = target.rotation;

                    if (cc != null) cc.enabled = true;
                }
            }

            // Reset player properties back to default
            var props = playerObj.GetComponent<PlayerProperties>();
            var mov = playerObj.GetComponent<PlayerMovement>();
            if (props != null)
            {
                // if this player was a seeker, revert any speed buff applied
                if (props.isSeeker && mov != null)
                {
                    mov.speed = mov.speed / 1.2f;
                }

                props.isSeeker = false;
                props.isDead = false;
                props.Hp = props.hpBar != null ? props.hpBar.maxValue : 100f;
                props.Mana = props.manaBar != null ? props.manaBar.maxValue : 100f;
                props.disguiseIndex = 0;
                props.isAttacking = false;
                props.isShieldActive = false;

                if (props.attackEffect != null) props.attackEffect.gameObject.SetActive(false);
                if (props.shield != null) props.shield.SetActive(false);
            }

            if (mov != null)
            {
                mov.isMovementEnabled = false;
                mov.isSkillEnabled = false;
            }
        }

        // Optionally reset manager-local counters
        seekerCount = 0;
        hiderCount = 0;
    }

    public void HiderCountUpdate()
    {
        hiderCount--;
        if (hiderCount <= 0 && Object.HasStateAuthority)
        {
            GameStarted = false;
        }
    }

    private void StartGameLogic()
    {
        if (_openingCam != null) _openingCam.SetActive(false);

        // --- PHẦN VIẾT THÊM: Đồng bộ Nickname lên PlayFab Leaderboard ---
        var myObj = Runner.GetPlayerObject(Runner.LocalPlayer);
        if (myObj != null)
        {
            var props = myObj.GetComponent<PlayerProperties>();
            if (props != null)
            {
                // Gọi hàm đồng bộ tên (Hàm UpdateNameOnLeaderboard ông đã thêm ở dưới)
                UpdateNameOnLeaderboard(props.NickName.ToString());
            }
        }
        // ---------------------------------------------------------------

        seekerCount = 0;
        hiderCount = 0;

        foreach (var player in Runner.ActivePlayers)
        {
            var playerObj = Runner.GetPlayerObject(player);
            if (playerObj != null)
            {
                var props = playerObj.GetComponent<PlayerProperties>();
                if (props != null)
                {
                    if (player == SeekerPlayerRef) seekerCount++;
                    else hiderCount++;
                }
            }
        }
        Debug.Log(seekerCount + " seeker(s) and " + hiderCount + " hider(s) in the game.");

        StartCoroutine(DisplayGameTimeRoutine());
        SetUpRolesAndPermissions();
    }

    private void SetUpRolesAndPermissions()
    {
        var myObj = Runner.GetPlayerObject(Runner.LocalPlayer);
        if (myObj == null) return;

        var props = myObj.GetComponent<PlayerProperties>();
        var mov = myObj.GetComponent<PlayerMovement>();

        isLocalSeeker = (SeekerPlayerRef == Runner.LocalPlayer);

        if (isLocalSeeker && props != null)
        {
            props.isSeeker = true;
            if (mov != null) mov.speed *= 1.2f;
        }

        StartCoroutine(RoleCountdownRoutine(mov));
    }

    private IEnumerator RoleCountdownRoutine(PlayerMovement mov)
    {
        while (RoleTimer.IsRunning && !RoleTimer.Expired(Runner))
        {
            float? remaining = RoleTimer.RemainingTime(Runner);
            int seconds = Mathf.CeilToInt(remaining ?? 0);

            if (isLocalSeeker)
            {
                canva.readiedHubText.text = $"BẠN LÀ <color=red>SEEKER</color>! ĐỢI TRONG: {seconds}";
                if (mov != null) mov.isMovementEnabled = false;
            }
            else
            {
                canva.readiedHubText.text = $"BẠN LÀ <color=green>HIDER</color>! TRỐN ĐI: {seconds}";
                if (mov != null)
                {
                    mov.isMovementEnabled = true;
                    mov.isSkillEnabled = true;
                }
            }
            yield return new WaitForSeconds(0.2f);
        }

        if (mov != null)
        {
            mov.isMovementEnabled = true;
            mov.isSkillEnabled = true;
        }

        canva.readiedHubText.text = isLocalSeeker ? "BẮT ĐẦU ĐI SĂN!" : "SEEKER ĐÃ BẮT ĐẦU ĐI SĂN!";

        yield return new WaitForSeconds(3f);
        canva.readiedHub.SetActive(false);
    }

    private IEnumerator DisplayGameTimeRoutine()
    {
        timer = GameTime;
        while (timer > 0 && GameStarted)
        {
            if (canva != null) canva.UpdateGameTime(timer);
            yield return new WaitForSeconds(1f);
            timer--;
        }

        if (Object.HasStateAuthority && GameStarted)
        {
            GameStarted = false;
        }
    }

    private void EndGameLogic()
    {
        if (canva != null)
        {
            canva.readiedHub.SetActive(true);
            canva.HideDeadHub();
            canva.readiedHubText.text = "GAME KẾT THÚC!";
        }

        // Xác định đội thắng
        string finalWinner = hiderCount > 0 ? "HIDER" : "SEEKER";

        if (canva != null)
            canva.readiedHubText.text += $"\n<color={(hiderCount > 0 ? "green" : "red")}>{finalWinner} THẮNG!</color>";

        // QUAN TRỌNG: Chỉ máy chủ (State Authority) mới được quyền gửi dữ liệu trận đấu
        if (Object.HasStateAuthority)
        {
            CollectAndSendPlayFabData(finalWinner);
        }

        // Bắt đầu đếm ngược reset game
        //StartCoroutine(ResetGame());
    }
    public void UpdateNameOnLeaderboard(string nickName)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = nickName
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            result => Debug.Log("<color=green>PlayFab: Đã đồng bộ NickName lên Server!</color>"),
            error => Debug.LogError("PlayFab: Lỗi đồng bộ tên: " + error.GenerateErrorReport())
        );
    }
    private void CollectAndSendPlayFabData(string winnerTeam)
    {
        Debug.Log("[PlayFab] Đang thu thập dữ liệu cuối trận...");

        MatchResult result = new MatchResult();
        result.winnerTeam = winnerTeam;

        int finalDuration = GameTime - timer;
        result.durationSeconds = finalDuration;

        foreach (var playerRef in Runner.ActivePlayers)
        {
            var playerObj = Runner.GetPlayerObject(playerRef);
            if (playerObj != null)
            {
                var props = playerObj.GetComponent<PlayerProperties>();
                if (props != null)
                {
                    MatchPlayerInfo pInfo = new MatchPlayerInfo();
                    pInfo.playerName = props.NickName.ToString();
                    pInfo.role = props.isSeeker ? "Seeker" : "Hider";

                    if (winnerTeam == "SEEKER")
                        pInfo.isWinner = props.isSeeker;
                    else
                        pInfo.isWinner = !props.isSeeker;

                    result.players.Add(pInfo);
                }
            }
        }

        if (PlayFabManager.Instance != null && result.players.Count > 0)
        {
            // 1. Gửi Match Data (Player Data) và PlayStream Event qua hàm gộp
            PlayFabManager.Instance.SendMatchDataAndEvent(result);

            // 2. PHẦN CHỈNH SỬA: Cập nhật Bảng xếp hạng (Leaderboard)
            var statsRequest = new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate> {
                    new StatisticUpdate {
                        StatisticName = "BestDurationMatches", // Tên này phải khớp 100% trên Web
                        Value = finalDuration
                    }
                }
            };

            PlayFabClientAPI.UpdatePlayerStatistics(statsRequest,
                res => Debug.Log($"<color=yellow>PlayFab: Đã cập nhật Duration ({finalDuration}s) lên Leaderboard!</color>"),
                err => Debug.LogError("PlayFab: Lỗi Leaderboard: " + err.GenerateErrorReport())
            );
        }
        else
        {
            Debug.LogWarning("[PlayFab] Không có dữ liệu để gửi hoặc PlayFabManager chưa khởi tạo!");
        }
    }

    public void PlayerReadied()
    {
        if (Object != null && Object.IsValid) RPC_RequestReady();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestReady()
    {
        CountReady++;
        if (CountReady >= Runner.ActivePlayers.Count())
        {
            var allPlayers = Runner.ActivePlayers.ToList();
            if (allPlayers.Count > 0)
                SeekerPlayerRef = allPlayers[Random.Range(0, allPlayers.Count)];

            GameTime = 190;

            RoleTimer = TickTimer.CreateFromSeconds(Runner, 10f);

            GameStarted = true;
        }
    }

    public string GetPlayerName(PlayerRef playerRef)
    {
        var playerObj = Runner.GetPlayerObject(playerRef);
        if (playerObj != null)
        {
            var props = playerObj.GetComponent<PlayerProperties>();
            if (props != null) return props.NickName.ToString();
        }
        return "Unknown";
    }
    public void ScrollChatToBottom()
    {
        if (canva != null) canva.ScrollChatHubToBottom();
    }   
}