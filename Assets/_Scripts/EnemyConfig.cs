using UnityEngine;

// How this tier attacks. Only used starting in Phase 2 - the AI already reads it now
// so we don't have to re-edit the config assets later.
public enum EnemyAttackType
{
    ContactMelee,   // hurts the player just by touching them (Easy)
    LungeMelee,     // winds up, then dashes in for a strike (Medium)
    Ranged          // winds up, then fires a projectile from range (Hard)
}

// All the tunable numbers for ONE enemy difficulty tier. Create one via
// Assets > Create > Halloween > Enemy Config, then drop it onto an enemy's EnemyAI.
// Make three of these (Easy / Medium / Hard) - same AI script, different numbers.
[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Halloween/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Just a label to keep your assets straight, e.g. Easy / Medium / Hard.")]
    public string tierName = "Easy";

    [Header("Health")]
    public float maxHealth = 3f;

    [Header("Movement")]
    [Tooltip("Speed (units/sec) while chasing the player.")]
    public float moveSpeed = 2f;
    [Tooltip("Patrol speed as a fraction of moveSpeed. Enemies wander slower than they chase.")]
    [Range(0f, 1f)] public float patrolSpeedMultiplier = 0.5f;

    [Header("Patrol")]
    [Tooltip("How far from its starting spot the enemy wanders while patrolling.")]
    public float patrolRadius = 3f;
    [Tooltip("Random pause (seconds) between wander hops.")]
    public float patrolWaitMin = 0.5f;
    public float patrolWaitMax = 2f;

    [Header("Detection")]
    [Tooltip("The player is spotted within this distance -> start chasing.")]
    public float detectionRange = 5f;
    [Tooltip("Give up the chase once the player gets farther than this. Kept LARGER than " +
             "detectionRange so the enemy doesn't flicker between patrol/chase at the edge.")]
    public float loseSightRange = 7f;

    [Header("Aggro (getting hit)")]
    [Tooltip("When the enemy takes damage - even sniped from outside its detection range - it " +
             "chases for this many seconds before it's allowed to give up and return to patrol.")]
    public float aggroDuration = 5f;

    [Header("Attack (behavior added in Phase 2)")]
    public EnemyAttackType attackType = EnemyAttackType.ContactMelee;
    [Tooltip("Enemy stops approaching and switches to its attack once within this distance.")]
    public float attackRange = 1f;
}
