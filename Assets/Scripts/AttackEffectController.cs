using UnityEngine;

public class AttackEffectController : MonoBehaviour
{
    public Transform player;
    private PlayerProperties playerProperties;
    private Vector3 posOffset = new Vector3(0, -0.151f, 0);
    private Quaternion rotOffset = Quaternion.Euler(0, -90, 0);

    private void Awake()
    {
        playerProperties = player.GetComponent<PlayerProperties>();
    }
    private void OnEnable()
    {
        if (player != null)
        {
            transform.SetParent(null);
        }

    }
    private void OnDisable()
    {
        if (player != null)
        {
            playerProperties.isAttacking = false;
            // Gắn lại làm con của Player
            transform.SetParent(player);

            // Đưa về vị trí cũ dưới chân Player để sẵn sàng cho lần sau
            transform.localPosition = posOffset;
            transform.localRotation = rotOffset;
        }
        else
        {
            // Nếu Player đã thoát game hoặc bị xóa, thì xóa luôn hiệu ứng này
            Destroy(gameObject);
        }
    }
}
