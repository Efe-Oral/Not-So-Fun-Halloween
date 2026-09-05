using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Runs one night: 3 waves in order, back to back. A wave starts, EnemySpawner spawns its
// enemies off-screen, and NightManager waits until every enemy from that wave is dead
// before starting the next one. Other scripts (UI, audio) can hook the events below
// instead of polling this for state.
public class NightManager : MonoBehaviour
{
    [SerializeField] EnemySpawner spawner;
    [SerializeField] WaveConfig[] waves = new WaveConfig[3];

    [Header("Pacing")]
    [SerializeField] float delayBeforeFirstWave = 2f;
    [SerializeField] float delayBetweenWaves = 5f;
    [Tooltip("How many of the final countdown ticks carry a number (matches how many digit " +
             "sprites the UI has). The countdown always shows at least one label-only tick " +
             "('next wave in...' alone) before the numbers start, so short delays get " +
             "stretched to at least countdownDigits + 1 seconds.")]
    [SerializeField] int countdownDigits = 3;

    // waveIndex is 0-based (0 = Wave 1).
    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCleared;
    public event Action OnNightComplete;

    // Fired once per second while waiting before a wave, counting down to 0 (e.g. 3, 2, 1).
    // UI/audio use this for "wave incoming" countdowns instead of the wave just popping in.
    public event Action<int> OnCountdownTick;

    public int CurrentWaveNumber { get; private set; } // 1-based, 0 before the night starts
    public int TotalWaves => waves.Length;
    public bool IsNightComplete { get; private set; }

    readonly List<Health> aliveEnemies = new List<Health>();
    bool nightStarted;

    // Doesn't auto-start in Start() - something else (e.g. NightStartPrompt, gated on a key
    // press) has to call this. Keeps NightManager reactive/driven, the same way it doesn't
    // know or care who's listening to its own events.
    public void BeginNight()
    {
        if (nightStarted) return;
        nightStarted = true;
        StartCoroutine(RunNight());
    }

    IEnumerator RunNight()
    {
        yield return StartCoroutine(RunCountdown(delayBeforeFirstWave));

        for (int i = 0; i < waves.Length; i++)
        {
            if (waves[i] == null)
            {
                Debug.LogWarning($"NightManager: wave slot {i} is empty, skipping.", this);
                continue;
            }

            CurrentWaveNumber = i + 1;
            yield return StartCoroutine(RunWave(i, waves[i]));

            if (i < waves.Length - 1)
                yield return StartCoroutine(RunCountdown(delayBetweenWaves));
        }

        IsNightComplete = true;
        OnNightComplete?.Invoke();
    }

    // Counts down to 1, firing OnCountdownTick once per second. Always includes at least one
    // tick above countdownDigits (e.g. "4") before entering the numbered "3, 2, 1" range, so
    // the "next wave in..." label always gets a beat on its own first instead of popping in
    // at the same instant as the first number. If seconds is too short to fit that, the wait
    // is stretched to countdownDigits + 1 rather than skipping the label-only beat.
    IEnumerator RunCountdown(float seconds)
    {
        int wholeSeconds = Mathf.Max(Mathf.CeilToInt(seconds), countdownDigits + 1);
        for (int remaining = wholeSeconds; remaining > 0; remaining--)
        {
            OnCountdownTick?.Invoke(remaining);
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator RunWave(int waveIndex, WaveConfig wave)
    {
        aliveEnemies.Clear();
        if (OnWaveStarted != null)
        {
           OnWaveStarted.Invoke(waveIndex);
        }

        Debug.Log("Starting wave #" + waveIndex);

        yield return StartCoroutine(spawner.SpawnWave(wave, RegisterEnemy));

        // Wave might roll zero enemies (all ranges 0-0) - don't hang forever waiting on nothing.
        yield return new WaitUntil(() => aliveEnemies.Count == 0);

        OnWaveCleared?.Invoke(waveIndex);
    }

    void RegisterEnemy(GameObject enemy)
    {
        Health health = enemy.GetComponent<Health>();
        if (health == null) return;

        aliveEnemies.Add(health);
        health.OnDied += () => aliveEnemies.Remove(health);
    }
}
