using Fusion;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerProperties : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(UpdateNameUI))]
    public NetworkString<_16> NickName { get; set; }
    [Networked, OnChangedRender(nameof(UpdateHp))]
    private float Hp { get; set; }

    [Networked, OnChangedRender(nameof(UpdateMana))]
    private float Mana { get; set; }

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

    }
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            CurrentState = AnimState.Idle;
            Hp = hpBar.value;
            Mana = manaBar.value;
        }
    }
    private void UpdateHp()
    {
        hpBar.value = Hp;
    }
    private void UpdateMana()
    {
        manaBar.value = Mana;
    }
    public bool UseMana(float amount)
    {
        if(Mana < amount)
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
        bool addHpPressed = playerInput.actions["AddHp"].IsPressed();
        bool minusHpPressed = playerInput.actions["MinusHp"].IsPressed();

        UpdateHpAndMana(addHpPressed, minusHpPressed);
    }
    private void UpdateHpAndMana(bool addHp, bool minusHp)
    {
        if (addHp) Hp = Mathf.Min(Hp + 1, hpBar.maxValue);
        if (minusHp) Hp = Mathf.Max(Hp - 1, 0);
    }
    public void OnAnimStateChanged()
    {
        if (animator == null) return;
        animator.CrossFade(CurrentState.ToString(), 0.1f);
        if(CurrentState == AnimState.Sound && !ismoewClipPlaying)
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
    }
}