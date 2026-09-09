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

    TMP_Text text;

    void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
    }

    public void Show(float amount)
    {
        text.text = Mathf.RoundToInt(amount).ToString();

        transform.localScale = Vector3.zero;
        Color c = text.color;
        text.color = new Color(c.r, c.g, c.b, 1f);

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
