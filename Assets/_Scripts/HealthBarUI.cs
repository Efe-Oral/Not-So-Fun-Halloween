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

    [Header("Color by health (evaluated left-to-right as health goes from 0 to full)")]
    [SerializeField] Gradient colorByHealth = DefaultGradient();

    [Header("Hit feedback")]
    [SerializeField] float punchScale = 0.15f;
    [SerializeField] float punchDuration = 0.15f;

    Tween fillTween;
    Tween colorTween;
    Image fillImage;

    void Awake()
    {
        if (health == null) health = GetComponentInParent<Health>();
        if (target == null && health != null) target = health.transform;
        if (slider != null && slider.fillRect != null) fillImage = slider.fillRect.GetComponent<Image>();

        transform.SetParent(null, true);
    }

    void OnEnable()
    {
        if (health == null) return;
        health.OnDamaged += HandleDamaged;
        health.OnDied += HandleDied;
    }

    // Reads the initial health ratio here rather than in OnEnable - Unity doesn't guarantee
    // Health.Awake() (which sets CurrentHealth = maxHealth) runs before this object's OnEnable,
    // since they're on different GameObjects. Every Awake in the scene is guaranteed to finish
    // before any Start runs, so this is the first point where reading CurrentHealth is safe.
    void Start()
    {
        ApplyRatio(SafeRatio(), animate: false);
    }

    void OnDisable()
    {
        if (health == null) return;
        health.OnDamaged -= HandleDamaged;
        health.OnDied -= HandleDied;
        fillTween?.Kill();
        colorTween?.Kill();
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
        ApplyRatio(SafeRatio(), animate: true);

        transform.DOKill(true);
        transform.DOPunchScale(Vector3.one * punchScale, punchDuration, 4, 0.5f);
    }

    void HandleDied()
    {
        Destroy(gameObject);
    }

    // Updates both the fill amount and its color together, since they represent the same
    // piece of information (current health ratio) and should never show conflicting values.
    void ApplyRatio(float ratio, bool animate)
    {
        Color targetColor = colorByHealth.Evaluate(ratio);

        fillTween?.Kill();
        colorTween?.Kill();

        if (animate)
        {
            fillTween = slider.DOValue(ratio, fillTweenDuration);
            if (fillImage != null) colorTween = fillImage.DOColor(targetColor, fillTweenDuration);
        }
        else
        {
            slider.value = ratio;
            if (fillImage != null) fillImage.color = targetColor;
        }
    }

    float SafeRatio()
    {
        return health.MaxHealth > 0f ? health.CurrentHealth / health.MaxHealth : 0f;
    }

    // Green at full health, through yellow, to red at empty - a sane default so the field
    // isn't blank until someone customizes it in the Inspector.
    static Gradient DefaultGradient()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.85f, 0.15f, 0.15f), 0f),
                new GradientColorKey(new Color(0.95f, 0.85f, 0.2f), 0.5f),
                new GradientColorKey(new Color(0.2f, 0.8f, 0.3f), 1f),
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        return gradient;
    }
}
