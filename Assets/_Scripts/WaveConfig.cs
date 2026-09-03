using UnityEngine;

// How many enemies of each difficulty a single wave spawns. min/max are rolled
// independently per difficulty, so leaving min==max gives you an exact count and
// min<max gives a random range - covers both "let it be random" and "I want to
// choose exactly how many of each" from the same field.
[System.Serializable]
public class DifficultySpawnRange
{
    public EnemyDifficulty difficulty;
    [Min(0)] public int minCount = 0;
    [Min(0)] public int maxCount = 0;

    // How many enemies of this difficulty to spawn this time, decided once per wave start.
    public int RollCount()
    {
        int lo = Mathf.Min(minCount, maxCount);
        int hi = Mathf.Max(minCount, maxCount);
        return Random.Range(lo, hi + 1); // Random.Range(int,int) is max-exclusive, so +1
    }
}

// One wave's worth of spawn settings. Create via Assets > Create > Halloween > Wave Config.
// A night is just an ordered list of these (see NightManager).
[CreateAssetMenu(fileName = "WaveConfig", menuName = "Halloween/Wave Config")]
public class WaveConfig : ScriptableObject
{
    [Header("Identity")]
    public string waveName = "Wave 1";

    [Header("How many of each difficulty to spawn")]
    public DifficultySpawnRange[] spawnRanges = new DifficultySpawnRange[]
    {
        new DifficultySpawnRange { difficulty = EnemyDifficulty.Easy },
        new DifficultySpawnRange { difficulty = EnemyDifficulty.Medium },
        new DifficultySpawnRange { difficulty = EnemyDifficulty.Hard },
    };

    [Header("Pacing")]
    [Tooltip("Seconds between each individual enemy spawn, so a wave doesn't pop in all at once.")]
    public float spawnStagger = 0.15f;
}
