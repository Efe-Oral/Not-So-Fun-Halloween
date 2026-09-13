using System;
using System.Collections.Generic;
using UnityEngine;

// Parses coin-drop-per-difficulty amounts from a JSON TextAsset into a lookup table. Same
// shape and validation approach as WaveConfigJsonLoader: an unrecognized difficulty string
// logs a warning and is skipped rather than crashing, and negative amounts are clamped to 0.
public static class CoinDropJsonLoader
{
    public static Dictionary<EnemyDifficulty, int> Load(TextAsset json)
    {
        var result = new Dictionary<EnemyDifficulty, int>();

        if (json == null)
        {
            Debug.LogError("CoinDropJsonLoader: no JSON TextAsset assigned.");
            return result;
        }

        CoinDropDataList data = JsonUtility.FromJson<CoinDropDataList>(json.text);
        if (data == null || data.coinDrops == null)
        {
            Debug.LogError($"CoinDropJsonLoader: failed to parse '{json.name}' - check it's " +
                            "valid JSON with a top-level \"coinDrops\" array.");
            return result;
        }

        foreach (CoinDropData entry in data.coinDrops)
        {
            if (!Enum.TryParse(entry.difficulty, ignoreCase: true, out EnemyDifficulty difficulty))
            {
                Debug.LogWarning($"CoinDropJsonLoader: unrecognized difficulty " +
                                  $"\"{entry.difficulty}\" - skipping.");
                continue;
            }

            result[difficulty] = Mathf.Max(entry.amount, 0);
        }

        return result;
    }
}
