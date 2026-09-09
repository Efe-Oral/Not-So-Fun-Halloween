using UnityEngine;

// Spawns a floating damage number above whatever Health this is attached to (or nested
// under) every time it takes damage. Purely reactive, same pattern as EnemyHitFlash - can be
// added to or removed from any damageable object (enemy or player) without touching Health
// or whatever dealt the damage.
public class DamagePopupSpawner : MonoBehaviour
{
    [Tooltip("Left empty, this looks on the parent hierarchy for a Health component.")]
    [SerializeField] Health health;
    [SerializeField] DamagePopup popupPrefab;
    [SerializeField] Vector3 spawnOffset = new Vector3(0f, 1f, 0f);
    [Tooltip("Random horizontal spread so rapid hits don't stack numbers exactly on top of each other.")]
    [SerializeField] float horizontalJitter = 0.3f;

    [SerializeField] Transform locationOfDamageNumbers;

    void Awake()
    {
        if (health == null) health = GetComponentInParent<Health>();
    }

    void OnEnable()
    {
        if (health != null) health.OnDamaged += HandleDamaged;
    }

    void OnDisable()
    {
        if (health != null) health.OnDamaged -= HandleDamaged;
    }

    void HandleDamaged(float amount)
    {
        if (popupPrefab == null) return;

        // Spawned with no parent - it's a self-contained, momentary effect, so it should
        // never inherit the target's rotation or scale (e.g. the player spinning via
        // LookAtCamera) the way a persistent child object would.
        Vector3 jitter = new Vector3(Random.Range(-horizontalJitter, horizontalJitter), 0f, 0f);
        DamagePopup popup = Instantiate(popupPrefab, new Vector3(locationOfDamageNumbers.transform.position.x,locationOfDamageNumbers.transform.position.y,locationOfDamageNumbers.transform.position.z)
        + spawnOffset + jitter, Quaternion.identity);
        popup.Show(amount);
    }
}
