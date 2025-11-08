using UnityEngine;

public class Fish : BaseUnit
{
    private SpriteRenderer _spriteRenderer;

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
}
