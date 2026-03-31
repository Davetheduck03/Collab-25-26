using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI objectiveLabel;
    public TextMeshProUGUI progressLabel;
    public Image progressBar;
    public GameObject checkmark;

    public void Refresh(ObjectiveProgress progress)
    {
        MissionObjective obj = progress.objective;

        string targetName = obj.GetTargetID();
        string typeLabel = obj.missionType.ToString();

        objectiveLabel.text = $"{typeLabel}: {targetName}";
        progressLabel.text = $"{progress.currentAmount} / {obj.targetAmount}";

        if (obj.targetAmount > 0)
        {
            progressBar.fillAmount = (float)progress.currentAmount / obj.targetAmount;
        }
        else
        {
            progressBar.fillAmount = 0f;
        }

        checkmark.SetActive(progress.isComplete);
    }
}