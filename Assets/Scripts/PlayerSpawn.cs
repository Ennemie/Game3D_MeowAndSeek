using Fusion;
using System.Collections;
using System.Linq;
using UnityEngine;

public class PlayerSpawn : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    public CanvaController canva;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            StartCoroutine(WaitToSpawn(player));
        }
    }
    IEnumerator WaitToSpawn(PlayerRef player)
    {
        // đợi một frame để Runner.ActivePlayers được cập nhật
        yield return null;
        Vector3 spawnPos = GetSpawnPoint(player).position;
        Quaternion spawnRot = GetSpawnPoint(player).rotation;

        Runner.Spawn(
            playerPrefab,
            spawnPos,
            spawnRot,
            player,
            (runner, obj) =>
            {
                var setup = obj.GetComponent<PlayerSetup>();

                if (setup != null)
                {
                    setup.SetupCamera(obj.transform);
                }
                else
                {
                    Debug.LogError("PlayerSetup not found on prefab");
                }
            });

        canva.EnterGame();
    }
    private Transform GetSpawnPoint(PlayerRef player)
    {
        var players = Runner.ActivePlayers.ToList();
        // đảm bảo mọi client có cùng thứ tự
        players.Sort((a, b) => a.RawEncoded.CompareTo(b.RawEncoded));

        int index = players.IndexOf(player);
        if (index < 0)
        {
            Debug.LogError("Player not found in ActivePlayers");
            index = 0;
        }

        index = Mathf.Clamp(index, 0, spawnPoints.Length - 1);

        return spawnPoints[index];
    }
}