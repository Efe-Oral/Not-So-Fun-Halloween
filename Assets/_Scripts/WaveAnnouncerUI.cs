using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// Shows wave state using sprite banners instead of text: a countdown number for the last 3
// seconds before a wave, then a phrase banner ("night starts!", "wave cleared", "night
// cleared"). Purely reactive - only listens to NightManager's events, never touches
// spawning/wave logic itself.
public class WaveAnnouncerUI : MonoBehaviour
{
    [SerializeField] NightManager nightManager;

    [Header("Phrase banner")]
    [SerializeField] Image phraseImage;
    [Tooltip("Shown on every OnCountdownTick, alongside the countdown number.")]
    [SerializeField] Sprite nextWaveInSprite;
    [Tooltip("Shown when the night's first wave (index 0) starts. Later waves get no banner - " +
             "the countdown beforehand already announced them.")]
    [SerializeField] Sprite nightStartsSprite;
    [SerializeField] Sprite waveClearedSprite;
    [SerializeField] Sprite nightClearedSprite;

    [Header("Countdown number (last 3 seconds of a countdown only)")]
    [SerializeField] Image numberImage;
    [Tooltip("Index 0 = \"1\", index 1 = \"2\", index 2 = \"3\".")]
    [SerializeField] Sprite[] countdownSprites = new Sprite[3];

    [Header("Animation")]
    [SerializeField] float popScale = 1.3f;
    [SerializeField] float popDuration = 0.2f;
    [SerializeField] float holdDuration = 0.8f;
    [SerializeField] float fadeDuration = 0.3f;

    Sequence phraseSequence;
    Sequence numberSequence;
    Sprite currentPhraseSprite;

    void Awake()
    {
        SetAlpha(phraseImage, 0f);
        SetAlpha(numberImage, 0f);
    }

    void OnEnable()
    {
        nightManager.OnCountdownTick += HandleCountdownTick;
        nightManager.OnWaveStarted += HandleWaveStarted;
        nightManager.OnWaveCleared += HandleWaveCleared;
        nightManager.OnNightComplete += HandleNightComplete;
    }

    void OnDisable()
    {
        nightManager.OnCountdownTick -= HandleCountdownTick;
        nightManager.OnWaveStarted -= HandleWaveStarted;
        nightManager.OnWaveCleared -= HandleWaveCleared;
        nightManager.OnNightComplete -= HandleNightComplete;
    }

    void HandleCountdownTick(int secondsRemaining)
    {
        // CurrentWaveNumber is still 0 during the countdown before wave 1 - skip the "next
        // wave in..." banner there since "night starts!" (from HandleWaveStarted) already
        // announces it once the wave actually begins; between-wave countdowns still show it.
        if (nightManager.CurrentWaveNumber > 0) ShowPhrase(nextWaveInSprite);

        int index = secondsRemaining - 1; // remaining=1 -> "1" sprite at index 0, etc.
        if (index >= 0 && index < countdownSprites.Length)
            ShowNumber(countdownSprites[index]);
        else
            HideNumber();
    }

    void HandleWaveStarted(int waveIndex)
    {
        HideNumber();
        if (waveIndex == 0) ShowPhrase(nightStartsSprite);
    }

    void HandleWaveCleared(int waveIndex) => ShowPhrase(waveClearedSprite);
    void HandleNightComplete() => ShowPhrase(nightClearedSprite);

    // Only restarts the pop/hold/fade if the sprite actually changed. Without this, every
    // single countdown tick would re-trigger "next wave in..." from scratch and it would
    // never stop bouncing for the whole countdown - this way it pops in once and lets its
    // own animation finish while only the number keeps updating underneath it.
    void ShowPhrase(Sprite sprite)
    {
        if (sprite == null || sprite == currentPhraseSprite) return;
        currentPhraseSprite = sprite;
        phraseSequence?.Kill();
        phraseImage.sprite = sprite;
        Pop(phraseImage, out phraseSequence);
    }

    void ShowNumber(Sprite sprite)
    {
        if (sprite == null) return;
        numberSequence?.Kill();
        numberImage.sprite = sprite;
        Pop(numberImage, out numberSequence);
    }

    void HideNumber()
    {
        numberSequence?.Kill();
        SetAlpha(numberImage, 0f);
    }

    // Pops an image in (scale bounce), holds, then fades out. Used for both the phrase
    // banner and the countdown number so they animate identically.
    void Pop(Image image, out Sequence sequence)
    {
        image.transform.localScale = Vector3.zero;
        SetAlpha(image, 1f);

        sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOScale(popScale, popDuration).SetEase(Ease.OutBack));
        sequence.Append(image.transform.DOScale(1f, popDuration * 0.5f));
        sequence.AppendInterval(holdDuration);
        sequence.Append(image.DOFade(0f, fadeDuration));
    }

    void SetAlpha(Image image, float alpha)
    {
        Color c = image.color;
        image.color = new Color(c.r, c.g, c.b, alpha);
    }
}
