using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One row in the Quest Tracker HUD.
/// Must be in its own file — Unity requires MonoBehaviour filename to match class name.
/// </summary>
public class QuestTrackerRow : MonoBehaviour
{
    [SerializeField] public TMP_Text objectiveText;
    [SerializeField] public TMP_Text progressText;
    [SerializeField] public Image    progressBar;

    public void Populate(ObjectiveProgress progress)
    {
        string targetID = progress.objective.GetTargetID();

        if (objectiveText != null)
            objectiveText.text = $"Catch: {targetID}";

        if (progressText != null)
            progressText.text = $"{progress.currentAmount} / {progress.objective.targetAmount}";

        if (progressBar != null)
        {
            float fill = progress.objective.targetAmount > 0
                ? (float)progress.currentAmount / progress.objective.targetAmount
                : 0f;
            progressBar.fillAmount = Mathf.Clamp01(fill);
            progressBar.color = progress.isComplete
                ? new Color(0.4f, 1f, 0.5f)      // green when done
                : new Color(1f, 0.85f, 0.3f);     // gold in progress
        }
    }
}
