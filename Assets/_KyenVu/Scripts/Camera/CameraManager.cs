using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("The boat transform to follow normally.")]
    public Transform boatTarget;
    [Tooltip("The hook/triangle transform to follow while fishing.")]
    public Transform hookTarget;

    [Header("Follow Settings")]
    [Tooltip("How smoothly the camera follows its target. Lower = snappier.")]
    public float smoothTime = 0.2f;
    [Tooltip("Offset from the target (e.g. slightly above the boat).")]
    public Vector3 offset = new Vector3(0f, 2f, -10f);

    private Transform _currentTarget;
    private Vector3 _velocity = Vector3.zero;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        BoatController.OnFishingStarted  += HandleFishingStarted;
        CastLineControl.OnFishingFinished += HandleFishingFinished;
    }

    private void OnDisable()
    {
        BoatController.OnFishingStarted  -= HandleFishingStarted;
        CastLineControl.OnFishingFinished -= HandleFishingFinished;
    }

    private void Start()
    {
        if (boatTarget == null)
        {
            var boat = GameObject.FindWithTag("Boat") ?? GameObject.Find("Boat");
            if (boat != null) boatTarget = boat.transform;
        }

        _currentTarget = boatTarget;
    }

    private void LateUpdate()
    {
        if (_currentTarget == null) return;

        Vector3 targetPos = _currentTarget.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, smoothTime);
    }

    // ── Event Handlers ────────────────────────────────────────────────────────

    private void HandleFishingStarted()
    {
        if (hookTarget != null)
        {
            _currentTarget = hookTarget;
            Debug.Log("[CameraManager] Switched to hook target.");
        }
        else
        {
            Debug.LogWarning("[CameraManager] hookTarget not assigned — staying on boat.");
        }
    }

    private void HandleFishingFinished()
    {
        _currentTarget = boatTarget;
        Debug.Log("[CameraManager] Switched back to boat target.");
    }
}
