using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIIndicatorPointer : MonoBehaviour
{
    [Header("Edge Clamping Settings")]
    public float edgePadding = 50f;

    [Header("Visibility & Scaling")]
    [Tooltip("Hide the indicator if the player is closer than this distance (in world units), even if the center point is off-screen.")]
    public float hideDistance = 8f; // <--- NEW: Distance fallback
    public float maxScaleDistance = 20f;
    public float minScale = 0.5f;
    public float maxScale = 1.0f;

    [Header("UI References")]
    public RectTransform arrowGraphic;
    public TMP_Text labelText;
    public Image arrowImage;

    private Transform player;
    private Transform target;
    private Camera mainCamera;

    private Vector3 arrowBaseScale;
    private Color baseColor;
    private Tween highlightTween;
    private bool isHighlighted = false;

    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private RectTransform canvasRect;

    public void Initialize(Transform playerRef, Transform targetRef, string name)
    {
        player = playerRef;
        target = targetRef;
        labelText.text = name;

        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();

        parentCanvas = GetComponentInParent<Canvas>();
        canvasRect = parentCanvas.GetComponent<RectTransform>();

        arrowBaseScale = arrowGraphic.localScale;
        baseColor = arrowImage.color;
    }

    void Update()
    {
        if (player == null || target == null || mainCamera == null || canvasRect == null) return;

        // --- NEW: Calculate distance to the target first ---
        float distanceToTarget = Vector3.Distance(player.position, target.position);

        // 1. Check if the target's center is visible on the screen
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(target.position);
        bool isVisibleOnScreen = viewportPos.z > 0 &&
                                 viewportPos.x >= 0 && viewportPos.x <= 1 &&
                                 viewportPos.y >= 0 && viewportPos.y <= 1;

        // --- CHANGED: Hide if it's on screen OR if the player is just really close to it ---
        if (isVisibleOnScreen || distanceToTarget <= hideDistance)
        {
            arrowGraphic.gameObject.SetActive(false);
            labelText.gameObject.SetActive(false);
            return;
        }

        // If it is far away and off-screen, ensure it is turned on
        if (!arrowGraphic.gameObject.activeSelf)
        {
            arrowGraphic.gameObject.SetActive(true);
            labelText.gameObject.SetActive(true);
        }

        // 2. Get the actual screen positions
        Vector3 targetScreenPos = mainCamera.WorldToScreenPoint(target.position);
        Vector3 playerScreenPos = mainCamera.WorldToScreenPoint(player.position);

        if (targetScreenPos.z < 0)
        {
            targetScreenPos *= -1;
        }

        // 3. Clamp the indicator to the edges of the screen!
        Vector3 clampedScreenPos = targetScreenPos;
        clampedScreenPos.x = Mathf.Clamp(clampedScreenPos.x, edgePadding, Screen.width - edgePadding);
        clampedScreenPos.y = Mathf.Clamp(clampedScreenPos.y, edgePadding, Screen.height - edgePadding);

        // 4. Translate the clamped screen pixel to the Canvas's local space
        Camera uiCamera = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            clampedScreenPos,
            uiCamera,
            out Vector2 localPos
        );

        rectTransform.localPosition = new Vector3(localPos.x, localPos.y, 0f);

        // 5. Rotate the arrow
        Vector3 screenDir = (targetScreenPos - playerScreenPos).normalized;
        float angle = Mathf.Atan2(screenDir.y, screenDir.x) * Mathf.Rad2Deg;
        arrowGraphic.rotation = Quaternion.Euler(0, 0, angle - 90f);

        // 6. Apply distance scaling
        float scalePercent = Mathf.Clamp01(distanceToTarget / maxScaleDistance);
        float dynamicScale = Mathf.Lerp(minScale, maxScale, scalePercent);

        rectTransform.localScale = Vector3.one * dynamicScale;
    }

    // ... Keep your SetHighlight and OnDestroy methods the same below!

    public void SetHighlight(bool active)
    {
        if (active == isHighlighted) return;
        isHighlighted = active;

        if (active)
        {
            arrowImage.color = Color.yellow;
            labelText.color = Color.yellow;

            highlightTween = arrowGraphic.DOScale(arrowBaseScale * 1.5f, 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            highlightTween?.Kill();
            arrowGraphic.localScale = arrowBaseScale;
            arrowImage.color = baseColor;
            labelText.color = Color.white;
        }
    }

    private void OnDestroy()
    {
        highlightTween?.Kill();
    }
}