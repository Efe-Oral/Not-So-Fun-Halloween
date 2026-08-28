using UnityEngine;
using DG.Tweening;

// Drives the player's movement-state visual effects: a "breathing" squash-stretch
// loop plus ambient embers while standing still, and a dust trail while moving.
// Everything here is just two mirrored reactions to one Rigidbody2D-speed check, so
// it lives in one script rather than splitting idle-FX and move-FX into separate
// files that would both re-implement the same threshold test.
//
// Put this on the Player root (it reads the player's own Rigidbody2D for speed).
// It tweens the VISUAL child's scale, not the Player root's - the root also carries
// the BoxCollider2D, and Unity scales colliders along with their transform, so
// pulsing the root would make the hitbox breathe in sync with the sprite.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerIdleAnimator : MonoBehaviour
{
    [Header("What to animate")]
    [SerializeField] Transform visual;              // child holding the SpriteRenderer (squash target)
    [SerializeField] ParticleSystem idleParticles;   // ambient embers, only emit while idle
    [SerializeField] ParticleSystem moveDust;        // footstep dust, only emits while moving

    [Header("Idle Detection")]
    [Tooltip("Below this speed (units/sec) the player counts as 'idle'.")]
    [SerializeField] float moveThreshold = 0.05f;

    [Header("Squash & Stretch")]
    [Tooltip("How far the scale swings from 1.0 on each axis (e.g. 0.08 = 1.08/0.92).")]
    [SerializeField] float squashAmount = 0.08f;
    [Tooltip("Duration of one half of the pulse (windup OR release), in seconds.")]
    [SerializeField] float pulseDuration = 0.6f;
    [SerializeField] Ease pulseEase = Ease.InOutSine;

    Rigidbody2D rb;
    Tween pulseTween;
    bool isIdle;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (visual != null) visual.localScale = Vector3.one;
    }

    void Update()
    {
        bool nowIdle = rb.velocity.sqrMagnitude <= moveThreshold * moveThreshold;

        if (nowIdle && !isIdle) StartIdle();
        else if (!nowIdle && isIdle) StopIdle();

        isIdle = nowIdle;
    }

    void StartIdle()
    {
        if (idleParticles != null) idleParticles.Play();
        if (moveDust != null) moveDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (visual == null) return;

        pulseTween?.Kill();
        pulseTween = visual
            .DOScale(new Vector3(1f + squashAmount, 1f - squashAmount, 1f), pulseDuration)
            .SetEase(pulseEase)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // Called the instant the player starts moving (i.e. idle just ended).
    void StopIdle()
    {
        // Let particles already in flight finish naturally instead of vanishing.
        if (idleParticles != null) idleParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if (moveDust != null) moveDust.Play();

        pulseTween?.Kill();
        pulseTween = null;
        if (visual != null) visual.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutQuad);
    }

    void OnDestroy()
    {
        pulseTween?.Kill();
    }
}
