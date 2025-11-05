using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class PopupText : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private TMP_Text text;

    public void SetText(string textString, Color textColor = default)
    {
        textColor = textColor == default ? Color.white : textColor;
        text.text = textString;
        text.color = textColor;
    }

    void OnEnable()
    {
        rectTransform.localScale = Vector3.zero;
        rectTransform.localPosition = Vector3.zero;

        rectTransform.DOScale(Vector3.one, 0.15f);
        rectTransform.DOAnchorPosY(0.5f, 0.15f).OnComplete(() => 
        {
            rectTransform.DOScale(Vector3.zero, 0.5f);
            rectTransform.DOAnchorPosY(0.75f, 0.5f).OnComplete(() => 
            {
              
                text.color = Color.white;
            });
        });
    }
}
