using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Card : MonoBehaviour
{
    public int cardId;
    public RectTransform cardVisual;    // Assign this to CardVisual child
    public Image frontImage;
    public Image backImage;
    public bool isFlipped = false;
    public bool isMatched = false;
    public float flipDuration = 0.3f;
    private bool isAnimating = false;
    private CanvasGroup group;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>(); // Fallback for old instances
    }

    public void OnCardClicked()
    {
        if (isMatched || isAnimating) return;
        if (GameManager.Instance.IsInputLocked) return;
        if (isFlipped) return;
        GameManager.Instance.PlayFlipSound();
        Flip();
        GameManager.Instance.OnCardFlipped(this);
    }

    public void Flip()
    {
        if (isAnimating) return;
        StartCoroutine(FlipAnimation());
    }

    private IEnumerator FlipAnimation()
    {
        isAnimating = true;
        float time = 0f;
        Quaternion startRot = cardVisual.localRotation;
        Quaternion midRot = Quaternion.Euler(0, 90, 0);

        // Rotate to 90°
        while (time < flipDuration / 2)
        {
            float t = time / (flipDuration / 2);
            cardVisual.localRotation = Quaternion.Lerp(startRot, midRot, t);
            time += Time.deltaTime;
            yield return null;
        }
        cardVisual.localRotation = midRot;

        // Toggle image
        isFlipped = !isFlipped;
        frontImage.gameObject.SetActive(isFlipped);
        backImage.gameObject.SetActive(!isFlipped);

        // Rotate back to 0° or 180°
        time = 0f;
        Quaternion endRot = Quaternion.Euler(0, 0, 0);
        while (time < flipDuration / 2)
        {
            float t = time / (flipDuration / 2);
            cardVisual.localRotation = Quaternion.Lerp(midRot, endRot, t);
            time += Time.deltaTime;
            yield return null;
        }
        cardVisual.localRotation = endRot;

        // Always set correct images at the end of the animation (safety!)
        frontImage.gameObject.SetActive(isFlipped);
        backImage.gameObject.SetActive(!isFlipped);

        isAnimating = false;
    }

    public void SetMatched()
    {
        isMatched = true;
        StartCoroutine(FadeOutAndDisable());
    }

    private IEnumerator FadeOutAndDisable()
    {
        float fadeTime = 0.3f;
        float startAlpha = group.alpha;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            group.alpha = Mathf.Lerp(startAlpha, 0, t / fadeTime);
            yield return null;
        }
        group.alpha = 0;
        gameObject.SetActive(false);
    }

    public void SetFaceDown(bool instant = false)
    {
        isMatched = false;
        if (instant)
        {
            // Instantly reset visuals for restart!
            isFlipped = false;
            StopAllCoroutines();
            isAnimating = false;
            frontImage.gameObject.SetActive(false);
            backImage.gameObject.SetActive(true);
            cardVisual.localRotation = Quaternion.Euler(0, 0, 0);
            if (group != null) group.alpha = 1;
            gameObject.SetActive(true);
        }
        else
        {
            // During gameplay, animate flip if card is face up
            if (isFlipped && !isAnimating)
                Flip();
        }
    }

    public void SetFaceUp()
    {
        if (!isFlipped && !isAnimating)
        {
            Flip(); // Animate flip to face up
        }
        else
        {
            isFlipped = true;
            frontImage.gameObject.SetActive(true);
            backImage.gameObject.SetActive(false);
            cardVisual.localRotation = Quaternion.Euler(0, 0, 0);
            group.alpha = 1;
        }
    }
}
