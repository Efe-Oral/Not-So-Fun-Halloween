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

    // waveIndex is 0-based (0 = Wave 1).
    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCleared;
    public event Action OnNightComplete;

    public int CurrentWaveNumber { get; private set; } // 1-based, 0 before the night starts
    public int TotalWaves => waves.Length;
    public bool IsNightComplete { get; private set; }

    readonly List<Health> aliveEnemies = new List<Health>();

    void Start()
    {
        StartCoroutine(RunNight());
    }

    IEnumerator RunNight()
    {
        yield return new WaitForSeconds(delayBeforeFirstWave);

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
                yield return new WaitForSeconds(delayBetweenWaves);
        }

        IsNightComplete = true;
        OnNightComplete?.Invoke();
    }

    IEnumerator RunWave(int waveIndex, WaveConfig wave)
    {
        aliveEnemies.Clear();
        OnWaveStarted?.Invoke(waveIndex);
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
