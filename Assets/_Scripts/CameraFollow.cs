using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] float smoothTime = 0.12f;

    Vector3 smoothVelocity;

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 goal = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, goal, ref smoothVelocity, smoothTime);
    }
}
