using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Fish : BaseUnit
{
    private SpriteRenderer _spriteRenderer;
    private bool isCaught = false;
    private bool facingRight = false;
    private bool facingLeft = false;
    private bool expectingLeft = false;
    private bool expectingRight = false;
    private bool expectingParry = false;

    private bool isResolvingAttack = false;

    private float maxHP;
    private float nextThreshold;

    public static event Action<float> OnFishHealthThresholdReached;

    [Header("Components")]
    [SerializeField] private GameObject m_Boat;
    [SerializeField] private DamageComponent damageComponent;
    public HealthComponent healthComponent;
    public MovementComponent movementComponent;

    // --- NEW: Reference to the Minigame Script ---
    [Header("UI System")]
    public Parry parryMinigame;

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
    }

    public void Initialize()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        ApplyScriptableData();
        GetComponents(components);
        m_Boat = GameObject.Find("Boat");

        foreach (var comp in components)
        {
            comp.Setup(this, unitData);
        }
    }

    private void ApplyScriptableData()
    {
        if (unitData == null)
        {
            Debug.LogWarning($"[Fish] No UnitSO assigned on {name}");
            return;
        }

        name = unitData.UnitName;
        _spriteRenderer.sprite = unitData.inGameSprite;
    }

    private void GotCaught()
    {
        isCaught = true;
        maxHP = healthComponent.currentHealth;
        nextThreshold = maxHP * 0.8f;
        fightCoroutine = StartCoroutine(FightPattern());
    }

    private IEnumerator FightPattern()
    {
        while (isCaught && healthComponent.currentHealth > 0)
        {
            yield return new WaitUntil(() => !isResolvingAttack);

            int action = Random.Range(0, 3);
            ResetExpectations();

            float waitTime = Random.Range(1f, 3f);

            switch (action)
            {
                case 0:
                    OnTurnRight();
                    break;
                case 1:
                    OnTurnLeft();
                    break;
                case 2:
                    OnAttack();
                    break;
            }

            // Only wait the standard delay if we aren't locked in an attack resolution
            if (!isResolvingAttack)
            {
                yield return new WaitForSeconds(waitTime);
            }
        }
    }
    private void OnTurnRight()
    {
        if (!isCaught) return;

        expectingRight = true;
        Debug.Log("Fish Turn Right! Player must ATTACK RIGHT!");
    }

    private void OnTurnLeft()
    {
        if (!isCaught) return;

        expectingLeft = true;
        Debug.Log("Fish Turn Left! Player must ATTACK LEFT!");
    }

    private void OnAttack()
    {
        if (!isCaught) return;

        expectingParry = true;
        Debug.Log("Fish Attacks! Player must PARRY!");

        float speed = GetSpeedFromRarity();

        // 2. Determine duration (give less time for harder rarities?)
        float duration = 2.5f; // Fixed duration for now, or make it variable

        // 3. Start Minigame with a callback
        parryMinigame.BeginParry(speed, duration, OnParryFinished);
    }

    private void OnParryFinished(bool isSuccess)
    {
        if (isSuccess)
        {
            Debug.Log("Player successfully parried the fish!");
            // Optional: Deal small "Counter" damage to fish?
            // damageComponent.TryDealDamage(this.gameObject); 
        }
        else
        {
            Debug.Log("Parry failed! Boat takes damage.");
            damageComponent.TryDealDamage(m_Boat);
        }

        ResetExpectations();

        // Add a small delay before resuming the fight loop so it's not instant
        StartCoroutine(ResumeFightAfterDelay(1f));
    }

    private IEnumerator ResumeFightAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isResolvingAttack = false; // RESUME the fight loop
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
                case Rarity.Legendary: return 450f;
                case Rarity.Mythic: return 600f;
            }
        }
        return 200f; // Default
    }

    private void ResetExpectations()
    {
        expectingLeft = false;
        expectingRight = false;
        expectingParry = false;

    }

    // ... [Rest of your Health Code stays the same] ...
    private void CheckForHealthThreshold()
    {
        float hpPercent = healthComponent.currentHealth / maxHP;

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
            InventoryController.Instance.AddItem(enemySO.itemData, 1, enemySO.GeneratePrice());
        }
    }

    private void PlayerPressedLeft(DamageComponent damageComponent)
    {
        if (expectingLeft)
        {
            Debug.Log("Correct! Player hits the fish!");
            damageComponent.TryDealDamage(this.gameObject);
            CheckForHealthThreshold();
        }
        else
        {
            Debug.Log("Wrong input! Player takes damage!");
            this.damageComponent.TryDealDamage(m_Boat);
        }
        ResetExpectations();
    }

    private void PlayerPressedRight(DamageComponent damageComponent)
    {
        if (expectingRight)
        {
            Debug.Log("Correct! Player hits the fish!");
            damageComponent.TryDealDamage(this.gameObject);
            CheckForHealthThreshold();
        }
        else
        {
            Debug.Log("Wrong input! Player takes damage!");
            this.damageComponent.TryDealDamage(m_Boat);
        }
        ResetExpectations();
    }

    // --- UPDATED: Parry Logic ---
    private void PlayerPressedParry()
    {
        if (expectingParry)
        parryMinigame.CheckResult();
    }
}