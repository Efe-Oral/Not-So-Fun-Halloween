using UnityEngine;

// Changes an enemy's look on death: optionally swaps its sprite to dedicated death art, and
// always tints it a death color. The sprite swap is optional (leave deadSprite empty) so
// enemies without death art yet still get a color change, while ones that do have it (Jason,
// Ghostface) get both from the same script. Purely reactive to Health.OnDied, same pattern as
// EnemyHitFlash - doesn't touch movement/collision, EnemyAI.HandleDied already owns that.
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyDeathVisual : MonoBehaviour
{
    [Tooltip("Optional - leave empty if this enemy doesn't have dedicated death art yet. " +
             "It'll still get the death color below.")]
    [SerializeField] Sprite deadSprite;
    [SerializeField] Color deadColor = Color.gray;

    SpriteRenderer spriteRenderer;
    Health health;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Health may sit on this object or on a parent - look in both places.
        health = GetComponentInParent<Health>();
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
        
        if (deadSprite != null)
        {
            spriteRenderer.sprite = deadSprite;
        }   
        spriteRenderer.color = deadColor;

    }
}
