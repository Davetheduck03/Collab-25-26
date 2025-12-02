using DG.Tweening;
using UnityEngine;

public class ButtonAnimation : MonoBehaviour
{
    public void OnHover()
    {
        transform.DOScale(1.1f, 0.15f).SetEase(Ease.OutQuad);
    }

    public void OnExit()
    {
        transform.DOScale(1f, 0.15f).SetEase(Ease.OutQuad);
    }

    public void OnClickAnim()
    {
        transform.DOPunchScale(Vector3.one * 0.15f, 0.2f);
    }
}
