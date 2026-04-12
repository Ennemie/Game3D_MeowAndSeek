using Fusion; // Cần thiết để dùng NetworkBehaviour và RPC
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Đổi từ MonoBehaviour sang NetworkBehaviour để có thể gửi dữ liệu qua mạng
public class ChatManager : NetworkBehaviour
{
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private Button sendChatBtn;
    [SerializeField] private TMP_Text chatDisplay;

    private void Start()
    {
        // Gán sự kiện khi click chuột vào nút Send
        if (sendChatBtn != null)
            sendChatBtn.onClick.AddListener(SendChatMessage);
    }

    private void Update()
    {
        // Kiểm tra nếu người dùng nhấn phím Enter trên bàn phím
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            SendChatMessage();
        }
    }

    private void SendChatMessage()
    {
        // Kiểm tra nếu ô nhập trống hoặc chỉ có khoảng trắng thì không gửi
        if (string.IsNullOrWhiteSpace(chatInputField.text)) return;

        // Gọi hàm RPC để đồng bộ tin nhắn cho tất cả mọi người trong phòng
        // Object.InputAuthority: định danh người gửi
        RPC_SendMessage(Runner.LocalPlayer, chatInputField.text);

        // Xóa nội dung trong ô nhập sau khi gửi và tập trung con trỏ lại ô đó
        chatInputField.text = "";
        chatInputField.ActivateInputField();
    }

    // RPC (Remote Procedure Call) để gửi dữ liệu từ một máy đến tất cả các máy khác
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SendMessage(PlayerRef sender, string message)
    {
        // Định dạng tin nhắn: "Player ID: Nội dung"
        string formattedMessage = $"<b>{GameManager.Instance.GetPlayerName(sender)}:</b> <i>{message}</i>\n";

        // Hiển thị lên UI
        if (chatDisplay != null)
        {
            chatDisplay.text += formattedMessage;
        }

        // Log ra Console để kiểm tra lỗi nếu có
        Debug.Log($"Chat từ {sender}: {message}");
    }
}