using System;
using System.Collections.Generic;
using UnityEngine;

// Loads coin-drop amounts from JSON once, and holds one coin pickup prefab per difficulty
// (same grouped-by-difficulty idea as EnemySpawner.DifficultyPrefabs, just one prefab instead
// of a random pick from several), so individual enemy prefabs (EnemyCoinDrop) need zero
// per-instance wiring - they just read from this table's static accessors. One instance of
// this should exist in the scene.
public class CoinDropTable : MonoBehaviour
{
    [Serializable]
    public class DifficultyCoinPrefab
    {
        public EnemyDifficulty difficulty;
        public CoinPickup prefab;
    }

    [SerializeField] TextAsset coinDropsJson;
    [SerializeField] DifficultyCoinPrefab[] coinPickupPrefabsByDifficulty;

    static Dictionary<EnemyDifficulty, int> amounts;
    static Dictionary<EnemyDifficulty, CoinPickup> prefabs;

    void Awake()
    {
        amounts = CoinDropJsonLoader.Load(coinDropsJson);

        prefabs = new Dictionary<EnemyDifficulty, CoinPickup>();
        foreach (DifficultyCoinPrefab entry in coinPickupPrefabsByDifficulty)
        {
            prefabs[entry.difficulty] = entry.prefab;
        }
    }

    public static CoinPickup GetPrefab(EnemyDifficulty difficulty)
    {
        if (prefabs != null && prefabs.TryGetValue(difficulty, out CoinPickup prefab)) return prefab;
        return null;
    }

    public static int GetAmount(EnemyDifficulty difficulty)
    {
        if (amounts != null && amounts.TryGetValue(difficulty, out int amount)) return amount;
        return 0;
    }
}
