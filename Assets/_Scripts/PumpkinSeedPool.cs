using UnityEngine;
using UnityEngine.Pool;

// Pools PumpkinSeed instances instead of Instantiate/Destroy-ing one per shot. This is the
// only thing that knows pooling exists - ProjectileWeapon asks it for a seed instead of
// Instantiating the prefab directly, and PumpkinSeed itself has no idea it's pooled; it just
// fires OnDespawn when it's done, and this class is the one thing listening for that.
public class PumpkinSeedPool : MonoBehaviour
{
    [SerializeField] PumpkinSeed seedPrefab;
    [Tooltip("How many seeds to have ready before the pool ever needs to grow.")]
    [SerializeField] int defaultCapacity = 11;
    [Tooltip("Hard ceiling - beyond this, released seeds are destroyed instead of kept, so a " +
             "burst of shooting can't hold onto an unbounded number of idle seeds forever.")]
    [SerializeField] int maxSize = 100;

    ObjectPool<PumpkinSeed> pool;

    void Awake()
    {
        pool = new ObjectPool<PumpkinSeed>(
            createFunc: CreateSeed,
            actionOnGet: seed => seed.gameObject.SetActive(true),
            actionOnRelease: seed => seed.gameObject.SetActive(false),
            actionOnDestroy: seed => Destroy(seed.gameObject),
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    // ProjectileWeapon calls this instead of Instantiate(seedPrefab, ...).
    public PumpkinSeed Get(Vector3 position, Vector2 velocity)
    {
        PumpkinSeed seed = pool.Get();
        seed.transform.SetPositionAndRotation(position, Quaternion.identity);
        seed.Launch(velocity);
        return seed;
    }

    // Only called the first few times, when the pool is still growing toward defaultCapacity/
    // maxSize - not on every shot. OnDespawn is subscribed once here, for the object's whole
    // lifetime, since this one PumpkinSeed instance belongs to this one pool forever - unlike
    // OnEnable/OnDisable elsewhere in this project, there's no repeated subscribe/unsubscribe
    // needed here.
    PumpkinSeed CreateSeed()
    {
        PumpkinSeed seed = Instantiate(seedPrefab);
        seed.OnDespawn += HandleDespawn;
        return seed;
    }

    void HandleDespawn(PumpkinSeed seed)
    {
        pool.Release(seed);
    }
}
