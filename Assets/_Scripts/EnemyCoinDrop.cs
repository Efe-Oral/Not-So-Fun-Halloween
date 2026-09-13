using UnityEngine;

// Spawns a coin pickup when this enemy dies, sized and skinned by its difficulty (via
// CoinDropTable). Purely reactive to Health.OnDied, same pattern as EnemyDeathVisual - doesn't
// touch EnemyAI's state machine or Health itself, and needs zero per-instance wiring: EnemyAI's
// own Config (already used by EnemyAttack) supplies the difficulty, and CoinDropTable supplies
// both the amount/prefab-per-difficulty lookups.
public class EnemyCoinDrop : MonoBehaviour
{
    [Tooltip("Where the coin lands relative to the death position - negative X/Y drops it " +
             "toward the lower-left, matching the little pop-and-fall animation on the coin.")]
    [SerializeField] Vector2 landingOffset = new Vector2(-0.5f, -0.3f);

    Health health;
    EnemyAI enemyAI;

    void Awake()
    {
        // Health/EnemyAI may sit on this object or on a parent - look in both places.
        health = GetComponentInParent<Health>();
        enemyAI = GetComponentInParent<EnemyAI>();
    }

    void OnEnable()
    {
        if (health != null) health.OnDied += HandleDied;
    }

    void OnDisable()
    {
        if (health != null) health.OnDied -= HandleDied;
    }

    void HandleDied()
    {
        if (enemyAI == null || enemyAI.Config == null) return;

        EnemyDifficulty difficulty = enemyAI.Config.difficulty;
        CoinPickup prefab = CoinDropTable.GetPrefab(difficulty);
        if (prefab == null) return;

        int amount = CoinDropTable.GetAmount(difficulty);
        if (amount <= 0) return;

        Vector3 spawnPosition = transform.position;
        Vector3 landingPosition = spawnPosition + (Vector3)landingOffset;

        CoinPickup pickup = Instantiate(prefab, spawnPosition, Quaternion.identity);
        pickup.Initialize(amount, landingPosition);
    }
}
