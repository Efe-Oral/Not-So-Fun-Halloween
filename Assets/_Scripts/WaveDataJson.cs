using System;

// Plain data shapes for parsing wave definitions out of a JSON file via JsonUtility. These
// mirror WaveConfig/DifficultySpawnRange's fields, but as raw data with no Unity Object
// baggage - JsonUtility needs public fields, can't parse a bare top-level array (hence
// WaveDataList wrapping "waves"), and can't parse an enum from its name, so difficulty is
// a string here and gets converted to EnemyDifficulty by WaveConfigJsonLoader.
[Serializable]
public class WaveDataList
{
    public WaveData[] waves;
}

[Serializable]
public class WaveData
{
    public string waveName;
    public float spawnStagger = 0.15f;
    public DifficultySpawnRangeData[] spawnRanges;
}

[Serializable]
public class DifficultySpawnRangeData
{
    public string difficulty; // "Easy", "Medium", or "Hard"
    public int minCount;
    public int maxCount;
}
