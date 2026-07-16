using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

// Put this on the enemy (Jason), on the same object as its SpriteRenderer.
// Call Flash() to make the enemy quickly blink white and then return to normal.
public class EnemyHitFlash : MonoBehaviour
{
    [SerializeField] Color flashColor = Color.white;
    [SerializeField] float flashDuration = 0.1f;

    SpriteRenderer spriteRenderer;
    Color originalColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;   // remember the real color once, at the start
    }

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
