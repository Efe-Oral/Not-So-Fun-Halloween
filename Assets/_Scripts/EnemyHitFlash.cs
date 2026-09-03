using System.Collections;
using UnityEngine;

// Makes the enemy blink white for a moment whenever it takes damage.
// Put this on the enemy, on the same object as its SpriteRenderer.
//
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyHitFlash : MonoBehaviour
{
    [SerializeField] Color flashColor = Color.white;
    [SerializeField] float flashDuration = 0.1f;

    SpriteRenderer spriteRenderer;
    Color originalColor;
    Health health;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;   // remember the real color once, at the start

        // Health may sit on this object or on a parent - look in both places.
        health = GetComponentInParent<Health>();
    }

    // Subscribe/unsubscribe in OnEnable/OnDisable so we never leak a listener
    // or react after this object is turned off.
    void OnEnable()
    {
        if (health != null) health.OnDamaged += HandleDamaged;
    }

    void OnDisable()
    {
        if (health != null) health.OnDamaged -= HandleDamaged;
    }

    // Health passes the damage amount; we don't need it here, just the fact that we were hit.
    void HandleDamaged(float amount) => Flash();

    // Blink white, then go back to the original color.
    public void Flash()
    {
        // Stop any flash already playing so a second hit restarts cleanly.
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        spriteRenderer.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0.8f);
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}
