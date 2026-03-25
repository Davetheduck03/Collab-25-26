using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MissionPanelUI : MonoBehaviour
{
    public ObjectiveUI objectiveRowPrefab;
    public Transform rowsContainer;
    public GameObject emptyStateLabel;

    private Dictionary<ObjectiveProgress, ObjectiveUI> _rowMap
        = new Dictionary<ObjectiveProgress, ObjectiveUI>();

    // Animation settings
    [Header("Panel Animation")]
    [SerializeField] private float _animationDuration = 0.35f;
    [SerializeField] private Ease _openEase = Ease.OutBack;
    [SerializeField] private Ease _closeEase = Ease.InBack;

    private CanvasGroup _canvasGroup;
    private Vector3 _originalScale;
    private bool _isOpen = false;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _originalScale = transform.localScale;

        // CHANGED: We removed SetActive(false) so the script stays awake to listen for the 'M' key!
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
        transform.localScale = _originalScale * 0.7f;
    }

    public void OpenPanel()
    {
        if (_isOpen) return;
        _isOpen = true;

        transform.DOKill();
        _canvasGroup.DOKill();

        Sequence openSeq = DOTween.Sequence();
        openSeq.Append(transform.DOScale(_originalScale, _animationDuration).SetEase(_openEase));
        openSeq.Join(_canvasGroup.DOFade(1f, _animationDuration));
        openSeq.OnComplete(() =>
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
        });

        // Force a refresh the exact moment it opens
        SyncRows();
        RefreshAllRows();
        UpdateEmptyState();
    }

    public void ClosePanel()
    {
        if (!_isOpen) return;
        _isOpen = false;

        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;

        transform.DOKill();
        _canvasGroup.DOKill();

        Sequence closeSeq = DOTween.Sequence();
        closeSeq.Append(transform.DOScale(_originalScale * 0.7f, _animationDuration).SetEase(_closeEase));
        closeSeq.Join(_canvasGroup.DOFade(0f, _animationDuration));
        // CHANGED: Removed SetActive(false) here as well!
    }

    private void Update()
    {
        if (MissionManager.Instance == null) return;

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (_isOpen)
                ClosePanel();
            else
                OpenPanel();
        }

        // CHANGED: Only recalculate the UI lists if the panel is currently visible to save performance
        if (_isOpen)
        {
            SyncRows();
            RefreshAllRows();
            UpdateEmptyState();
        }
    }

    private void SyncRows()
    {
        List<ObjectiveProgress> liveProgress = GetAllProgress();

        foreach (ObjectiveProgress progress in liveProgress)
        {
            if (!_rowMap.ContainsKey(progress))
            {
                ObjectiveUI newRow = Instantiate(objectiveRowPrefab, rowsContainer);
                _rowMap[progress] = newRow;
            }
        }

        List<ObjectiveProgress> toRemove = new List<ObjectiveProgress>();
        foreach (ObjectiveProgress tracked in _rowMap.Keys)
        {
            if (!liveProgress.Contains(tracked))
                toRemove.Add(tracked);
        }

        foreach (ObjectiveProgress old in toRemove)
        {
            Destroy(_rowMap[old].gameObject);
            _rowMap.Remove(old);
        }
    }

    private void RefreshAllRows()
    {
        foreach (var pair in _rowMap)
            pair.Value.Refresh(pair.Key);
    }

    private void UpdateEmptyState()
    {
        if (emptyStateLabel == null) return;
        emptyStateLabel.SetActive(_rowMap.Count == 0);
    }

    private List<ObjectiveProgress> GetAllProgress()
    {
        List<ObjectiveProgress> all = new List<ObjectiveProgress>();
        foreach (ActiveMission mission in MissionManager.Instance.activeMissions)
            all.AddRange(mission.progressList);
        return all;
    }
}