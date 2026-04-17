using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Phuc.SoundSystem;

public class DailyResultUI : MonoBehaviour
{
    public static DailyResultUI Instance;

    [Header("UI References")]
    [Tooltip("The main pop-up panel that holds the text and buttons.")]
    public GameObject resultPanel;

    [Header("Text Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Buttons")]
    public Button advanceButton;
    public Button mainMenuButton;

    [Header("Advance Button Sprites")]
    public Sprite nextDaySprite;
    public Sprite restartSprite;

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Optional Sounds")]
    public SO_SFXEvent winSound;
    public SO_SFXEvent loseSound;

    private bool isWinState = false;

    private void Awake()
    {
        // Prevent zombie duplicates
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Hide everything at start
        resultPanel.SetActive(false);
        resultPanel.transform.localScale = Vector3.zero;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnEnable()
    {
        advanceButton.onClick.AddListener(OnAdvanceClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    private void OnDisable()
    {
        advanceButton.onClick.RemoveListener(OnAdvanceClicked);
        mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
    }

    public void ShowWinScreen()
    {
        isWinState = true;

        if (winSound != null && SoundManager.Instance != null) SoundManager.Instance.PlaySfx(winSound);

        titleText.text = "Day Survived!";
        titleText.color = Color.green;

        if (QuotaManager.Instance != null)
        {
            descriptionText.text = $"Quota Met: {QuotaManager.Instance.GoldTarget}g\nGreat job! Ready for the next day?";
        }

        if (advanceButton != null)
        {
            // --- NEW: Make sure the button is turned ON for winning ---
            advanceButton.gameObject.SetActive(true);

            if (nextDaySprite != null)
            {
                advanceButton.GetComponent<Image>().sprite = nextDaySprite;
            }
        }

        ShowPanel();
    }

    public void ShowLoseScreen()
    {
        isWinState = false;

        if (loseSound != null && SoundManager.Instance != null) SoundManager.Instance.PlaySfx(loseSound);

        titleText.text = "Game Over!";
        titleText.color = Color.red;

        if (QuotaManager.Instance != null)
        {
            descriptionText.text = $"You only made {QuotaManager.Instance.GoldEarned}g out of the {QuotaManager.Instance.GoldTarget}g needed.\n Mr Gustaff kicked you out of the village.";
        }

        if (advanceButton != null)
        {
            // --- NEW: Turn OFF the advance/restart button completely so they can only go to Main Menu ---
            advanceButton.gameObject.SetActive(false);
        }

        ShowPanel();
    }

    private void ShowPanel()
    {
        resultPanel.transform.DOKill();

        resultPanel.SetActive(true);
        resultPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);

        Time.timeScale = 0f;
    }

    private void OnAdvanceClicked()
    {
        Time.timeScale = 1f;

        if (isWinState)
        {
            if (QuotaManager.Instance != null) QuotaManager.Instance.ApplyWinRewards();
            if (TimeManager.Instance != null) TimeManager.Instance.ExecuteSleepRoutine();

            resultPanel.transform.DOKill();
            resultPanel.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
            {
                resultPanel.SetActive(false);
            });

            PlayerStateManager player = Object.FindFirstObjectByType<PlayerStateManager>();
            if (player != null) player.EndInteraction();
        }
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}