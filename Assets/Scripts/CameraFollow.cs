using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    
    private CinemachineCamera cinemachineCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
        if(cinemachineCamera ==null)
        {
            Debug.LogError("CinemachineCamera component not found on " + gameObject.name);
        }

    }
    public void AssignCamera(Transform target)
    {
        if(cinemachineCamera!=null)
        {
            cinemachineCamera.Follow = target;
            cinemachineCamera.LookAt = target;
        }
    }
}
