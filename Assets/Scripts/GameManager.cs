using UnityEngine;
using Fusion;
using System.Collections.Generic;
using Unity.Cinemachine;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject playerPrefab;
    private List<PlayerRef> spawnedPlayers = new List<PlayerRef>();

    [Header("Player Name")]


    [Header("Cameras")]
    public CinemachineCamera openingCam;
    public CinemachineCamera playerCam;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        playerPrefab.GetComponent<PlayerMovement>().isMovementEnabled = true; ////XXXX
    }
    public override void Spawned()
    {
        // Trong Shared Mode, chúng ta kiểm tra nếu người chơi vừa tham gia
        // chính là bản thân mình (Local Player) thì tiến hành Spawn
        if (Runner.IsSharedModeMasterClient)
        {
            Debug.Log("GameManager khởi tạo bởi Master Client");
        }

        // Tự động Spawn nhân vật cho chính Client này
        AddSpawnedPlayer(Runner.LocalPlayer);
    }
    private void AddSpawnedPlayer(PlayerRef player)
    {
        spawnedPlayers.Add(player);
    }
    public void DisableOpeningCam()
    {
        openingCam.gameObject.SetActive(false);
    }
}