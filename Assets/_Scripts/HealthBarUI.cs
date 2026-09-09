using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// A world-space health bar that floats above a target and never inherits its rotation or
// scale. It's convenient to nest this as a child of an enemy/player prefab for authoring
// (drag it into place once, in the Prefab editor) - but it detaches itself from that parent
// at runtime, so the target's own scale (EnemyConfig's scaleX/Y/Z) or rotation (the player
// spinning to face the mouse via LookAtCamera) never distorts or spins the bar.
//
// Purely reactive to Health's events, same pattern as EnemyHitFlash - works on any object
// with a Health component, enemy or player, with zero per-instance wiring beyond dragging
// this prefab into place.
public class HealthBarUI : MonoBehaviour
{
    [Tooltip("Left empty, this looks on the parent hierarchy for a Health component.")]
    [SerializeField] Health health;
    [Tooltip("What to hover above. Left empty, this uses health's own transform.")]
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0f, 1f, 0f);

    [Header("Fill")]
    [SerializeField] Slider slider;
    [SerializeField] float fillTweenDuration = 0.2f;

    [Header("Hit feedback")]
    [SerializeField] float punchScale = 0.15f;
    [SerializeField] float punchDuration = 0.15f;

    Tween fillTween;

    void Awake()
    {
        if (health == null) health = GetComponentInParent<Health>();
        if (target == null && health != null) target = health.transform;

        transform.SetParent(null, true);
    }

    void OnEnable()
    {
        if (health == null) return;
        health.OnDamaged += HandleDamaged;
        health.OnDied += HandleDied;
        slider.value = SafeRatio();
    }

    void OnDisable()
    {
        if (health == null) return;
        health.OnDamaged -= HandleDamaged;
        health.OnDied -= HandleDied;
        fillTween?.Kill();
    }

    void LateUpdate()
    {
        // Target destroyed out from under us (e.g. the enemy GameObject was removed directly
        // instead of going through Health.OnDied) - don't float in place forever.
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = target.position + offset;
        transform.rotation = Quaternion.identity;
    }

    void HandleDamaged(float amount)
    {
        fillTween?.Kill();
        fillTween = slider.DOValue(SafeRatio(), fillTweenDuration);

        transform.DOKill(true);
        transform.DOPunchScale(Vector3.one * punchScale, punchDuration, 4, 0.5f);
    }

    void HandleDied()
    {
        Destroy(gameObject);
    }

    float SafeRatio()
    {
        return health.MaxHealth > 0f ? health.CurrentHealth / health.MaxHealth : 0f;
    }
}
