using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectPanel : MonoBehaviour
{
    [SerializeField] private List<Image> effectImageList = new List<Image>(); // UI effect slots
    private Dictionary<EffectKey, Sprite> effectSprites = new Dictionary<EffectKey, Sprite>();

    [SerializeField] private Sprite buff_Damage;
    [SerializeField] private Sprite buff_CritChance;
    [SerializeField] private Sprite buff_Resistance;
    [SerializeField] private Sprite debuff_Damage;
    [SerializeField] private Sprite debuff_CritChance;
    [SerializeField] private Sprite debuff_Resistance;
    [SerializeField] private Sprite debuff_Decay;

    private void Awake()
    {
        // Initialize dictionary with sprites
        effectSprites = new Dictionary<EffectKey, Sprite>
        {
            { EffectKey.Buff_Damage, buff_Damage },
            { EffectKey.Buff_CritChance, buff_CritChance },
            { EffectKey.Buff_Resistance, buff_Resistance },
            { EffectKey.Debuff_Damage, debuff_Damage },
            { EffectKey.Debuff_CritChance, debuff_CritChance },
            { EffectKey.Debuff_Resistance, debuff_Resistance },
            { EffectKey.Debuff_Decay, debuff_Decay }
        };

        ToggleOffAllEffect();
    }

    void ToggleOffAllEffect()
    {
        foreach (Image image in effectImageList)
            image.gameObject.SetActive(false);
    }

    public void ToggleOnEffect(EffectKey effectKey)
    {
        if (effectSprites.TryGetValue(effectKey, out Sprite effectSprite))
        {
            foreach (Image image in effectImageList)
            {
                //Avoid applying the same effect
                if(image.gameObject.activeInHierarchy && image.sprite == effectSprite)
                    break;
                    
                if (!image.gameObject.activeInHierarchy) // Find an available slot
                {
                    image.sprite = effectSprite;
                    image.gameObject.SetActive(true);
                    return;
                }
            }
        }
    }

    public void ToggleOffEffect(EffectKey effectKey)
    {
        foreach (Image image in effectImageList)
        {
            if (image.gameObject.activeInHierarchy && image.sprite == effectSprites[effectKey])
            {
                image.gameObject.SetActive(false);
                return;
            }
        }
    }
}

public enum EffectKey
{
    Buff_Damage,
    Buff_CritChance,
    Buff_Resistance,
    Debuff_Damage,
    Debuff_CritChance,
    Debuff_Resistance,
    Debuff_Decay
}
