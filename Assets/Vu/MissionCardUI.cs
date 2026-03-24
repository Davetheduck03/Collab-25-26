using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionCardUI : MonoBehaviour
{
    [Header("Card Header")]
    public TextMeshProUGUI missionNameLabel;
    public TextMeshProUGUI goldRewardLabel;
    public GameObject readyBanner;

    [Header("Objectives")]
    public ObjectiveUI objectiveRowPrefab;
    public Transform objectivesContainer;

    private List<ObjectiveUI> _rowPool = new List<ObjectiveUI>();
    private ActiveMission _mission;

    public void Initialise(ActiveMission mission)
    {
        _mission = mission;
        missionNameLabel.text = mission.missionData.missionName;
        goldRewardLabel.text = $"Reward: {mission.missionData.goldReward} Gold";
        EnsureRowCount(mission.progressList.Count);
    }

    public void Refresh()
    {
        if (_mission == null) return;

        EnsureRowCount(_mission.progressList.Count);

        for (int i = 0; i < _mission.progressList.Count; i++)
        {
            _rowPool[i].Refresh(_mission.progressList[i]);
        }

        if (readyBanner != null)
            readyBanner.SetActive(_mission.isReadyToTurnIn);
    }

    private void EnsureRowCount(int needed)
    {
        while (_rowPool.Count < needed)
        {
            ObjectiveUI row = Instantiate(objectiveRowPrefab, objectivesContainer);
            _rowPool.Add(row);
        }

        for (int i = 0; i < _rowPool.Count; i++)
            _rowPool[i].gameObject.SetActive(i < needed);
    }
}