using System.Collections;
using DG.Tweening;
using UnityEngine;

// Shows the store after a night ends, and starts the next night when the player confirms.
// Waits a real-time delay first (WaitForSeconds, not yet paused) so the "night cleared"
// banner/audio - both reacting to the same NightManager.OnNightComplete - play out normally
// before the store takes over. Only then does it pause the game (Time.timeScale = 0) and show
// the store panel - Unity's UI event system isn't tied to timeScale, so buttons keep working
// while the world underneath is frozen
public class StoreController : MonoBehaviour
{
    [SerializeField] NightManager nightManager;
    [Tooltip("The full-screen store panel - should completely cover the game view.")]
    [SerializeField] GameObject storePanel;
    [SerializeField] float delayBeforeStore = 2f;
    [SerializeField] float fadeDuration = 0.5f;

    CanvasGroup canvasGroup;
    Tween fadeTween;

    void Awake()
    {
        // Left to auto-find: add a CanvasGroup to Store UI (or its Canvas) and this picks it up.
        // includeInactive so it's found even though the panel is hidden.
        if (storePanel != null) canvasGroup = storePanel.GetComponentInChildren<CanvasGroup>(true);
        if (canvasGroup == null)
            Debug.LogWarning("StoreController: no CanvasGroup under storePanel - the store will " +
                             "open instantly. Add a CanvasGroup to 'Store UI' to get the fade.", this);
    }

    void OnEnable()
    {
        if (nightManager != null) nightManager.OnNightComplete += HandleNightComplete;
    }

    void OnDisable()
    {
        if (nightManager != null) nightManager.OnNightComplete -= HandleNightComplete;
        fadeTween?.Kill();
        storePanel.SetActive(false);
    }

    void Start()
    {
        if (storePanel != null) storePanel.SetActive(false);
    }

    void HandleNightComplete()
    {
        StartCoroutine(ShowStoreAfterDelay());
    }

    IEnumerator ShowStoreAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeStore);

        if (storePanel != null)
        {
            // Start fully transparent BEFORE activating, so there's no one-frame flash of the
            // finished panel. Non-interactable until the fade ends so a half-visible Buy button
            // can't be clicked.
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
            }

            storePanel.SetActive(true);
            FadeIn();
        }

        Time.timeScale = 0f;
    }

    // SetUpdate(true) makes the tween ignore Time.timeScale. DOTween tweens use scaled time by
    // default, so without it this fade would never advance: the game is paused (timeScale = 0)
    // the moment the panel appears.
    void FadeIn()
    {
        if (canvasGroup == null) return;

        fadeTween?.Kill();
        fadeTween = canvasGroup.DOFade(1f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() => canvasGroup.interactable = true);
    }

    // Wired to the store's Confirm button (Button.onClick).
    public void ConfirmAndStartNextNight()
    {
        Time.timeScale = 1f;
        if (storePanel != null) storePanel.SetActive(false);
        if (nightManager != null) nightManager.BeginNight();
    }
}
