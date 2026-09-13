using System.Collections;
using UnityEngine;

// Dashes the player a fixed, configurable distance in the direction of current movement input
// (WASD), falling back to the last direction they moved in if currently standing still. Costs
// stamina - can't dash without enough (Stamina.TrySpend checks and consumes atomically).
//
// Takes exclusive control of the Rigidbody2D's velocity for the dash's duration via
// PlayerController.MovementLocked, so PlayerController's own FixedUpdate doesn't immediately
// overwrite the dash's velocity on the very next physics step.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour
{
    [SerializeField] KeyCode dashKey = KeyCode.Space;
    [Tooltip("How far the dash travels, in world units, under unobstructed conditions - it's " +
             "still a real physics-driven movement (via velocity), so a wall in the way stops " +
             "it short rather than letting it teleport through.")]
    [SerializeField] float dashDistance = 4f;
    [SerializeField] float dashDuration = 0.15f;
    [SerializeField] float staminaCost = 30f;

    [Tooltip("Left empty, this looks for a PlayerController on the same object.")]
    [SerializeField] PlayerController playerController;
    [Tooltip("Left empty, this looks for a Stamina on the same object.")]
    [SerializeField] Stamina stamina;

    Rigidbody2D rb;
    Vector2 lastMoveDirection = Vector2.right;
    bool isDashing;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (playerController == null) playerController = GetComponent<PlayerController>();
        if (stamina == null) stamina = GetComponent<Stamina>();
    }

    void Update()
    {
        if (playerController.MovementInput.sqrMagnitude > 0.01f)
            lastMoveDirection = playerController.MovementInput.normalized;

        if (!isDashing && Input.GetKeyDown(dashKey))
            TryDash();
    }

    void TryDash()
    {
        if (stamina != null && !stamina.TrySpend(staminaCost)) return;
        StartCoroutine(DashRoutine(lastMoveDirection));
    }

    IEnumerator DashRoutine(Vector2 direction)
    {
        isDashing = true;
        playerController.MovementLocked = true;

        float dashSpeed = dashDistance / dashDuration;
        rb.velocity = direction * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.velocity = Vector2.zero;
        playerController.MovementLocked = false;
        isDashing = false;
    }
}
