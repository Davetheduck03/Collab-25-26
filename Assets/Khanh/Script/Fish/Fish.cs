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
            int action = Random.Range(0, 3);

            // Important: Reset previous states/UI before starting new action
            ResetExpectations();

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
            float waitTime = Random.Range(1f, 3f);
            yield return new WaitForSeconds(waitTime);
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

        ResetExpectations(); // This also closes the UI via StopMinigame()
    }
}