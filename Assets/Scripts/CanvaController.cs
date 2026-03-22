using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvaController : MonoBehaviour
{
    private string playerName;

    [Header("Name hub")]
    public GameObject nameHub;
    public TMP_Text nameHubText;
    public TMP_InputField nameInputField;
    public Button enterRoomBtn;

    [Header("Ready hub")]
    public GameObject readyHub;
    public Button readyBtn;

    [Header("ReadiedHub")]
    public GameObject readiedHub;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        nameHub.SetActive(false);
        readyHub.SetActive(false);
        readiedHub.SetActive(false);
    }
    public void EnterGame()
    {
        nameHub.SetActive(true);
    }
    public void EnterRoom()
    {
        playerName = nameInputField.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            nameHubText.text = "Vui lòng nhập tên của bạn!";
        }

        nameHub.SetActive(false);
        readyHub.SetActive(true);
        DisplayName();
    }
    public void ReadiedHub()
    {
        readyHub.SetActive(false);
        readiedHub.SetActive(true);
        GameManager.Instance.DisableOpeningCam();
    }
    private void DisplayName()
    {
        string nameInput = nameInputField.text;
        if (string.IsNullOrEmpty(nameInput)) return;

        // 1. Tìm tất cả các PlayerProperties trong game
        PlayerProperties[] allPlayers = FindObjectsByType<PlayerProperties>(FindObjectsSortMode.None);

        foreach (var player in allPlayers)
        {
            if (player.Object.HasInputAuthority)
            {
                player.SetMyName(nameInput);
                break;
            }
        }
    }
}
