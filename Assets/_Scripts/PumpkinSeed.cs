using UnityEngine;

// A pumpkin seed. The ProjectileWeapon sets its speed when it spawns;
// this script just deletes it after a few seconds so seeds don't pile up forever.
public class PumpkinSeed : MonoBehaviour
{
    [SerializeField] float lifetime = 3f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
