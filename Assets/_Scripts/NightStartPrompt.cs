using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// Shows a bobbing "press space to start the night" sprite and gates NightManager behind it -
// nothing spawns/counts down until the player presses the key. Once pressed, fades itself out
// and calls NightManager.BeginNight(). NightManager doesn't reference this script at all, so
// the prompt could be swapped for a UI button later without touching NightManager.
public class NightStartPrompt : MonoBehaviour
{
    [SerializeField] NightManager nightManager;
    [SerializeField] Image promptImage;
    [SerializeField] KeyCode startKey = KeyCode.Space;

    [Header("Floating animation")]
    [SerializeField] float floatDistance = 15f;
    [SerializeField] float floatDuration = 0.8f;

    [Header("Dismiss animation")]
    [SerializeField] float fadeDuration = 0.3f;

    Tween floatTween;
    bool started;

    void OnEnable()
    {
        started = false;
        Color c = promptImage.color;
        promptImage.color = new Color(c.r, c.g, c.b, 1f);

        Vector3 startPos = promptImage.transform.localPosition;
        floatTween = promptImage.transform
            .DOLocalMoveY(startPos.y + floatDistance, floatDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    void OnDisable()
    {
        floatTween?.Kill();
    }

    void Update()
    {
        if (started) return;
        if (Input.GetKeyDown(startKey)) Begin();
    }

    void Begin()
    {
        started = true;
        floatTween?.Kill();

        // Deactivate promptImage's own GameObject, not this.gameObject - if this script ends
        // up sharing a controller object with something else (e.g. WaveAnnouncerUI), disabling
        // this.gameObject would take that sibling component down with it and silently kill its
        // event subscriptions.
        promptImage.DOFade(0f, fadeDuration).OnComplete(() => promptImage.gameObject.SetActive(false));
        enabled = false;
        nightManager.BeginNight();
    }
}
