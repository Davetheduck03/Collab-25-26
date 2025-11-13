using System.Collections;
using UnityEngine;

public class Fish : BaseUnit
{
    private SpriteRenderer _spriteRenderer;
    private bool isCaught = false;
    private bool facingRight = false;
    private bool facingLeft = false;

    private Coroutine fightCoroutine;

    private void OnEnable()
    {
        CastLineControl.OnFishCaught += GotCaught;
    }
    private void OnDisable()
    {
        CastLineControl.OnFishCaught -= GotCaught;
    }


    public void Initialize()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        ApplyScriptableData();
        GetComponents(components);

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
        fightCoroutine = StartCoroutine(FightPattern());

    }

    private IEnumerator FightPattern()
    {
        while (isCaught && unitData.health > 0)
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
        if(!isCaught) return;
        Debug.Log("Fish Attack Right!");
    }
    // The fish will turn left and only take damage when player attacks left, attack player when player attacks right
    private void OnTurnLeft()
    {
        if(!isCaught) return;
        Debug.Log("Fish Attack Left!");
    }
    // The fish will attack the player and player have to parry to avoid damage
    private void OnAttack()
    {
        if(!isCaught) return;
        Debug.Log("Fish Parry!");
    }

}
