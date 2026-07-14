using UnityEngine;
using DG.Tweening;

// Tunable sword values. Create one via Assets > Create > Halloween > Sword Config,
// then assign it on the SwordController. Everything here can be changed without touching code.
[CreateAssetMenu(fileName = "SwordConfig", menuName = "Halloween/Sword Config")]
public class SwordConfig : ScriptableObject
{
    [Header("Swing Shape")]
    [Tooltip("Total angle the sword sweeps through, in degrees. Half winds up, half follows through.")]
    public float swingArc = 140f;

    [Header("Windup (pull back)")]
    public float windupDuration = 0.06f;
    public Ease windupEase = Ease.OutQuad;

    [Header("Swing (the strike)")]
    public float swingDuration = 0.12f;
    public Ease swingEase = Ease.OutCubic;

    [Header("Recover (back to rest)")]
    public float recoverDuration = 0.12f;
    public Ease recoverEase = Ease.InOutQuad;

    [Header("Cooldown")]
    [Tooltip("Extra time after the swing before another swing is allowed.")]
    public float cooldown = 0.05f;
}
