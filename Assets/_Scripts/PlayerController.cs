

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotateSpeed = 2f;

    private Vector2 movementVector;

    private Rigidbody2D rb;

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
        //rb.velocity = new Vector2(movementVector.x * moveSpeed, movementVector.y * moveSpeed);
        rb.velocity = new Vector2(movementVector.x, movementVector.y).normalized * moveSpeed;
    }


}
