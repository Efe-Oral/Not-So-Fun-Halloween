

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotateSpeed = 2f;

    private Vector2 movementVector;

    private Rigidbody2D rb;

    // Read-only view of the current input, so other scripts (PlayerDash) can react to it
    // without each one separately calling Input.GetAxisRaw.
    public Vector2 MovementInput => movementVector;

    // While true, FixedUpdate skips setting velocity - lets another system (PlayerDash) take
    // exclusive control of the Rigidbody2D's velocity without this overwriting it every step.
    public bool MovementLocked { get; set; }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        movementVector.x = Input.GetAxisRaw("Horizontal");
        movementVector.y = Input.GetAxisRaw("Vertical");


    }

    private void FixedUpdate()
    {
        if (MovementLocked) return;

        //rb.velocity = new Vector2(movementVector.x * moveSpeed, movementVector.y * moveSpeed);
        rb.velocity = new Vector2(movementVector.x, movementVector.y).normalized * moveSpeed;
    }


}
