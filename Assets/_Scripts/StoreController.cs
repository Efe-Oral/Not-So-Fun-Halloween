using System.Collections;
using UnityEngine;

// Shows the store after a night ends, and starts the next night when the player confirms.
// Waits a real-time delay first (WaitForSeconds, not yet paused) so the "night cleared"
// banner/audio - both reacting to the same NightManager.OnNightComplete - play out normally
// before the store takes over. Only then does it pause the game (Time.timeScale = 0) and show
// the store panel - Unity's UI event system isn't tied to timeScale, so buttons keep working
// while the world underneath is frozen.
public class StoreController : MonoBehaviour
{
    [SerializeField] NightManager nightManager;
    [Tooltip("The full-screen store panel - should completely cover the game view.")]
    [SerializeField] GameObject storePanel;
    [SerializeField] float delayBeforeStore = 2f;

    void OnEnable()
    {
        if (nightManager != null) nightManager.OnNightComplete += HandleNightComplete;
    }

    void OnDisable()
    {
        if (nightManager != null) nightManager.OnNightComplete -= HandleNightComplete;
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

        if (storePanel != null) storePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // Wired to the store's Confirm button (Button.onClick).
    public void ConfirmAndStartNextNight()
    {
        Time.timeScale = 1f;
        if (storePanel != null) storePanel.SetActive(false);
        if (nightManager != null) nightManager.BeginNight();
    }
}
