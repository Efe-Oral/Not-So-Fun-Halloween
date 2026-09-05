using UnityEngine;

// Plays SFX for wave state changes. Independent from WaveAnnouncerUI - it listens to the
// exact same NightManager events but neither script knows the other exists. That's the payoff
// of using events here: adding audio didn't require touching the UI script or NightManager's
// wave logic at all, just one more subscriber.
public class WaveAudioManager : MonoBehaviour
{
    [SerializeField] NightManager nightManager;
    [SerializeField] AudioSource audioSource;

    [Header("Clips")]
    [SerializeField] AudioClip countdownTickClip;
    [SerializeField] AudioClip waveStartClip;
    [SerializeField] AudioClip waveClearedClip;
    [SerializeField] AudioClip nightCompleteClip;

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

    // secondsRemaining is unused here but has to match OnCountdownTick's Action<int> signature.
    void HandleCountdownTick(int secondsRemaining) => Play(countdownTickClip);
    void HandleWaveStarted(int waveIndex) => Play(waveStartClip);
    void HandleWaveCleared(int waveIndex) => Play(waveClearedClip);
    void HandleNightComplete() => Play(nightCompleteClip);

    // PlayOneShot (not audioSource.Play) so overlapping clips - e.g. a wave-cleared sting
    // ringing out while the next countdown starts ticking - don't cut each other off.
    void Play(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }
}
