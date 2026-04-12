using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform mainCamTransform;

    void LateUpdate()
    {
        // Nếu chưa tìm thấy Cam, thử tìm lại
        if (mainCamTransform == null)
        {
            if (Camera.main != null)
                mainCamTransform = Camera.main.transform;
            return; // Thoát ra để đợi frame sau
        }

        // Thực hiện xoay
        transform.LookAt(transform.position + mainCamTransform.rotation * Vector3.forward,
                         mainCamTransform.rotation * Vector3.up);
    }
}