using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Instantiates enemies for a wave at points outside the player's camera view.
// Doesn't know anything about waves clearing or nights - NightManager drives it and
// listens to the enemies it hands back via the onEnemySpawned callback.
public class EnemySpawner : MonoBehaviour
{
    [Serializable]
    public class DifficultyPrefabs
    {
        public EnemyDifficulty difficulty;
        public GameObject[] prefabs;
    }

    [Header("Enemy prefabs, grouped by difficulty")]
    [SerializeField] DifficultyPrefabs[] prefabsByDifficulty;

    [Header("References")]
    [Tooltip("Left empty, this finds the object tagged 'Player' at Awake.")]
    [SerializeField] Transform player;
    [Tooltip("Left empty, this uses Camera.main. Spawn points are chosen outside THIS camera's view.")]
    [SerializeField] Camera viewCamera;

    [Header("Off-screen placement")]
    [Tooltip("Extra distance (world units) beyond the camera's edge before a point counts as safely off-screen.")]
    [SerializeField] float offscreenBuffer = 2f;
    [Tooltip("Spawn points are chosen at a random distance within this many extra units past the off-screen minimum, so enemies don't all appear in a ring at exactly the same radius.")]
    [SerializeField] float spawnRingDepth = 6f;
    [Tooltip("Optional: enemies won't spawn on top of these (walls, props). Leave empty to skip the check.")]
    [SerializeField] LayerMask obstacleMask;
    [Tooltip("Radius used when checking a candidate spawn point against obstacleMask.")]
    [SerializeField] float obstacleCheckRadius = 0.5f;
    [Tooltip("Optional: candidate points outside this collider's bounds are rejected, so enemies can't spawn outside the playable map. Leave empty to skip the check.")]
    [SerializeField] Collider2D levelMapBounds;
    [SerializeField] int maxAttemptsPerEnemy = 20;

    void Awake()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
        if (viewCamera == null) viewCamera = Camera.main;
    }

    // Spawns every enemy this wave calls for, staggered over time. Call via StartCoroutine.
    // onEnemySpawned is invoked once per enemy right after Instantiate, so the caller
    // (NightManager) can hook into that enemy's Health without this script knowing about waves.
    public IEnumerator SpawnWave(WaveConfig wave, Action<GameObject> onEnemySpawned)
    {
        foreach (DifficultySpawnRange range in wave.spawnRanges)
        {
            int count = range.RollCount();
            for (int i = 0; i < count; i++)
            {
                GameObject enemy = SpawnOne(range.difficulty);
                if (enemy != null) onEnemySpawned?.Invoke(enemy);

                if (wave.spawnStagger > 0f)
                    yield return new WaitForSeconds(wave.spawnStagger);
            }
        }
    }

    GameObject SpawnOne(EnemyDifficulty difficulty)
    {
        GameObject prefab = PickPrefab(difficulty);
        if (prefab == null)
        {
            Debug.LogWarning($"EnemySpawner: no prefabs assigned for difficulty {difficulty}.", this);
            return null;
        }

        Vector2 point = FindOffscreenPoint();
        return Instantiate(prefab, point, Quaternion.identity);
    }

    GameObject PickPrefab(EnemyDifficulty difficulty)
    {
        foreach (DifficultyPrefabs entry in prefabsByDifficulty)
        {
            if (entry.difficulty != difficulty) continue;
            if (entry.prefabs == null || entry.prefabs.Length == 0) return null;
            return entry.prefabs[UnityEngine.Random.Range(0, entry.prefabs.Length)];
        }
        return null;
    }

    // Picks a random point around the player, far enough out that it's guaranteed to sit
    // outside the camera's current view. Retries a few times to dodge obstacles/level edges;
    // falls back to the last candidate if nothing clean turns up.
    Vector2 FindOffscreenPoint()
    {
        Vector2 origin = player != null ? (Vector2)player.position : Vector2.zero;
        float minRadius = OffscreenRadius() + offscreenBuffer;

        Vector2 candidate = origin;
        for (int attempt = 0; attempt < maxAttemptsPerEnemy; attempt++)
        {
            float radius = minRadius + UnityEngine.Random.Range(0f, spawnRingDepth);
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            candidate = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            if (levelMapBounds != null && !levelMapBounds.bounds.Contains(candidate)) continue;

            if (obstacleMask.value != 0 &&
                Physics2D.OverlapCircle(candidate, obstacleCheckRadius, obstacleMask) != null) continue;

            return candidate;
        }

        return candidate; // couldn't find a clean spot - spawn at the last try rather than not spawning at all
    }

    // Half-diagonal of the camera's orthographic view: any point at least this far from
    // the camera's center is guaranteed outside the visible rectangle in every direction.
    float OffscreenRadius()
    {
        if (viewCamera == null || !viewCamera.orthographic) return 15f; // sane fallback if no camera found
        float halfHeight = viewCamera.orthographicSize;
        float halfWidth = halfHeight * viewCamera.aspect;
        return Mathf.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight);
    }
}
