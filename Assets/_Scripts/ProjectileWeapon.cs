using UnityEngine;

// Shoots a pumpkin seed toward the mouse when you left-click.
// This only runs while the GameObject is active, so switching to the sword (slot 1)
// automatically stops it from shooting.
public class ProjectileWeapon : MonoBehaviour
{
    [SerializeField] GameObject seedPrefab;  // the pumpkin seed to spawn
    [SerializeField] float seedSpeed = 12f;

    [SerializeField] WeaponInventory weaponInventory;


    void Update()
    {
        if (Input.GetMouseButtonDown(0) && weaponInventory.isSlingshotEquipped)  // 0 = left mouse button
            Shoot();
    }

    void Shoot()
    {
        // Figure out where the mouse is in the game world.
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // Direction from the player to the mouse.
        Vector2 direction = (mousePosition - transform.position).normalized;

        // Create the seed at the player, then push it toward the mouse.
        GameObject seed = Instantiate(seedPrefab, transform.position, Quaternion.identity);
        seed.GetComponent<Rigidbody2D>().velocity = direction * seedSpeed;
    }
}
