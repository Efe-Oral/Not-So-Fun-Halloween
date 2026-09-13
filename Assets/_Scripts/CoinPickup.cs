using System;
using UnityEngine;
using DG.Tweening;

// A dropped coin/candy sitting in the world. EnemyCoinDrop sets its amount and landing spot via
// Initialize right after spawning it, which also kicks off a little pop-up-then-fall arc via
// DOJump - it shoots up, then comes down at the landing position instead of just appearing
// there. Fires a static event on collection rather than needing a direct reference to whatever
// tracks the total - any number of listeners (a wallet, a UI, both) can react without this
// script knowing they exist.
//
// Its collider should have "Is Trigger" checked so touching it (not colliding with it) counts
// as collecting it.
public class CoinPickup : MonoBehaviour
{
    [SerializeField] int amount;

    [Header("Drop animation")]
    [SerializeField] float jumpPower = 1f;
    [SerializeField] float dropDuration = 0.4f;

    public static event Action<int> OnCollected;

    public void Initialize(int coinAmount, Vector3 landingPosition)
    {
        amount = coinAmount;
        transform.DOJump(landingPosition, jumpPower, numJumps: 1, duration: dropDuration)
            .SetEase(Ease.OutQuad);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        OnCollected?.Invoke(amount);
        Destroy(gameObject);
    }
}
