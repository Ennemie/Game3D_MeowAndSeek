using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvaController : MonoBehaviour
{
    private string playerName;
    private Coroutine disguiseRoutine; // Quản lý Coroutine để tránh chồng chéo

    [Header("Name Hub")]
    public GameObject nameHub;
    public TMP_Text nameHubText;
    public TMP_InputField nameInputField;
    public Button enterRoomBtn;

    [Header("Ready Hub")]
    public GameObject readyHub;
    public Button readyBtn;

    [Header("Readied Hub")]
    public GameObject readiedHub;
    public TMP_Text readiedHubText;

    [Header("Play Hub")]
    public GameObject playHub;
    public TMP_Text gameTimeText;

    [Header("Disguise Hub")]
    public GameObject disguiseHub;
    public TMP_Text playerNameText;
    public Slider hpBar;
    public Slider manaBar;
    public TMP_Text disguiseTimeText;

    [Header("Dead Hub")]
    public GameObject deadHub;

    void Awake()
    {
        // Khởi tạo trạng thái ban đầu: Ẩn tất cả trừ Name Hub (hoặc tùy logic game của bạn)
        HideAllHubs();
    }

    private void HideAllHubs()
    {
        nameHub.SetActive(false);
        readyHub.SetActive(false);
        readiedHub.SetActive(false);
        playHub.SetActive(false);
        disguiseHub.SetActive(false);
        deadHub.SetActive(false);
    }

    // --- LOGIC PHÒNG CHỜ (LOBBY) ---

    public void EnterGame()
    {
        HideAllHubs();
        nameHub.SetActive(true);
        nameInputField.text = string.Empty;
        nameHubText.text = "Nhập tên của bạn";
    }

    public void EnterRoom()
    {
        playerName = nameInputField.text;
        if (string.IsNullOrWhiteSpace(playerName))
        {
            nameHubText.text = "<color=red>Vui lòng nhập tên!</color>";
            return;
        }

        nameHub.SetActive(false);
        readyHub.SetActive(true);
        // playHub thường hiện các thông số cơ bản khi vào phòng
        playHub.SetActive(true);

        DisplayName();
    }

    public void ReadiedHub()
    {
        readyHub.SetActive(false);
        readiedHub.SetActive(true);
        // Gọi GameManager thông qua Singleton
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerReadied();
        }
    }

    private void DisplayName()
    {
        // Tìm Player của máy local để gán tên lên Network
        PlayerProperties[] allPlayers = FindObjectsByType<PlayerProperties>(FindObjectsSortMode.None);
        foreach (var player in allPlayers)
        {
            if (player.Object != null && player.Object.HasInputAuthority)
            {
                player.SetMyName(playerName);
                break;
            }
        }
    }

    // --- LOGIC TRONG TRẬN ĐẤU (GAMEPLAY) ---

    public void OpenDisguiseHub(string pName, float currentHp, float currentMana, int duration)
    {
        disguiseHub.SetActive(true);
        playerNameText.text = pName;
        playerNameText.color = Color.green;

        // Cập nhật thanh trạng thái
        UpdateHpBar(currentHp);

        // Xử lý đếm ngược: Dừng cái cũ nếu đang chạy
        if (disguiseRoutine != null)
        {
            StopCoroutine(disguiseRoutine);
        }
        disguiseRoutine = StartCoroutine(ShowDisguiseTime(duration));
    }

    public void UpdateHpBar(float hp)
    {
        if (hpBar != null) hpBar.value = hp;
    }
    public void UpdateManaBar(float mana)
    {
        if (manaBar != null) manaBar.value = mana;
    }

    public void CloseDisguiseHub()
    {
        if (disguiseRoutine != null)
        {
            StopCoroutine(disguiseRoutine);
            disguiseRoutine = null;
        }
        disguiseHub.SetActive(false);
    }

    private IEnumerator ShowDisguiseTime(int duration)
    {
        int remainingTime = duration;

        while (remainingTime >= 0)
        {
            disguiseTimeText.text = $"Ngụy trang: <color=yellow>{remainingTime}</color> giây";

            // Đợi 1 giây thực tế (không bị ảnh hưởng bởi Time.timeScale nếu muốn)
            yield return new WaitForSeconds(1f);

            remainingTime--;
        }
        CloseDisguiseHub();
    }
    public void UpdateGameTime(int time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        gameTimeText.text = $"Thời gian: {minutes:00}:{seconds:00}";
    }
    public void ShowDeadHub()
    {
        deadHub.SetActive(true);
    }
    public void HideDeadHub()
    {
        deadHub.SetActive(false);
    }
}