using UnityEngine;
using TMPro;
using DG.Tweening;

// A floating damage number that pops in, rises, fades, and destroys itself. Only knows how
// to animate itself once told what number to show (via Show) - DamagePopupSpawner decides
// when and where to create one, this script doesn't know who dealt or took the damage.
//
// Uses TMP_Text (the common base class of both TextMeshPro variants) rather than naming
// either concrete type, and GetComponentInChildren rather than GetComponent - so this works
// whether the actual text sits on this object or on a child (e.g. a World Space Canvas's
// child Text), and whether it's the 3D TextMeshPro or the UI TextMeshProUGUI variant. In this
// project the UI variant (under a World Space Canvas, same approach as HealthBarUI) is the
// one that actually renders correctly - the 3D variant needs its own sorting-layer setup to
// draw above 2D sprites, which it doesn't get by default.
public class DamagePopup : MonoBehaviour
{
    [SerializeField] float riseDistance = 1f;
    [SerializeField] float duration = 0.8f;
    [SerializeField] float popScale = 1.3f;
    [SerializeField] float popDuration = 0.15f;

    [Header("Appearance")]
    [SerializeField] Color textColor = Color.white;
    [SerializeField] Color outlineColor = Color.black;
    [Tooltip("0 = no outline. TMP outline width is a fraction of the font's SDF range, so " +
             "small values (0.1-0.3) are usually enough.")]
    [SerializeField, Range(0f, 1f)] float outlineWidth = 0.2f;

    TMP_Text text;

    void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
    }

    public void Show(float amount)
    {
        // Outline setup lives here, not Awake - Show() is only ever called right after
        // Instantiate() returns, which Unity guarantees is after every Awake/OnEnable on the
        // new hierarchy has already run (including TMP_Text's own internal setup). Doing this
        // in Awake risked running before TMP's own Awake, on a different GameObject, in
        // whichever order Unity happened to pick - the same class of bug HealthBarUI had.
        text.outlineWidth = outlineWidth;
        text.outlineColor = outlineColor;

        // An outline stroke extends past a glyph's normal quad. Without extraPadding, TMP
        // doesn't allocate that extra room in the mesh, so the outline gets generated but
        // clipped away right at the glyph edge - invisible, even though the material is
        // correctly set. UpdateMeshPadding recalculates the mesh now that outlineWidth is set.
        text.extraPadding = true;
        text.UpdateMeshPadding();

        text.text = Mathf.RoundToInt(amount).ToString();

        transform.localScale = Vector3.zero;
        text.color = new Color(textColor.r, textColor.g, textColor.b, 1f);

        transform.DOScale(popScale, popDuration).SetEase(Ease.OutBack)
            .OnComplete(() => transform.DOScale(1f, popDuration * 0.5f));

        transform.DOMoveY(transform.position.y + riseDistance, duration).SetEase(Ease.OutQuad);

        // DOVirtual.Float drives the fade manually (core DOTween, no module needed) since this
        // project's DOTween install doesn't have the TextMeshPro module - see CLAUDE.md.
        DOVirtual.Float(1f, 0f, duration, v =>
        {
            Color col = text.color;
            text.color = new Color(col.r, col.g, col.b, v);
        }).OnComplete(() => Destroy(gameObject));
    }
}
