using UnityEngine;
using UnityEngine.UI;

// A world-space stamina bar, same tracking/detaching approach as HealthBarUI - floats below
// its target and never inherits the target's rotation or scale. Default offset sits it just
// under a HealthBarUI using that script's default offset (0, 1, 0).
//
// Unlike HealthBarUI, there's no punch/color-gradient feedback here: stamina regenerates
// continuously, every frame, so OnStaminaChanged fires constantly while regenerating - a punch
// effect on every one of those frames would look chaotic rather than juicy. Setting the fill
// value directly (no tween) already reads as smooth since the underlying value itself is
// changing smoothly.
public class StaminaBarUI : MonoBehaviour
{
    [Tooltip("Left empty, this looks on the parent hierarchy for a Stamina component.")]
    [SerializeField] Stamina stamina;
    [Tooltip("What to hover below. Left empty, this uses stamina's own transform.")]
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0f, 0.75f, 0f);

    [SerializeField] Slider slider;

    void Awake()
    {
        if (stamina == null) stamina = GetComponentInParent<Stamina>();
        if (target == null && stamina != null) target = stamina.transform;

        transform.SetParent(null, true);
    }

    void OnEnable()
    {
        if (stamina == null) return;
        stamina.OnStaminaChanged += HandleStaminaChanged;
    }

    // See HealthBarUI for why the initial value is read in Start, not OnEnable.
    void Start()
    {
        if (stamina != null) HandleStaminaChanged(stamina.CurrentStamina);
    }

    void OnDisable()
    {
        if (stamina == null) return;
        stamina.OnStaminaChanged -= HandleStaminaChanged;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = target.position + offset;
        transform.rotation = Quaternion.identity;
    }

    void HandleStaminaChanged(float current)
    {
        slider.value = stamina.MaxStamina > 0f ? current / stamina.MaxStamina : 0f;
    }
}
