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



    [SerializeField] private GameObject m_Boat;

    private Coroutine fightCoroutine;

    [SerializeField] private DamageComponent damageComponent;
    public HealthComponent healthComponent;
    public MovementComponent movementComponent;

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

        if (unitData is EnemySO enemySO)
        {
            Debug.Log($"{name} price: {enemySO.price} | rarity: {enemySO.rarity}");
        }
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

    // The fish will turn right and only take damage when player attacks right, attack player when player attacks left
    private void OnTurnRight()
    {
        if (!isCaught) return;

        expectingRight = true;
        expectingLeft = false;
        expectingParry = false;

        Debug.Log("Fish Turn Right! Player must ATTACK RIGHT!");
    }

    // The fish will turn left and only take damage when player attacks left, attack player when player attacks right
    private void OnTurnLeft()
    {
        if (!isCaught) return;

        expectingLeft = true;
        expectingRight = false;
        expectingParry = false;

        Debug.Log("Fish Turn Left! Player must ATTACK LEFT!");
    }

    // The fish will attack the player and player have to parry to avoid damage
    private void OnAttack()
    {
        if (!isCaught) return;

        expectingParry = true;
        expectingLeft = false;
        expectingRight = false;

        Debug.Log("Fish Attacks! Player must PARRY!");
    }

    private void ResetExpectations()
    {
        expectingLeft = false;
        expectingRight = false;
        expectingParry = false;
    }

    private void CheckForHealthThreshold()
    {
        float hpPercent = healthComponent.currentHealth / maxHP;

        if (healthComponent.currentHealth <= nextThreshold)
        {
            float normalizedPercent = healthComponent.currentHealth / maxHP; // 0.8, 0.6, 0.4, 0.2, 0.0
            OnFishHealthThresholdReached?.Invoke(normalizedPercent);

            nextThreshold -= maxHP * 0.2f;
        }

        if (healthComponent.currentHealth <= 0)
        {
            StopCoroutine(fightCoroutine);
            Debug.Log("Fish defeated!");
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

    private void PlayerPressedParry()
    {
        if (expectingParry)
        {
            Debug.Log("Perfect Parry!");
        }
        else
        {
            Debug.Log("Failed Parry! Player takes damage!");
            damageComponent.TryDealDamage(m_Boat);
        }

        ResetExpectations();
    }

}
