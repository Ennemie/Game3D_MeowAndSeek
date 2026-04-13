using Fusion;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerProperties : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(UpdateNameUI)), HideInInspector]
    public NetworkString<_16> NickName { get; set; }

    [Networked, OnChangedRender(nameof(DisplaySeeker)), HideInInspector]
    public NetworkBool isSeeker { get; set; } = false;

    [Networked, OnChangedRender(nameof(UpdateDead)), HideInInspector]
    public NetworkBool isDead { get; set;} = false;

    [Networked, OnChangedRender(nameof(UpdateHp))]
    public float Hp { get; set; }

    [Networked, OnChangedRender(nameof(UpdateMana))]
    public float Mana { get; set; }

    public GameObject catObj;
    public GameObject playerCanva;
    private CanvaController canvaController;
    public List<GameObject> disguiseProps;
    private GameObject _currentActiveProp;
    [Networked, OnChangedRender(nameof(ChangeDisguise))]
    public int disguiseIndex { get; set; } = 0;
    public ParticleSystem disguiseEffect;

    public ParticleSystem attackEffect;
    [Networked, OnChangedRender(nameof(UpdateAttackEffect))]
    public bool isAttacking { get; set; } = false;
    public BoxCollider attackBox;

    public GameObject shield;
    [Networked, OnChangedRender(nameof(UpdateShield))]
    public bool isShieldActive { get; set; } = false;

    public GameObject deadEffect;

    public TMP_Text playerNameText;
    public Slider hpBar;
    public Slider manaBar;
    private float currentMana;

    private AudioSource audioSource;
    public AudioClip meowClip;
    public AudioClip screamCatClip;
    public AudioClip iceCrackClip;
    private bool isMeowClipPlaying = false;
    public Animator animator;
    private PlayerInput playerInput;
    public enum AnimState
    {
        Idle,
        Walk,
        Run,
        Jump,
        Sound,
        Eat,
    }

    [Networked, OnChangedRender(nameof(OnAnimStateChanged))]
    public AnimState CurrentState { get; set; }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        playerInput = GetComponent<PlayerInput>();
        hpBar.value = 100;
        manaBar.value = 100;
        currentMana = manaBar.value;
        canvaController = GameObject.FindWithTag("Canva").GetComponent<CanvaController>();
    }
    public override void Spawned()
    {
        UpdateNameUI();
        if (Object.HasInputAuthority)
        {
            CurrentState = AnimState.Idle;
            Hp = hpBar.value;
            Mana = manaBar.value;
        }
    }
    private void UpdateDead()
    {
        if (isDead)
        {
            GameManager.Instance.HiderCountUpdate();
            Instantiate(deadEffect, transform.position - new Vector3(0, 0.151f, 0), Quaternion.identity);
            if(!isSeeker && Object.HasInputAuthority) canvaController.ShowDeadHub();
            gameObject.SetActive(false);
        }
    }
    private void UpdateShield()
    {
        if (shield != null)
        {
            if(isShieldActive)
            {
                shield.SetActive(true);
                StartCoroutine(DeactiveShield(5f));
            }
            else
            {
                shield.SetActive(false);
            }
        }
    }
    private IEnumerator DeactiveShield(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        isShieldActive = false;
    }
    private void UpdateAttackEffect()
    {
        if (isAttacking)
        {
            attackEffect.gameObject.SetActive(true);
            isAttacking = false;
        }
    }
    public void StartAttackProcess()
    {
        if (Object.HasInputAuthority && isSeeker)
        {
            StartCoroutine(RequestAttack());
        }
    }

    private void ChangeDisguise()
    {
        // 1. Xóa món đồ cũ nếu có
        if (_currentActiveProp != null) Destroy(_currentActiveProp);

        if(disguiseEffect != null) disguiseEffect.gameObject.SetActive(true);

        if (disguiseIndex == 0)
        {
            catObj.SetActive(true);
            playerCanva.SetActive(true);
            if (canvaController != null) canvaController.CloseDisguiseHub();
        }
        else
        {
            catObj.SetActive(false);
            playerCanva.SetActive(false);
            // 2. Sinh ra món đồ mới từ Prefab trong List
            // safety checks
            if (disguiseProps == null || disguiseProps.Count <= disguiseIndex || disguiseProps[disguiseIndex] == null)
            {
                Debug.LogWarning($"Player {Object.Id} disguiseProps missing or index {disguiseIndex} out of range");
                return;
            }

            GameObject prop = disguiseProps[disguiseIndex - 1];
            _currentActiveProp = Instantiate(prop, transform.position, transform.rotation);

            // Ensure it's active and properly parented/positioned so it's visible on the player
            _currentActiveProp.SetActive(true);
            _currentActiveProp.transform.SetParent(this.transform, worldPositionStays: false);
            _currentActiveProp.transform.localPosition = new Vector3(0, -0.151f, 0);
            _currentActiveProp.transform.localRotation = Quaternion.identity;
            // If prefab uses a different scale, preserve it; otherwise ensure it's not zero
            if (_currentActiveProp.transform.localScale == Vector3.zero) _currentActiveProp.transform.localScale = Vector3.one;

            // Make sure renderer is enabled (in case prefab was disabled)
            var renderers = _currentActiveProp.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers) r.enabled = true;

            Debug.Log($"Player {Object.Id} đổi trang phục thành {prop.name}");
        }
    }
    private void DisplaySeeker()
    {
        if (isSeeker)
        {
            playerNameText.color = Color.red;
        }
        else
        {
            playerNameText.color = Color.white;
        }
    }
    private void UpdateHp()
    {
        canvaController.UpdateHpBar(Hp);
        hpBar.value = Hp;
        if (Hp > 0 && Hp < 100) isShieldActive = true;
    }
    private void UpdateMana()
    {
        manaBar.value = Mana;
        canvaController.UpdateManaBar(Mana);
    }
    public bool UseMana(float amount)
    {
        if (Mana < amount)
        {
            return false;
        }
        if (Object.HasInputAuthority)
        {
            Mana = Mathf.Max(Mana - amount, 0);
            return true;
        }
        return false;
    }
    private void Update()
    {
        currentMana = Mana;
        Mana = Mathf.Lerp(currentMana, 100, 0.2f * Time.deltaTime);
    }
    public override void FixedUpdateNetwork()
    {
    }
    public void OnAnimStateChanged()
    {
        if (animator == null) return;
        animator.CrossFade(CurrentState.ToString(), 0.1f);
        if (CurrentState == AnimState.Sound && !isMeowClipPlaying)
        {
            StartCoroutine(PlayMeowClip());
        }
    }
    private IEnumerator PlayMeowClip()
    {
        isMeowClipPlaying = true;
        audioSource.PlayOneShot(meowClip);
        yield return new WaitForSecondsRealtime(meowClip.length);
        isMeowClipPlaying = false;
    }
    public void SetAnimationState(AnimState newState)
    {
        if (Object.HasInputAuthority && CurrentState != newState)
        {
            CurrentState = newState;
        }
    }

    public void SetMyName(string newName)
    {
        if (Object.HasInputAuthority)
        {
            RpcSetNickName(newName);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RpcSetNickName(string name)
    {
        NickName = name;
    }

    private void UpdateNameUI()
    {
        if (playerNameText != null) playerNameText.text = NickName.ToString();

        if (Object.HasInputAuthority)
        {
            playerNameText.color = Color.green;
        }
        else
        {
            playerNameText.color = Color.white;
        }
    }
    /*
    public IEnumerator RequestAttack()
    {
        Debug.Log("<color=cyan>[Attack] Bắt đầu RequestAttack...</color>");

        // 1. Kiểm tra an toàn Runner
        if (Runner == null)
        {
            Debug.LogError("[Attack] Lỗi: Runner là NULL!");
            yield break;
        }

        if (Runner.LagCompensation == null)
        {
            Debug.LogError("[Attack] Lỗi: Runner.LagCompensation là NULL! (Kiểm tra xem đã bật Lag Compensation trong NetworkProjectConfig chưa)");
            yield break;
        }

        if (!Object.HasInputAuthority || !isSeeker)
        {
            Debug.LogWarning($"[Attack] Dừng: InputAuth={Object.HasInputAuthority}, isSeeker={isSeeker}");
            yield break;
        }

        // 2. Kiểm tra attackBox
        if (attackBox == null)
        {
            Debug.LogError("[Attack] Lỗi: Chưa kéo attackBox vào Inspector!");
            yield break;
        }

        // 3. Lấy thông số vùng quét
        Vector3 worldCenter = attackBox.transform.TransformPoint(attackBox.center);
        Vector3 halfExtents = attackBox.size * 0.5f;
        Quaternion worldRotation = attackBox.transform.rotation;

        // Vẽ Debug trong Scene (Nhớ bật Gizmos để nhìn thấy)
        Debug.DrawLine(worldCenter, worldCenter + Vector3.up * 2f, Color.red, 2f);
        Debug.Log($"[Attack] Đang quét tại: {worldCenter} | LayerMask Player: {LayerMask.NameToLayer("Player")}");

        List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
        int playerLayerMask = 1 << LayerMask.NameToLayer("Player");

        // 4. QUÉT VA CHẠM
        int hitCount = Runner.LagCompensation.OverlapBox(
            worldCenter,
            halfExtents,
            worldRotation,
            Object.InputAuthority,
            hits,
            playerLayerMask
        );

        Debug.Log($"[Attack] Kết quả quét: Tìm thấy {hitCount} vật thể.");

        if (hitCount > 0)
        {
            foreach (var hit in hits)
            {
                if (hit.GameObject == null) continue;

                Debug.Log($"[Attack] Chạm vật lý vào: <color=yellow>{hit.GameObject.name}</color> | Layer: {hit.GameObject.layer}");

                // Tìm script ở Root
                var target = hit.GameObject.GetComponentInParent<PlayerProperties>();

                if (target == null)
                {
                    Debug.LogWarning($"[Attack] Không tìm thấy PlayerProperties trên {hit.GameObject.name} hoặc cha của nó!");
                    continue;
                }

                // Kiểm tra các điều kiện logic
                bool isSelf = (target == this);
                bool targetIsSeeker = target.isSeeker;
                bool targetIsDead = target.isDead;

                Debug.Log($"[Attack] Phân tích mục tiêu {target.NickName}: IsSelf={isSelf}, IsSeeker={targetIsSeeker}, IsDead={targetIsDead}");

                if (!isSelf && !targetIsDead && !targetIsSeeker)
                {
                    Debug.Log($"<color=green>[Attack] HỢP LỆ! Đang gửi damage tới: {target.NickName}</color>");
                    target.RPC_ApplyDamage(50f);
                }
                else
                {
                    Debug.Log($"<color=orange>[Attack] Bỏ qua mục tiêu {target.NickName} vì không thỏa mãn điều kiện gây sát thương.</color>");
                }
            }
        }
        else
        {
            Debug.LogWarning("[Attack] KHÔNG TÌM THẤY HITBOX NÀO. Kiểm tra lại: 1. Layer Hider có phải 'Player' không? 2. Hider đã Bake Hitbox chưa?");
        }
    }
    */
    public IEnumerator RequestAttack()
    {
        // 1. Kiểm tra an toàn cơ bản
        if (!Object.HasInputAuthority || !isSeeker) yield break;
        if (attackBox == null) yield break;

        // 2. Lấy thông số vùng quét từ attackBox luôn active ở Root
        Vector3 worldCenter = attackBox.transform.TransformPoint(attackBox.center);
        Vector3 halfExtents = attackBox.size * 0.5f;
        Quaternion worldRotation = attackBox.transform.rotation;

        // Vẽ Debug để ông check vị trí quét trong Scene
        Debug.DrawLine(worldCenter, worldCenter + Vector3.up * 2f, Color.red, 1f);

        // 3. DÙNG VẬT LÝ UNITY CHUẨN (Vì Shared Mode không hỗ trợ LagComp)
        int playerLayerMask = 1 << LayerMask.NameToLayer("Player");
        Collider[] hitColliders = Physics.OverlapBox(worldCenter, halfExtents, worldRotation, playerLayerMask);

        Debug.Log($"[SharedMode Attack] Quét thấy {hitColliders.Length} vật thể.");

        foreach (var hitCollider in hitColliders)
        {
            // Tìm script PlayerProperties ở Root của đối thủ
            var target = hitCollider.GetComponentInParent<PlayerProperties>();

            if (target != null && target != this && !target.isDead && !target.isSeeker)
            {
                Debug.Log($"[HIT] Chém trúng Hider trong Shared Mode: {target.NickName}");
                
                target.RPC_ApplyDamage(50f);
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ApplyDamage(float damage)
    {
        if (isShieldActive || isDead) return;

        RPC_PlayScream();
        disguiseIndex = 0;
        Hp -= damage;

        UpSpeedForAWhile();

        if (Hp <= 0)
        {
            Hp = 0;
            isDead = true;
        }
    }
    private void UpSpeedForAWhile()
    {
        if (Object.HasInputAuthority)
        {
            PlayerMovement movement = GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.speed *= 1.5f; // Tăng tốc độ lên 150% trong 3 giây
                StartCoroutine(ResetSpeedAfterDelay(movement, 3f));
            }
        }
    }

    private IEnumerator ResetSpeedAfterDelay(PlayerMovement movement, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (movement != null)
        {
            movement.speed /= 1.5f; // Đặt lại tốc độ về bình thường
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayScream()
    {
        if (audioSource != null && screamCatClip != null)
        {
            audioSource.PlayOneShot(screamCatClip);
            Debug.Log("[RPC_PlayScream] Đã phát âm thanh Scream Cat!");
        }
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_PlayIceCrack()
    {
        if (audioSource != null && iceCrackClip != null)
        {
            audioSource.PlayOneShot(iceCrackClip);
            Debug.Log("[RPC_PlayIceCrack] Đã phát âm thanh Ice Crack!");
        }
    }
}