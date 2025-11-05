using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class ConfirmBox_Enter : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Button buttonYes;
    [SerializeField] private Button buttonNo;

    private void OnEnable()
    {
        rectTransform.localScale = Vector3.zero;
        buttonYes.transform.localScale = Vector3.zero;
        buttonNo.transform.localScale = Vector3.zero;
        
        Sequence sequence = DOTween.Sequence();
        
        sequence.Append(rectTransform.DOScale(1, 0.5f).SetEase(Ease.OutBack)).SetUpdate(true);
        sequence.AppendInterval(0.25f);
        sequence.Append(buttonYes.transform.DOScale(1, 0.5f).SetEase(Ease.OutQuad))
                .Join(buttonNo.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack).SetDelay(0.2f)).SetUpdate(true);

        sequence.AppendCallback(() =>
        {
            buttonYes.onClick.AddListener(Enter);
            buttonNo.onClick.AddListener(Leave);
        }).SetUpdate(true);
    }

    private void Enter()
    {
        Time.timeScale = 1;
       
    }

    private void Leave()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }
}
