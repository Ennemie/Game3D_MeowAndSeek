using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayHubController : MonoBehaviour
{
    [SerializeField] private Button chatHubBtn;
    [SerializeField] private GameObject chatHubContainer;
    [SerializeField] private Button closeBtn;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        chatHubBtn.gameObject.SetActive(true);
        chatHubContainer.SetActive(false);
    }

    public void OpenChatHub()
    {
        chatHubBtn.gameObject.SetActive(false);
        chatHubContainer.SetActive(true);
    }
    public void CloseChatHub()
        {
            chatHubBtn.gameObject.SetActive(true);
            chatHubContainer.SetActive(false);
    }
}
