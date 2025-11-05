using UnityEngine;
using UnityEngine.UI;

public class ShowDetailBtn : MonoBehaviour
{
    public UpgradeType upgradeType;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            UpgradeUIManager.Instance.ShowDetail(upgradeType);
        });
    }
}
