using TMPro;
using UnityEngine;

/// <summary>
/// Attach to a TMP text object in your HUD.
/// Displays the current zone name and updates whenever the zone changes.
/// </summary>
public class ZoneNameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text zoneText;

    private ZoneSO _lastZone;

    private void Start()
    {
        if (zoneText == null)
            zoneText = GetComponent<TMP_Text>();

        UpdateDisplay();
    }

    private void Update()
    {
        if (ZoneManager.Instance == null) return;

        // Only update the text when the zone actually changes
        if (ZoneManager.Instance.CurrentZone != _lastZone)
        {
            _lastZone = ZoneManager.Instance.CurrentZone;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (zoneText == null) return;

        zoneText.text = ZoneManager.Instance?.CurrentZone != null
            ? ZoneManager.Instance.CurrentZone.zoneName
            : "Unknown Zone";
    }
}
