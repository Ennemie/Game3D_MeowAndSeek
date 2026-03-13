using Fusion;
using UnityEngine;


public class PlayerSpawn : SimulationBehaviour, IPlayerJoined
{
    [SerializeField]
    private GameObject _playerPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
            Runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, player);
    
    }
}
