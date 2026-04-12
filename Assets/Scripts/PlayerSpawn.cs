using Fusion;
using System.Collections;
using System.Linq;
using UnityEngine;

public class PlayerSpawn : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private NetworkObject gameManagerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    public CanvaController canva;

    public void PlayerJoined(PlayerRef player)
    {
        if (Runner.IsSharedModeMasterClient && GameManager.Instance == null)
        {
            Runner.Spawn(gameManagerPrefab, Vector3.zero, Quaternion.identity);
        }

        if (player == Runner.LocalPlayer)
        {
            StartCoroutine(DoSpawn(player));
        }
    }

    public override void Render()
    {
        if (GameManager.Instance != null && !GameManager.Instance.Object.IsValid)
        {
        }
    }

    IEnumerator DoSpawn(PlayerRef player)
    {
        NetworkObject playerObj = Runner.Spawn(
            playerPrefab,
            spawnPoints[0].position,
            spawnPoints[0].rotation,
            player,
            (runner, obj) =>
            {
                runner.SetPlayerObject(player, obj);

                var setup = obj.GetComponent<PlayerSetup>();
                if (setup != null) setup.SetupCamera(obj.transform);
            });

        yield return null;

        Transform correctPoint = GetSpawnPoint(player);
        TeleportPlayer(playerObj.gameObject, correctPoint);

        if (canva != null) canva.EnterGame();
    }

    private void TeleportPlayer(GameObject playerObj, Transform target)
    {
        var cc = playerObj.GetComponent<CharacterController>();

        if (cc != null) cc.enabled = false;

        playerObj.transform.position = target.position;
        playerObj.transform.rotation = target.rotation;

        if (cc != null) cc.enabled = true;
    }

    private Transform GetSpawnPoint(PlayerRef player)
    {
        var playersList = Runner.ActivePlayers.ToList();

        playersList.Sort((a, b) => a.RawEncoded.CompareTo(b.RawEncoded));

        int index = playersList.IndexOf(player);
        if (index < 0) index = 0;

        index = Mathf.Clamp(index, 0, spawnPoints.Length - 1);
        return spawnPoints[index];
    }
}