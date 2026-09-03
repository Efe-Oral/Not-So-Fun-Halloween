using UnityEngine;

// The enemy "brain": a small state machine that patrols, spots the player, chases,
// and (from Phase 2 on) attacks. It reads all its numbers from an EnemyConfig, so
// Easy/Medium/Hard are just different config assets plugged into this same script.
//
// Movement uses Rigidbody2D velocity (same as the player) with lightweight "whisker"
// raycasts to steer around walls. Requires a Rigidbody2D and a Health on the enemy.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    enum State { Patrol, Chase, Attack, Dead }

    [Header("Config (which difficulty tier this is)")]
    [SerializeField] EnemyConfig config;

    // Lets other enemy scripts (like EnemyAttack) read the same tier settings
    // without you having to drag the config asset onto every component separately.
    public EnemyConfig Config => config;

    [Header("Obstacle Avoidance")]
    [Tooltip("Which layers count as walls to steer around. Leave empty for now if your " +
             "buildings don't have a dedicated layer yet - the enemy just won't avoid them.")]
    [SerializeField] LayerMask obstacleMask;
    [Tooltip("How far ahead the enemy 'feels' for walls.")]
    [SerializeField] float whiskerLength = 1.2f;

    Rigidbody2D rb;
    Health health;
    Transform player;
     Collider2D collider;

    State state;
    Vector2 home;            // where we started - the center of our patrol area
    Vector2 patrolTarget;    // the spot we're currently wandering toward
    float patrolWaitTimer;   // counts down while we pause between wander hops

    // While this is above zero, the enemy is "angry": it chases no matter the distance
    // and refuses to give up. Getting hit sets it to config.aggroDuration. This is what
    // makes a slingshot shot from across the map pull the enemy toward you.
    float aggroTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        collider = GetComponent<Collider2D>();
    }

    void OnEnable()
    {
        // when OnDamaged event fires, run HandleDamaged method
        health.OnDamaged += HandleDamaged;
        health.OnDied += HandleDied;
    }

    void OnDisable()
    {
        health.OnDamaged -= HandleDamaged;
        health.OnDied -= HandleDied;
    }

    void Start()
    {
        // Let the config drive our HP, so Hard enemies really are tougher.
        if (config != null)
        {
            health.SetMaxHealth(config.maxHealth);
        }

        home = rb.position;
        PickNewPatrolTarget();

        // Find the player by tag (make sure your player object is tagged "Player").
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        state = State.Patrol;
    }

    // Physics + movement live in FixedUpdate. We also decide state transitions here,
    // since they depend on distances we're already computing for movement.
    void FixedUpdate()
    {
        if (state == State.Dead || config == null) return;

        // Tick the anger timer down.
        if (aggroTimer > 0f) aggroTimer -= Time.fixedDeltaTime;

        float distToPlayer = player != null
            ? Vector2.Distance(rb.position, player.position)
            : Mathf.Infinity;   // no player found -> treat as infinitely far, just patrol

        UpdateStateTransitions(distToPlayer);
        Act(distToPlayer);
    }

    // Decide whether to switch states based on distance to the player and the aggro timer.
    void UpdateStateTransitions(float distToPlayer)
    {
        switch (state)
        {
            case State.Patrol:
                // Spotted the player normally, OR we were just hit -> chase.
                if (distToPlayer <= config.detectionRange || aggroTimer > 0f)
                    state = State.Chase;
                break;

            case State.Chase:
                // Give up ONLY if the player is far AND we're no longer angry. While the
                // aggro timer is running we keep chasing even past loseSightRange.
                if (distToPlayer > config.loseSightRange && aggroTimer <= 0f)
                {
                    state = State.Patrol;
                    home = rb.position;
                    PickNewPatrolTarget();
                }
                // Close enough to attack -> switch to attack.
                else if (distToPlayer <= config.attackRange)
                    state = State.Attack;
                break;

            case State.Attack:
                // Player stepped out of attack range -> resume chasing.
                if (distToPlayer > config.attackRange)
                    state = State.Chase;
                break;
        }
    }

    // Do the thing the current state calls for.
    void Act(float distToPlayer)
    {
        switch (state)
        {
            case State.Patrol:
                DoPatrol();
                break;

            case State.Chase:
                MoveToward(player.position, config.moveSpeed);
                break;

            case State.Attack:
                // Phase 2 fills this in with a real windup + strike / projectile.
                // For now, hold position at the player.
                rb.velocity = Vector2.zero;
                break;
        }
    }

    // Wander to a random spot near home, pause, then pick a new spot.
    void DoPatrol()
    {
        // Pausing between hops?
        if (patrolWaitTimer > 0f)
        {
            patrolWaitTimer -= Time.fixedDeltaTime;
            rb.velocity = Vector2.zero;
            return;
        }

        // Arrived at the current target? Pause, then choose the next one.
        if (Vector2.Distance(rb.position, patrolTarget) <= 0.15f)
        {
            patrolWaitTimer = Random.Range(config.patrolWaitMin, config.patrolWaitMax);
            PickNewPatrolTarget();
            rb.velocity = Vector2.zero;
            return;
        }

        MoveToward(patrolTarget, config.moveSpeed * config.patrolSpeedMultiplier);
    }

    void PickNewPatrolTarget()
    {
        // A random point inside a circle of radius patrolRadius around home.
        patrolTarget = home + Random.insideUnitCircle * config.patrolRadius;
    }

    // Steer toward a destination at a given speed, nudging around walls on the way.
    void MoveToward(Vector2 destination, float speed)
    {
        Vector2 desired = (destination - rb.position).normalized;
        Vector2 steered = AvoidObstacles(desired);
        rb.velocity = steered * speed;
    }

    // "Whisker" avoidance: feel ahead with a ray; if a wall is there, try angled rays
    // left/right and go whichever way is clear. Cheap, and good enough for open areas.
    Vector2 AvoidObstacles(Vector2 desired)
    {
        // Nothing set as an obstacle? Then there's nothing to avoid - go straight.
        if (obstacleMask.value == 0) return desired;

        RaycastHit2D ahead = Physics2D.Raycast(rb.position, desired, whiskerLength, obstacleMask);
        if (ahead.collider == null) return desired;   // path ahead is clear

        Vector2 left = Rotate(desired, 45f);
        Vector2 right = Rotate(desired, -45f);

        bool leftClear = Physics2D.Raycast(rb.position, left, whiskerLength, obstacleMask).collider == null;
        if (leftClear) return left;

        bool rightClear = Physics2D.Raycast(rb.position, right, whiskerLength, obstacleMask).collider == null;
        if (rightClear) return right;

        // Boxed in on both sides - slide along the wall we hit.
        return ahead.normal;
    }

    // Rotate a 2D vector by an angle in degrees.
    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }

    // Any damage makes us angry for a while. Because this reacts to the Health event, a hit
    // from ANY source (slingshot, sword, a future trap) triggers the chase - even from far away.
    void HandleDamaged(float amount)
    {
        // If we're already dead, ignore the hit.
        if (state == State.Dead) return;

        // No config assigned? We don't know how long to stay angry, so stop here.
        if (config == null) return;

        // Getting hit makes us angry: chase for aggroDuration seconds.
        aggroTimer = config.aggroDuration;
    }

    void HandleDied()
    {
        state = State.Dead;
        rb.velocity = Vector2.zero;
        collider.enabled = false;
    }

    // Draw the detection/attack ranges in the Scene view when the enemy is selected,
    // so you can tune them visually. (Editor-only; costs nothing in the build.)
    void OnDrawGizmosSelected()
    {
        if (config == null) return;

        Gizmos.color = Color.yellow;                       // spot the player
        Gizmos.DrawWireSphere(transform.position, config.detectionRange);

        Gizmos.color = new Color(1f, 0.6f, 0f);            // give up the chase
        Gizmos.DrawWireSphere(transform.position, config.loseSightRange);

        Gizmos.color = Color.red;                          // attack range
        Gizmos.DrawWireSphere(transform.position, config.attackRange);
    }
}
