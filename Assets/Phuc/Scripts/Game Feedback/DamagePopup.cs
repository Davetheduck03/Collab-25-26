using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;

    private const float DISAPPEAR_TIMER_MAX = 1f;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(float damageAmount)
    {
        textMesh.text = damageAmount.ToString("F0"); // "F0" for no decimals
        textColor = textMesh.color;
        disappearTimer = DISAPPEAR_TIMER_MAX;

        // Move up and slightly random horizontal direction
        moveVector = new Vector3(Random.Range(-1f, 1f), 3f) * 2f; // Adjust speed here
    }

    private void Update()
    {
        // Move the text
        transform.position += moveVector * Time.deltaTime;

        // Slow down the movement over time
        moveVector -= moveVector * 8f * Time.deltaTime;

        // Disappear logic
        if (disappearTimer > DISAPPEAR_TIMER_MAX * 0.5f)
        {
            // First half of lifetime: Scaling up/down pop effect (Optional)
            float increaseScaleAmount = 1f;
            transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;
        }
        else
        {
            // Second half: Fade out
            transform.localScale -= Vector3.one * 1f * Time.deltaTime;
        }

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            // Start fading alpha
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