using System;
using UnityEngine;

// Converts wave definitions from a JSON TextAsset into the same WaveConfig/DifficultySpawnRange
// objects EnemySpawner already knows how to read, so nothing downstream had to change. Each
// WaveConfig built this way lives only in memory (ScriptableObject.CreateInstance, not an
// asset on disk) - it behaves exactly like the hand-authored .asset versions did.
public static class WaveConfigJsonLoader
{
    public static WaveConfig[] Load(TextAsset json)
    {
        if (json == null)
        {
            Debug.LogError("WaveConfigJsonLoader: no JSON TextAsset assigned.");
            return new WaveConfig[0];
        }

        WaveDataList data = JsonUtility.FromJson<WaveDataList>(json.text);
        if (data == null || data.waves == null)
        {
            Debug.LogError($"WaveConfigJsonLoader: failed to parse '{json.name}' - check it's " +
                            "valid JSON with a top-level \"waves\" array.");
            return new WaveConfig[0];
        }

        WaveConfig[] result = new WaveConfig[data.waves.Length];
        for (int i = 0; i < data.waves.Length; i++)
        {
            result[i] = ToWaveConfig(data.waves[i], i);
        }
        return result;
    }

    static WaveConfig ToWaveConfig(WaveData source, int waveIndex)
    {
        WaveConfig config = ScriptableObject.CreateInstance<WaveConfig>();
        config.waveName = string.IsNullOrEmpty(source.waveName) ? $"Wave {waveIndex + 1}" : source.waveName;
        config.spawnStagger = source.spawnStagger;

        int rangeCount = source.spawnRanges?.Length ?? 0;
        config.spawnRanges = new DifficultySpawnRange[rangeCount];
        for (int i = 0; i < rangeCount; i++)
        {
            config.spawnRanges[i] = ToSpawnRange(source.spawnRanges[i], config.waveName);
        }

        return config;
    }

    static DifficultySpawnRange ToSpawnRange(DifficultySpawnRangeData source, string waveName)
    {
        if (!Enum.TryParse(source.difficulty, ignoreCase: true, out EnemyDifficulty difficulty))
        {
            Debug.LogWarning($"WaveConfigJsonLoader: '{waveName}' has an unrecognized difficulty " +
                              $"\"{source.difficulty}\" - defaulting to Easy.");
            difficulty = EnemyDifficulty.Easy;
        }

        int min = Mathf.Max(source.minCount, 0);
        int max = Mathf.Max(source.maxCount, 0);

        // Validation: minCount can't exceed maxCount - the same idea as "kill count can't
        // exceed enemy count", applied to this schema. JSON has no [Min(0)]-style Inspector
        // guard, so a malformed file needs to be caught here instead of silently misbehaving.
        if (min > max)
        {
            Debug.LogWarning($"WaveConfigJsonLoader: '{waveName}' {difficulty} has minCount ({min}) " +
                              $"> maxCount ({max}) - swapping them.");
            (min, max) = (max, min);
        }

        return new DifficultySpawnRange { difficulty = difficulty, minCount = min, maxCount = max };
    }
}
