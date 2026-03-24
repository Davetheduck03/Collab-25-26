using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI objectiveLabel;
    public TextMeshProUGUI progressLabel;
    public Slider progressBar;
    public GameObject checkmark;

    public void Refresh(ObjectiveProgress progress)
    {
        MissionObjective obj = progress.objective;

        string targetName = obj.GetTargetID();
        string typeLabel = obj.missionType.ToString();
        objectiveLabel.text = $"{typeLabel}: {targetName}";

        int current = progress.currentAmount;
        int target = obj.targetAmount;
        progressLabel.text = $"{current} / {target}";

        progressBar.value = Mathf.Clamp01((float)current / target);

        checkmark.SetActive(progress.isComplete);
    }
}
