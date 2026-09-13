using System;

// Plain data shapes for parsing coin drop amounts out of a JSON file via JsonUtility - same
// approach as WaveDataJson. difficulty is a string ("Easy"/"Medium"/"Hard") since JsonUtility
// can't parse a JSON string into an enum by name; CoinDropJsonLoader converts it.
[Serializable]
public class CoinDropDataList
{
    public CoinDropData[] coinDrops;
}

[Serializable]
public class CoinDropData
{
    public string difficulty;
    public int amount;
}
