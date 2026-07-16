using UnityEngine;
using DG.Tweening;

// Swings the sword with a DOTween sequence.
// Put this on a pivot object that is a child of the player, at the player's center,
// with the sword sprite as a child offset out from the pivot.
// The pivot inherits the player's aim (LookAtCamera), so tweening the pivot's LOCAL rotation
// sweeps the whole sword around the pivot as a swing, layered on top of that inherited aim.
public class SwordController : MonoBehaviour
{
    [SerializeField] SwordConfig config;

    [Header("Input")]
    [SerializeField] KeyCode swingKey = KeyCode.Mouse0;

    [Header("Hitbox")]
    [Tooltip("The sword's damage trigger. Left empty, it is found on the sword sprite child automatically.")]
    [SerializeField] SwordHitbox hitbox;

    bool isSwinging;
    Sequence swingSequence;

    void Awake()
    {
        // The SwordHitbox lives on the sword sprite, which is a child of this pivot.
        if (hitbox == null) hitbox = GetComponentInChildren<SwordHitbox>();
    }

    void Update()
    {
        if (!isSwinging && Input.GetKeyDown(swingKey))
        {
            Swing();
        }
    }

    void Swing()
    {
        isSwinging = true;
        if (hitbox != null) hitbox.BeginSwing();   // turn the damage trigger on for this swing

        float half = config.swingArc * 0.5f;

        swingSequence?.Kill();
        swingSequence = DOTween.Sequence();

        // Windup back, strike across, then recover to rest (local Z = 0).
        // Rotating the pivot sweeps the offset sword child around it.
        swingSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, half), config.windupDuration)
            .SetEase(config.windupEase));
        swingSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, -half), config.swingDuration)
            .SetEase(config.swingEase));
        swingSequence.Append(transform.DOLocalRotate(Vector3.zero, config.recoverDuration)
            .SetEase(config.recoverEase));

        if (config.cooldown > 0f) swingSequence.AppendInterval(config.cooldown);

        swingSequence.OnComplete(() =>
        {
            isSwinging = false;
            if (hitbox != null) hitbox.EndSwing();   // turn the damage trigger back off
        });
    }

    void OnDestroy()
    {
        swingSequence?.Kill();
    }
}
