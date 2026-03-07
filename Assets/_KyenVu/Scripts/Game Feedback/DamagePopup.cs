using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer;
    private float disappearTimerMax = 1f;
    private Color textColor;
    private Vector3 moveVector;

    // NEW: Keeps track of how big the text should be
    private float baseScale = 1f;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(float damageAmount)
    {
        SetupText(damageAmount.ToString("F0"), textMesh.color, 1f, 1f);
    }

    // NEW: Added a 'scaleModifier' parameter (defaults to 1f for normal damage)
    public void SetupText(string text, Color color, float lifetime, float scaleModifier = 1f)
    {
        textMesh.text = text;
        textMesh.color = color;
        textColor = color;
        disappearTimerMax = lifetime;
        disappearTimer = disappearTimerMax;

        // Set the starting size
        baseScale = scaleModifier;
        transform.localScale = Vector3.one * baseScale;

        // Move up and slightly random horizontal direction
        moveVector = new Vector3(Random.Range(-1f, 1f), 3f) * 2f;
    }

    private void Update()
    {
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 8f * Time.deltaTime;

        if (disappearTimer > disappearTimerMax * 0.5f)
        {
            // Scale up slightly based on the baseScale
            float increaseScaleAmount = baseScale;
            transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;
        }
        else
        {
            // Fade out and scale down based on the baseScale
            transform.localScale -= Vector3.one * (baseScale / (disappearTimerMax * 0.5f)) * Time.deltaTime;
        }

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float fadeSpeed = 3f;
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}