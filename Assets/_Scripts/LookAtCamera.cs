using UnityEngine;

// Rotates the player to face the mouse. Reads the mouse every frame (Update) for full
// responsiveness, but applies the rotation via Rigidbody2D.MoveRotation in FixedUpdate instead
// of writing transform.rotation directly. Position already goes through the physics/
// interpolation pipeline (rb.velocity in PlayerController.FixedUpdate); a direct rotation
// write bypasses that pipeline entirely, so with Interpolate on, position and rotation were
// being smoothed on two different, conflicting channels - that mismatch is what caused the
// jumping. Routing rotation through the same physics channel keeps both consistent.
//
// Stays on the Player root, same object as before - no hierarchy change, so sword/weapon aim
// (which inherits this object's rotation) is unaffected.
[RequireComponent(typeof(Rigidbody2D))]
public class LookAtCamera : MonoBehaviour
{
    Rigidbody2D rb;
    float targetAngle;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mouseWorldPosition - transform.position;
        targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
    }

    void FixedUpdate()
    {
        rb.MoveRotation(targetAngle);
    }
}
