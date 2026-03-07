using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Fish : BaseUnit
{
    private SpriteRenderer _spriteRenderer;
    private bool isCaught = false;
    private bool expectingLeft = false;
    private bool expectingRight = false;
    private bool expectingParry = false;

    private bool isResolvingAttack = false;
    private bool isWaitingForInput = false;
    private bool isCooldown = false;

    private float maxHP;
    private float nextThreshold;

    // NEW: Keep track of the active Q/E prompt so we can delete it
    private GameObject currentPrompt;

    public static event Action<float> OnFishHealthThresholdReached;

    public GameObject m_Boat;

    [Header("Components")]
    [SerializeField] private DamageComponent damageComponent;
    public HealthComponent healthComponent;
    public MovementComponent movementComponent;

    [Header("UI System")]
    public Parry parryMinigame;

    [Header("Damage Feedback")]
    public Color damageColor = Color.red;
    private Color originalColor = Color.white;

    private Coroutine fightCoroutine;

    private void OnEnable()
    {
        CastLineControl.OnFishCaught += GotCaught;
        CastLineControl.OnPlayerAttackLeft += PlayerPressedLeft;
        CastLineControl.OnPlayerAttackRight += PlayerPressedRight;
        CastLineControl.OnPlayerParry += PlayerPressedParry;
    }

    private void OnDisable()
    {
        CastLineControl.OnFishCaught -= GotCaught;
        CastLineControl.OnPlayerAttackLeft -= PlayerPressedLeft;
        CastLineControl.OnPlayerAttackRight -= PlayerPressedRight;
        CastLineControl.OnPlayerParry -= PlayerPressedParry;
        isCaught = false;
    }

    public void Initialize()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null)
        {
            originalColor = _spriteRenderer.color;
        }

        ApplyScriptableData();
        GetComponents(components);

        m_Boat = GameObject.FindWithTag("Boat");
        if (m_Boat == null)
            m_Boat = GameObject.Find("Boat");

        foreach (var comp in components)
        {
            comp.Setup(this, unitData);
        }
    }

    private void ApplyScriptableData()
    {
        if (unitData == null) return;
        name = unitData.UnitName;
        _spriteRenderer.sprite = unitData.inGameSprite;
    }

    private void GotCaught(GameObject caughtObject)
    {
        if (caughtObject != gameObject) return;

        isCaught = true;
        maxHP = healthComponent.currentHealth;
        nextThreshold = maxHP * 0.8f;
        fightCoroutine = StartCoroutine(FightPattern());
    }

    private IEnumerator FightPattern()
    {
        while (isCaught && healthComponent.currentHealth > 0)
        {
            yield return new WaitUntil(() => !isResolvingAttack && !isCooldown);

            int action = Random.Range(0, 3);
            ResetExpectations();

            switch (action)
            {
                case 0:
                    OnTurnRight();
                    isWaitingForInput = true;
                    break;
                case 1:
                    OnTurnLeft();
                    isWaitingForInput = true;
                    break;
                case 2:
                    OnAttack();
                    break;
            }

            if (action == 0 || action == 1)
            {
                yield return new WaitUntil(() => !isWaitingForInput);
            }
        }
    }

    private void OnTurnRight()
    {
        if (!isCaught) return;

        expectingRight = true;

        transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        if (DamagePopupManager.Instance != null)
        {
            // NEW: Passed in 0.5f to make the letter smaller, and saved it to currentPrompt
            currentPrompt = DamagePopupManager.Instance.ShowPrompt("E", transform.position, Color.yellow, 2.5f, 0.5f);
        }

        Debug.Log("Fish Turn Right! Player must ATTACK RIGHT (E)!");
    }

    private void OnTurnLeft()
    {
        if (!isCaught) return;

        expectingLeft = true;

        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        if (DamagePopupManager.Instance != null)
        {
            // NEW: Passed in 0.5f to make the letter smaller, and saved it to currentPrompt
            currentPrompt = DamagePopupManager.Instance.ShowPrompt("Q", transform.position, Color.cyan, 2.5f, 0.5f);
        }

        Debug.Log("Fish Turn Left! Player must ATTACK LEFT (Q)!");
    }

    private void OnAttack()
    {
        if (!isCaught) return;

        expectingParry = true;
        isResolvingAttack = true;

        float speed = GetSpeedFromRarity();
        float duration = 2.5f;

        parryMinigame.BeginParry(speed, duration, OnParryFinished);
    }

    private void OnParryFinished(bool isSuccess)
    {
        if (isSuccess)
        {
            StartCoroutine(BlinkRoutine());
        }
        else
        {
            damageComponent.TryDealDamage(m_Boat);
        }

        ResetExpectations();
        StartCoroutine(ResumeFightAfterDelay(1f));
    }

    private IEnumerator ResumeFightAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isResolvingAttack = false;
    }

    private IEnumerator InputCooldownRoutine()
    {
        isCooldown = true;
        float randomCooldown = Random.Range(0.5f, 1.0f);
        yield return new WaitForSeconds(randomCooldown);
        isCooldown = false;
    }

    private IEnumerator BlinkRoutine()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(0.1f);
            _spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
            _spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(0.1f);
            _spriteRenderer.color = originalColor;
        }
    }

    private float GetSpeedFromRarity()
    {
        if (unitData is EnemySO enemyData)
        {
            switch (enemyData.rarity)
            {
                case Rarity.Common: return 200f;
                case Rarity.Uncommon: return 250f;
                case Rarity.Rare: return 300f;
                case Rarity.Epic: return 350f;
                case Rarity.Mythic: return 450f;
            }
        }
        return 200f;
    }

    private void ResetExpectations()
    {
        expectingLeft = false;
        expectingRight = false;
        expectingParry = false;

        // NEW: Clean up any old prompts hanging around when expectations reset
        if (currentPrompt != null)
        {
            Destroy(currentPrompt);
            currentPrompt = null;
        }
    }

    private void CheckForHealthThreshold()
    {
        if (healthComponent.currentHealth <= nextThreshold)
        {
            float normalizedPercent = healthComponent.currentHealth / maxHP;
            OnFishHealthThresholdReached?.Invoke(normalizedPercent);
            nextThreshold -= maxHP * 0.2f;
        }

        if (healthComponent.currentHealth <= 0)
        {
            StopCoroutine(fightCoroutine);
            OnFishDefeated();
        }
    }

    private void OnFishDefeated()
    {
        if (unitData is EnemySO enemySO && enemySO.itemData != null)
        {
            // InventoryController.Instance.AddItem(enemySO.itemData, 1, enemySO.GeneratePrice());
        }
    }

    private void PlayerPressedLeft(DamageComponent damageComponent)
    {
        if (!isCaught || isCooldown || isResolvingAttack) return;

        if (isWaitingForInput)
        {
            isWaitingForInput = false;

            // NEW: Instantly destroy the floating text letter when the player acts!
            if (currentPrompt != null)
            {
                Destroy(currentPrompt);
                currentPrompt = null;
            }

            if (expectingLeft)
            {
                damageComponent.TryDealDamage(this.gameObject);
                StartCoroutine(BlinkRoutine());
                CheckForHealthThreshold();
            }
            else
            {
                this.damageComponent.TryDealDamage(m_Boat);
            }

            ResetExpectations();
            StartCoroutine(InputCooldownRoutine());
        }
    }

    private void PlayerPressedRight(DamageComponent damageComponent)
    {
        if (!isCaught || isCooldown || isResolvingAttack) return;

        if (isWaitingForInput)
        {
            isWaitingForInput = false;

            // NEW: Instantly destroy the floating text letter when the player acts!
            if (currentPrompt != null)
            {
                Destroy(currentPrompt);
                currentPrompt = null;
            }

            if (expectingRight)
            {
                damageComponent.TryDealDamage(this.gameObject);
                StartCoroutine(BlinkRoutine());
                CheckForHealthThreshold();
            }
            else
            {
                this.damageComponent.TryDealDamage(m_Boat);
            }

            ResetExpectations();
            StartCoroutine(InputCooldownRoutine());
        }
    }

    private void PlayerPressedParry()
    {
        if (!isCaught) return;

        if (expectingParry)
            parryMinigame.CheckResult();
    }
}