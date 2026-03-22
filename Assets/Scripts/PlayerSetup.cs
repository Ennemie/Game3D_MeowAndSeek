using Fusion;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    public void SetupCamera(Transform playerTransform)
    {
        // đúng authority cho camera
        if (!Object.HasInputAuthority) return;

        CameraFollow cameraFollow = FindFirstObjectByType<CameraFollow>();

        if (cameraFollow != null)
        {
            cameraFollow.AssignCamera(playerTransform);
        }
        else
        {
            Debug.LogError("CameraFollow not found");
        }
    }
}