



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] float rotateSpeed = 4f;

    Vector2 lookAtPoint;

    PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        RotatePlayer();
    }

    private void RotatePlayer()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var mousePositionDirection = mouseWorldPosition - transform.position;

        var angleToRotate = Mathf.Atan2(mousePositionDirection.y, mousePositionDirection.x) * Mathf.Rad2Deg;

        gameObject.transform.rotation = Quaternion.Euler(0, 0, angleToRotate - 90f);



    }
}
