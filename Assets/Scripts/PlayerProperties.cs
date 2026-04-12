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

    public GameObject shield;
    [Networked, OnChangedRender(nameof(UpdateShield))]
    public bool isShieldActive { get; set; } = false;

    public GameObject deadEffect;

    public TMP_Text playerNameText;
    public Slider hpBar;
    public Slider manaBar;
    private float currentMana;

    private AudioSource audioSource;
    public AudioClip moewClip;
    private bool ismoewClipPlaying = false;
    public Animator animator;
    private PlayerInput playerInput;
    public enum AnimState
    {
        Idle,
        Walk,
        Run,
        Jump,
        Sound,
        Eat
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
        if (CurrentState == AnimState.Sound && !ismoewClipPlaying)
        {
            StartCoroutine(PlayMeowClip());
        }
    }
    private IEnumerator PlayMeowClip()
    {
        ismoewClipPlaying = true;
        audioSource.PlayOneShot(moewClip);
        yield return new WaitForSecondsRealtime(moewClip.length);
        ismoewClipPlaying = false;
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
    private void OnTriggerEnter(Collider other)
    {
        // 1. Chạm vào bất cứ thứ gì có tag AttackEffect
        if (other.CompareTag("AttackEffect"))
        {
            // 2. Nếu tôi không phải Seeker thì tôi bị mất máu
            if (!isSeeker)
            {
                // Nếu dùng Photon Fusion, nên trừ máu trên máy có quyền điều khiển (InputAuthority)
                if (Object.HasInputAuthority)
                {
                    disguiseIndex = 0;
                    if (!isShieldActive)
                    {
                        Hp -= 50f;
                        if (Hp <= 0)
                        {
                            isDead = true;
                        }
                    }
                }
            }
        }
    }
}